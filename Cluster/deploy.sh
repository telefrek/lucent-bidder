#!/bin/bash
set -e
echo 'Pushing images'

if [ "${1:-setup}" == "setup" ]; then
    echo 'Setting up helm'
    kubectl update -f create_admin_role.yaml

    helm init --tiller-namespace lucent --service-account helm
    sleep 10
    helm install stable/nginx-ingress --name ingress --namespace kube-system --tiller-namespace lucent
    helm upgrade --install --tiller-namespace=lucent --namespace=lucent cassandra ./charts/cassandra
    helm upgrade --install --tiller-namespace=lucent --namespace=lucent rabbitmq ./charts/rabbitmq-ha
    helm upgrade --install --tiller-namespace=lucent --namespace=lucent prometheus-operator ./charts/prometheus-operator/
    helm upgrade --install --tiller-namespace=lucent --namespace=lucent prometheus ./charts/kube-prometheus/ -f ${2-.}/monitoring.yaml

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