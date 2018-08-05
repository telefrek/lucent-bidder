#!/bin/bash

set -xe

# Setup environment variables
SUBSCRIPTION_ID=
DNS_PREFIX=lucent-dev
AZURE_REGION=westus2

source ./common.sh

info 'Starting deployment'
echo

# print the account list
az account list -o table

echo
echo -n "Please choose a subscription id: "
read SUBSCRIPTION_ID
echo

if [ -z "$SUBSCRIPTION_ID" ]; then
    error 'Missing subscription id'
    exit 1
fi

info 'Running engine generation'
acs-engine generate kubernetes.json

info 'Deploying cluster to Azure'
acs-engine deploy --subscription-id $SUBSCRIPTION_ID --location $AZURE_REGION --api-model _output/$DNS_PREFIX/apimodel.json --force-overwrite

info 'Setting up kubectl'
export KUBECONFIG=`pwd`_output/$DNS_PREFIX/kubeconfig/kubeconfig.westus2.json

info 'Checking cluster information'
kubectl cluster-info
echo

info 'Setting up admin account'
kubectl create -f create_admin_role.yaml
echo

info 'Admin Token: '
warn $(kubectl -n kube-system describe secret $(kubectl -n kube-system get secret | grep admin-user | awk '{print $1}') | grep token: | awk '{print $2}')

info 'Waiting for Grafana'

until [ ! -z "$(kubectl get secret dashboard-grafana --ignore-not-found)" ]; do
    echo -n '.'
    sleep 5
done
echo

info 'Getting Grafana Credentials'

info 'User Credentials: ' 
warn $(kubectl get secret dashboard-grafana -o jsonpath="{.data.grafana-admin-user}" | base64 --decode)
info 'Password: '
warn $(kubectl get secret dashboard-grafana -o jsonpath="{.data.grafana-admin-password}" | base64 --decode)

info 'Waiting for tiller to come up'
until [ "$(kubectl get pods -n kube-system | grep tiller | awk '{print $3}') == 'Running'"]; do
    echo -n '.'
    sleep 5
done

info 'Upgrading to client helm version'
helm init --upgrade

info 'Waiting for tiller to come back up'
until [ "$(kubectl get pods -n kube-system | grep tiller | awk '{print $3}') == 'Running'"]; do
    echo -n '.'
    sleep 5
done


cd charts
info 'Installing lucent-bid into cluster'
./install_lucentbid.sh

info 'Deployment finished!'