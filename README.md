# The linked-data compiler for the knowledge base project

This project contains a compiler API for triggering the compilation of a linked data knowledge base.  The inputs are a url to a git repository containing markdown files.  The outputs are an elastic search index and some static html resources (posted to a resource api).

This project is based on the [ProjectScaffold F# template](http://fsprojects.github.io/ProjectScaffold/), uses FAKE for task management and [Paket](http://fsprojects.github.io/Paket/) for packet management.  Please see the project scaffold docs for more information about this setup.

# Requirements
* docker
* docker-compose

# Building and running unit tests

To build the project and run unit tests:
```
docker build .
```

# Running integration tests
An integration test environment in created using docker-compose.  The integration tests are then run against this transient environment, which is destroyed after every run.

First you must build the docker image locally:
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

## Compilation/publication Process
The stages of transformation for each markdown file are as follows (src/compiler/Compile.fs):
* Markdown parsed to in-memory type
* Use pandoc to create static html file 
* Extract and resolve annotations
* Converted to RDF
* serialised to TTL and write to disk
* added ttl file to graph db 

The publication process which follows on from compilation (src/compiler/Publish.fs):
* Find all statements in graph db
* compress to json-ld
* upload to elastic search
* collect and POST all static html resources to resource api
* 


