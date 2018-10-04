#!/bin/bash
set -xe
IMAGE_TAG=$(date '+%Y%m%d%H%M%S')
LUCENT_VERSION="$(cat VERSION).$IMAGE_TAG"

echo 'Pulling latest dotnet images'
docker pull microsoft/dotnet
docker pull microsoft/dotnet:aspnetcore-runtime

docker image prune -f
[[ ! -z "$(docker ps -aq)" ]] && docker rm -vf $(docker ps -qa)

[[ ! -z "$(docker images | grep 'telefrek')" ]] && docker rmi -f $(docker images | grep 'telefrek' | awk '{print $3}')

echo 'Building images'
docker build --rm=false -t telefrek/lucent-build:$IMAGE_TAG . 

sleep 2

echo 'Tagging images'
docker tag $(docker ps -a -f "label=component=portal" -f "status=exited" --format "{{.Image}}") telefrek/lucent-portal:$LUCENT_VERSION
docker tag telefrek/lucent-portal:$LUCENT_VERSION telefrek/lucent-portal:latest

docker tag $(docker ps -a -f "label=component=bidder" -f "status=exited" --format "{{.Image}}") telefrek/lucent-bidder:$LUCENT_VERSION
docker tag telefrek/lucent-bidder:$LUCENT_VERSION telefrek/lucent-bidder:latest

docker tag $(docker ps -a -f "label=component=orchestrator" -f "status=exited" --format "{{.Image}}") telefrek/lucent-orchestrator:$LUCENT_VERSION
docker tag telefrek/lucent-orchestrator:$LUCENT_VERSION telefrek/lucent-orchestrator:latest

docker tag $(docker ps -a -f "label=component=content" -f "status=exited" --format "{{.Image}}") telefrek/lucent-content:$LUCENT_VERSION
docker tag telefrek/lucent-content:$LUCENT_VERSION telefrek/lucent-content:latest

docker tag $(docker ps -a -f "label=component=scoring" -f "status=exited" --format "{{.Image}}") telefrek/lucent-scoring:$LUCENT_VERSION
docker tag telefrek/lucent-scoring:$LUCENT_VERSION telefrek/lucent-scoring:latest

echo 'Pushing images'

docker push telefrek/lucent-portal:$LUCENT_VERSION
docker push telefrek/lucent-portal:latest

docker push telefrek/lucent-bidder:$LUCENT_VERSION
docker push telefrek/lucent-bidder:latest

docker push telefrek/lucent-orchestrator:$LUCENT_VERSION
docker push telefrek/lucent-orchestrator:latest

docker push telefrek/lucent-content:$LUCENT_VERSION
docker push telefrek/lucent-content:latest

echo 'Cleanup local docker'

docker rm -vf $(docker ps -aq)

[[ ! -z "$(docker images | grep 'telefrek')" ]] && docker rmi -f $(docker images | grep 'telefrek' | awk '{print $3}')

docker image prune -f