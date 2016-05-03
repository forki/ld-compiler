#!/bin/bash

docker-compose up -d
sleep 10
docker-compose run --rm integrationtests bash /compiler-src/tests/compiler.IntegrationTests/run.sh
result=$?
docker-compose stop
docker-compose logs integrationtests
docker-compose rm -vf
exit $result
