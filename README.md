# Edge Testing Demo Application

This repo intends to be a simplified example of using the proposed edge client pattern
for testing microservices and defining how communication over an edge will work.

# Setup

## Database

The initial database is seeded from a json file, located at `edge-demo\Producer.Server\bin\Debug\database.json`,
and this file will be modified as the database is modified simulating a real mutable DB.

To initialize it, or to reset the database if you do not like your mutations,
copy the supplied `database.json` from this directory to the specified location.

## Kafka

Kafka is run through docker, a simple `docker-compose up` should spin up the kafka instance for you.

If you are running this in a windows VM through parallels, make sure to enable nested virtualization in parallels,
and that you allocate the VM 8192 MB of RAM. Less or More than that seems to be unstable.