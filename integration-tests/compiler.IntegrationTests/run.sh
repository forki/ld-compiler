#!/bin/bash
# script to Run the integration tests from docker-compose 
cd /compiler-src
./build.sh "BuildDebugIntegrationTestOnly"
# Run the server up in the background
sleep 5
# Now run the tests
mono /compiler-src/packages/NUnit.ConsoleRunner/tools/nunit3-console.exe /compiler-src/integration-tests/compiler.IntegrationTests/bin/Debug/compiler.IntegrationTests.dll
