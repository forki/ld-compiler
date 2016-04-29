#!/bin/bash

docker-compose up -d
sleep 10
docker-compose run --rm compiler bash /compiler-src/tests/compiler.IntegrationTests/run.sh
result=$?
docker-compose stop
docker-compose rm -vf
exit $result
