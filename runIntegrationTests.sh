#!/bin/bash

docker-compose up -d
sleep 10
docker-compose run --rm integrationtests bash /compiler-src/tests/compiler.IntegrationTests/run.sh
result=$?
docker-compose stop
if [ $result -ne 0 ]; then
   echo "Tests failed, showing logs from compiler container"
   docker logs ldcompiler_compiler_1
fi
docker-compose rm -vf
exit $result
