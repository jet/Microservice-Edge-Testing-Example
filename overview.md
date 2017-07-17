# Microservice Edge Interaction Architecture Demo

This repo is meant to be a sample implementation of a proposed pattern for
architecting the edges between microservices.

Also note that this project is intentionally simplified to allow for more focus
on the patterns rather than the actual logic of the code.

## Producer

The producer in this solution is a microservice that is in charge of storing and
updating the current state for different products (which are referred to by
unique identifiers called "sku"s). This producer is implementing the
distributable edge and edge fake pattern, and is the main focus of the repo.

### Types

A good place to start is in `Producer.Domain.Types`. This file defines any types
that both the producer and the consumer of the service will care about.
Obviously, this means the types that the producer expects for requests, and the
return types. Also, this contains the signatures for all the functions the
producer will implement. Since the consumer is no longer directly hitting a REST
API, kafka broker, or another protocol, this means that every function of the
producer is explicitly defined (and inherently documented) here.

### Logic

The actual producer starts implementing things in `Producer.Logic`. This logic
is isolated in its own project for a few resons. Firstly, the ease of now
targeting these small complicated functions with unit tests, since these
functions are probably what you would want to test most thoroughly. The second
is that while the logic primarily lives in the producer, it could also be called
by the mocked version of the producer that is shipped to clients. This is where
any complicated business logic lives. In this scenario, the logic functions take
in all the information they need, and emit a result which contains any results,
or what stateful changes need to be made. If you do need to actually make
stateful reads or writes in business logic, it would be imperative that these
functions would have their dependencies injected rather than directly requiring
them since these stateful operations might be reading from a real DB/service or
a mocked one.

### Controllers

In `Producer.Server`, the file `Controller.fs` contains the controllers for all
endpoints that actually have control over all that endpoints operations. For
example, for `HandleUpdateQuantity` it calls the buisiness logic to figure out
which action to take, and if it is determined an item needs to be updated, it
handles that.

### Producer production style wire up

In `Program.fs`, the actual microservice is initialized, simply listening on
rest endpoints calling the controllers, or listening on a kafka topic for
messages and passing them along. Since everything is depenency injected in
controllers & buisiness logic, this is also where the database / upstream
services would have to be injected

## Databases / Upstream Services

For every Database / Upstream service, the api to interact with it is definied
in the domain types. This is because depending on if you are running integration
or unit tests, and if you are running code in a fake, in production, or for
testing, you might have to mock out these databases/services. There are two
simple passthrough classes defined, each implementing this interface for a
database. Also, the mocked database is purposefully seeded with different data
to simulate a dev and QA/prod environment with different data. These should be
easily reinitializable since a new mock db/upstream will likely be needed for
every test.

## Producer Edge

Since we now have wired up the producer completely, we need a managed ay for
clients to access the producer the `ProducerEdge` simply implements the client
facing interface defined in our domain.types, and passes whatever it was given
over the correct protocols for the server to recieve them. This also means that
everything must be abstracted so that the client sees things in a protocol
agnostic way, for example HTTP turns into just an async response, so the client
never has to deal with whatever internal protocol is used for the client ->
producer communication.

## Producer Fake Edge

Now, since we are giving the client a way to talk to the real producer, the
interface is well definied, and we have abstracted out our business logic, we
can easily ship another edge to them that has a self contained version of the
producer. The `ProducerEdgeFake` does just that. In this intelligent fake we are
shipping, we reimplement the controllers for two reasons. One is to show that
this can be done due to the possibility of parts of your system being so complex
they can't be simply included and called directly, and must instead be mocked
out. Also, this gives us more ways to hook into the internal behavior, and can
allow us to tweak that behavior based on certain conditions. For example, here
we have a mutable state inside the class representing if the database is
down/unreachable. Normally this would not be testable, but here we expose a
method to allow the client to ask the mock to simulate that failure condition,
and the mock will then respond as if it were the production instance atctually
having that failure state. If your client has no such failure states you want to
expose, and if it is simple enough, you could directly call the controllers and
have a triviially simple fake edge. This is actually done in a third edge,
called `DirectEdge`. This is only used to test the real producer behavior
without having to go over tricky protocols to test like Kafka

## Producer Testing

In the producer testing folder, we have two types of tests. Unit tests, which
simply call the core bits of logic in isolation as we mentioned when looking
over the business logic module, and Edge Tests, which call the functions exposed
on the edge, and check responses to see if the entire system is working together
correctly. For the smaller targeted unit tests, these are exactly what you would
expect. However, for the fake and for the producer, we have to take extra care
to make sure we didn't introduce any differences between them when we created
the fake. In `Helpers.fs` you can see a simple function that is used in all the
edge tests, which simply takes a testing function, and calls it with both the
DirectEdge and the ProducerEdgeFake, so if either of them have any different
behavior the tests will fail. Also, this reinitializes the fake db every test so
you are always starting from a known state independent of the other tests

## Client

The client is a simple consumer of the producer microservice, which decides when
quantities or prices of items might need to change. Nothing is too novel on the
client side. It simply initializes the real edge, then runs whatever it would
normally run in production. For unit testing, it simply uses the faked edge to
allow for more stable tests and an easy way to bypass the complications testing
against a live kafka install.
