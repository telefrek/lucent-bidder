#!/bin/sh
TF_TYPE="cpu"
OS="linux"
TENSOR_VERSION="1.9.0"
TARGET_DIRECTORY="/usr/local"
curl -L \
   "https://storage.googleapis.com/tensorflow/libtensorflow/libtensorflow-${TF_TYPE}-${OS}-x86_64-${TENSOR_VERSION}.tar.gz" |
   sudo tar -C $TARGET_DIRECTORY -xz
sudo ldconfig
