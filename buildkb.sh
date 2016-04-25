#!/usr/bin/env bash
GITUSER=$1
GITPASS=$2

docker-compose stop
docker-compose rm -f
docker-compose up -d
sleep 10
docker exec -it ldcompiler_mimir_1 bash -c "git clone https://$GITUSER:$GITPASS@github.com/nhsevidence/ld-content-test /git && mkdir /artifacts && cd /compiler && mono publish.exe /git/qualitystandards /artifacts"
#docker-compose stop
#docker-compose rm -f
