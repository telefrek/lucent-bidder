#!/bin/sh
FROM_FILE=${1:-jwt}
kubectl create configmap lucent-keys -n lucent --from-file=$FROM_FILE