#!/bin/bash
set -e

if [ "${1:-infra}" == "infra" ]; then
    echo 'Setting up helm'

    # setup roles for kubernetes
    kubectl update -f create_admin_role.yaml

    # add the storage class
    kubectl create -f ./setup/content_file_store_sc.yaml -n lucent

    # Helm and nginx ingress
    helm init --tiller-namespace lucent --service-account helm
    helm repo update charts/stable
    sleep 10
    helm install stable/nginx-ingress --name ingress --namespace kube-system --tiller-namespace lucent

    helm upgrade --install --tiller-namespace=lucent --namespace=lucent cassandra ./charts/cassandra
    helm upgrade --install --tiller-namespace=lucent --namespace=lucent rabbitmq ./charts/rabbitmq-ha
fi

if [ "${1:-ldap}" == "monitoring" ]; then
    # setup monitoring
    helm upgrade --install --tiller-namespace=lucent --namespace=lucent prometheus stable/prometheus-operator -f ${2-.}/monitoring.yaml
fi

if [ "${1:-ldap}" == "ldap" ]; then
    helm upgrade --install --tiller-namespace=lucent --namespace=lucent openldap ./charts/openldap
    echo 'ldap credentials:'
    kubectl get secret --namespace lucent openldap-secret -o jsonpath="{.data.LDAP_ADMIN_PASSWORD}" | base64 --decode; echo
fi

if [ "${1:-portal}" == "portal" ]; then
    helm upgrade --install --tiller-namespace=lucent --namespace=lucent portal ./charts/portal/ -f ${2-.}/portal.yaml
fi

if [ "${1:-bidder}" == "bidder" ]; then
    helm upgrade --install --tiller-namespace=lucent --namespace=lucent bidder ./charts/bidder/ -f ${2-.}/bidder.yaml
fi

if [ "${1:-orchestrator}" == "orchestrator" ]; then
    helm upgrade --install --tiller-namespace=lucent --namespace=lucent orchestrator ./charts/orchestrator/ -f ${2-.}/orchestrator.yaml
fi

if [ "${1:-contenta}" == "content" ]; then
    helm upgrade --install --tiller-namespace=lucent --namespace=lucent content ./charts/cntent/ -f ${2-.}/content.yaml
fi