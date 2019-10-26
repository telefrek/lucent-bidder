#!/bin/bash
set -xe
IMAGE_TAG=${2:-alpha}
LUCENT_VERSION="$(cat VERSION).$IMAGE_TAG"

echo 'Pulling latest dotnet images'
docker pull telefrek/lucent-builder:3.0
docker pull telefrek/aspnet-core-ffmpeg:3.0

docker image prune -f
[[ ! -z "$(docker ps -aq)" ]] && docker rm -vf $(docker ps -qa)

[[ ! -z "$(docker images | grep 'telefrek' | grep -v 'builder' | grep -v 'ffmpeg')" ]] && docker rmi -f $(docker images | grep 'telefrek' | grep -v 'builder' | grep -v 'ffmpeg' | awk '{print $3}')

echo 'Building images'
docker build --rm=false -t telefrek/lucent-build:$IMAGE_TAG . 

sleep 5

docker tag $(docker ps -a -f "label=component=bidder" -f "status=exited" --format "{{.Image}}") telefrek/lucent-bidder:$LUCENT_VERSION
docker tag telefrek/lucent-bidder:$LUCENT_VERSION telefrek/lucent-bidder:latest

docker tag $(docker ps -a -f "label=component=orchestrator" -f "status=exited" --format "{{.Image}}") telefrek/lucent-orchestrator:$LUCENT_VERSION
docker tag telefrek/lucent-orchestrator:$LUCENT_VERSION telefrek/lucent-orchestrator:latest

echo 'Pushing images'

if [ "${1:-bidder}" == "bidder" ]; then
docker push telefrek/lucent-bidder:$LUCENT_VERSION
docker push telefrek/lucent-bidder:latest
fi

if [ "${1:-orchestrator}" == "orchestrator" ]; then
docker push telefrek/lucent-orchestrator:$LUCENT_VERSION
docker push telefrek/lucent-orchestrator:latest
fi

echo 'Cleanup local docker'

docker rm -vf $(docker ps -aq)

[[ ! -z "$(docker images | grep 'telefrek' | grep -v 'builder' | grep -v 'ffmpeg')" ]] && docker rmi -f $(docker images | grep 'telefrek' | grep -v 'builder' | grep -v 'ffmpeg' | awk '{print $3}')

docker image prune -f
