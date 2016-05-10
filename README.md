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


# Building a knowledge base for debugging
There is a buildkb script that takes a git repo url and builds a knowledge base and leaves it up for inspecting/debugging. for example

```
./buildkb.sh https://github.com/nhsevidence/ld-dummy-content
```

Now you should be able to inspect the search index via:
```
curl localhost:9200/_search?pretty
```

and the resourceapi like so:
```
curl localhost:8082/resource/qs1/st1
```
