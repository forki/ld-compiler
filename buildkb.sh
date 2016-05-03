#!/usr/bin/env bash
GIT_REPO_URL=$1

docker-compose stop
docker-compose rm -f
docker-compose up -d
sleep 10
echo "Started building the knowledge base from $GIT_REPO_URL"
curl -XPOST "http://localhost:8083/compile?repoUrl=$GIT_REPO_URL"
echo "Finished"
