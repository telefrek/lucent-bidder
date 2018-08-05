#!/bin/sh
set -e

source ./common.sh

# Environment variables
LUCENT_NAMESPACE='lucent-bidder'

# Install the charts
cd charts

info 'Installing LucentBid cluster'

info 'Ensuring Helm Cluster-Role exists'
cat ../create_admin_role.yaml | envsubst | kubectl apply -f -

info 'Ensuring helm installed with correct permissions'
helm init --upgrade --tiller-namespace=$LUCENT_NAMESPACE --tiller-service-account helm

info 'Waiting for tiller to come back up'
until [ "$(kubectl get pods -n $LUCENT_NAMESPACE | grep tiller | awk '{print $3}') == 'Running'" ]; do
    echo -n '.'
    sleep 5
done

info 'Adding OpenLDAP'
helm upgrade --install --tiller-namespace=$LUCENT_NAMESPACE --namespace=$LUCENT_NAMESPACE openldap ./openldap 

info 'Adding RabbitMQ'
helm upgrade --install --tiller-namespace=$LUCENT_NAMESPACE --namespace=$LUCENT_NAMESPACE rabbitmq ./rabbitmq-ha

info 'Adding Cassandra'

info 'Adding Portal'

info 'Adding Bidder'