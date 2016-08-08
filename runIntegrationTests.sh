#!/bin/bash

docker-compose -f docker-compose.test.yaml up -d
sleep 10
docker exec -it ldcompiler_integrationtests_1 bash /compiler-src/tests/compiler.IntegrationTests/run.sh
result=$?
docker-compose -f docker-compose.test.yaml stop
if [ $result -ne 0 ]; then
   echo "Tests failed, showing logs from compiler container"
   docker logs ldcompiler_compiler_1
fi
docker-compose -f docker-compose.test.yaml rm -vf
exit $result
