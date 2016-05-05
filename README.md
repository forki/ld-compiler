Coming soon...

# Building and running unit tests

To build the project and run unit tests:
```
./build.sh
```

# Running integration tests
An integration test environment in created using docker-compose.  The integration tests are then run against this transient environment, which is destroyed after every run.

First you must build the docker image:
```
docker build -t nice/ld-compiler .
```
Now run the following script:
```
./runIntegrationTests.sh
```


