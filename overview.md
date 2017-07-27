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
producer will implement.

### Logic

The actual producer starts implementing things in `Producer.Logic`. This is a
place for any complicated business logic, computations, or decision making In
this scenario, the logic functions take in all the information they need, and
emit a result which contains any results, or what stateful changes need to be
made.
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
in the domain types. There are two simple passthrough classes defined, each
implementing this interface for a database. Also, the mocked database is
purposefully seeded with different data to simulate a dev and QA/prod
environment with different data.

## Producer Edge

The `ProducerEdge` simply implements the client facing interface defined in our
domain.types, and passes whatever it was given over the correct protocols for
the server to recieve them. This is the libraryThis also means that everything
must be abstracted so that the client sees things in a protocol agnostic way,
for example HTTP turns into just an async response, so the client never has to
deal with whatever internal protocol is used for the client -> producer
communication.

## Client

The client is a simple consumer of the producer microservice, which decides when
quantities or prices of items might need to change. Nothing is too novel on the
client side. It simply initializes the real edge, then runs whatever it would
normally run in production. For unit testing, it simply uses the faked edge to
allow for more stable tests and an easy way to bypass the complications testing
against a live kafka install.

## Producer Fake Edge

Now, since we are giving the client a way to talk to the real producer with a
well defined business interface we can easily ship another edge to the consumer
that has a self contained faked out version of the producer. The
`ProducerEdgeFake` does just that. This behavior does not have to be production
ready or able to handle load, but it should be a mimic of all the producer's
behaviors and subtle nuances. In this case we were able to even include some of
the core business logic from the real producer, but this won't always be
possible.

Also, By reimplementing the controller, we can tweak its behavior to simulate
different states or levels of degraded performance that the producer might
encounter in production. For example, here we implemented methods to simulate
the producer's database being down or unreachable. This allows clients to test
against not just the expected results, but edge cases and errors that the
producer might throw to the client in production.

Also, a simple passthrough edge can be created that calls the controllers of the
producer directly if you want to test it directly. We do this in a third edge,
called `DirectEdge`, calling the producer directly as a way to test the real
implementation's functionality. Both of these also allow for fully testing the
api without full integration tests or trying to simulate kafka brokers or
complicated docker setups.

## Testing

In the producer testing folder, we have two types of tests. Unit tests, which
simply call the core bits of logic in isolation as we mentioned when looking
over the business logic module, and Edge Tests, which call the functions exposed
on the edge, and check responses to see if the entire system is working together
correctly. Also, on every test run, we run the testing function through a
helper, defined in `Helpers.fs`, which runs the same test against the fake and
actual code to make sure the functionality keeps parity. Also, this
reinitializes the fake db every test so you are always starting from a known
state independent of the other tests, fulfilling the requirement for fast
parallelized test running.

The client uses only the producer edge fake for unit tests, which like in the
producer ensures that there is a version of the producer starting from a given
state with a self contained database / external dependencies, with a small
mocked-out codebase that runs entirely in memory on the consumers side. Another
big win that this brings is that since its all self contained on the consumer's
side once they recieve the fake, the consumer never has any dependencies on some
kafka broker or extenal service being up, but due to the defined interface is
aware of all the functionality and can make sure its tests align with how the
producer actually does run.

