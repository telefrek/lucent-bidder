#!/bin/bash
set -e
IMAGE_TAG=$(date +%s)
docker pull microsoft/dotnet
docker pull microsoft/dotnet:aspnetcore-runtime

echo 'Building portal'
docker build -t telefrek/lucent-portal:$IMAGE_TAG -f Dockerfile.portal .
docker tag telefrek/lucent-portal:$IMAGE_TAG telefrek/lucent-portal:${LUCENT_VERSION:-alpha}

echo 'Publishing portal'
docker push telefrek/lucent-portal:${LUCENT_VERSION:-alpha}

echo 'Building bidder'
docker build -t telefrek/lucent-bidder:$IMAGE_TAG -f Dockerfile.bidder .
docker tag telefrek/lucent-bidder:$IMAGE_TAG telefrek/lucent-bidder:${LUCENT_VERSION:-alpha}

echo 'Publishing bidder'
docker push telefrek/lucent-bidder:${LUCENT_VERSION:-alpha}

echo 'Updating deployment'
kubectl -n lucent-bidder delete -f lucent.yaml --ignore-not-found && kubectl -n lucent-bidder create -f lucent.yaml