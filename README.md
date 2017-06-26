# Edge Testing Demo Application

This repo intends to be a simplified example of using the proposed edge client pattern
for testing microservices and defining how communication over an edge will work

# Setup

## Databse

The initial databse is seeded from a json file, located at `edge-demo\Producer.Server\bin\Debug\database.json`,
and this file will be modified as the database is modified simulating a real mutable DB.

To initialize it, or to reset the database if you do not like your mutations,
copy the supplied `database.json` from this directory to the specified location.