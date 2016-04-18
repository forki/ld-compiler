#!/bin/bash
# script to Run the integration tests from docker-compose 
sleep 10
mono /nunit/packages/NUnit.ConsoleRunner/tools/nunit3-console.exe /tests/bin/Release/publish.IntegrationTests.dll
