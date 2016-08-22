#!/bin/bash
docker-compose pull
docker-compose up -d
sleep 10
docker exec ldcompiler_integrationtests bash /compiler-src/integration-tests/compiler.IntegrationTests/run.sh
result=$?
docker-compose stop
if [ $result -ne 0 ]; then
   echo "Tests failed, showing logs from compiler container"
   docker-compose logs compiler
fi
docker-compose rm -vf
exit $result
