#!/bin/bash
# script to Run the integration tests from docker-compose 
cd /compiler-src
./build.sh "BuildDebug"
# Run the server up in the background
mono /compiler-src/src/compiler.api/bin/Debug/compiler.api.exe &
sleep 5
# Now run the tests
mono /compiler-src/packages/NUnit.ConsoleRunner/tools/nunit3-console.exe /compiler-src/tests/compiler.IntegrationTests/bin/Debug/compiler.IntegrationTests.dll
