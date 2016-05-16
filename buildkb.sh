#!/usr/bin/env bash
GIT_REPO_URL=$1

docker-compose -f docker-compose.test.yaml stop
docker-compose -f docker-compose.test.yaml rm -f
docker-compose -f docker-compose.test.yaml up -d
sleep 10
echo "Started building the knowledge base from $GIT_REPO_URL"

curl -XPOST "http://localhost:8081/compile?repoUrl=$GIT_REPO_URL"
sleep 2
while
  echo "Still Running"
  sleep 2
  echo "Checking status..."
  $(curl --silent "http://localhost:8081/status" 2>&1 | grep --silent "Running")
do :; done
echo "Finished"
