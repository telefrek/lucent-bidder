#!/bin/bash

set -e

# Setup environment variables
SUBSCRIPTION_ID=
DNS_PREFIX=lucent-dev
AZURE_REGION=westus2

# Function blocks
function info(){
    echo -e "\033[0;36m$(date +%H:%M:%s) ${1:-unknown}\033[0m"
}

function warn(){
    echo -e "\033[1;33m$(date +%H:%M:%s) ${1:-unknown}\033[0m"
}

function error(){
    echo -e "\033[0;31m$(date +%H:%M:%s) ${1:-unknown}\033[0m"
}

info 'Deleting the cluster'
az group delete -n $DNS_PREFIX -y

info 'Removing local files'
rm -rf _output
rm -rf translations

info 'Done'