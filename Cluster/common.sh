#!/bin/bash


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