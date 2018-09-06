FROM microsoft/dotnet
ENV TENSOR_VERSION=1.9.0
RUN TARGET_DIRECTORY="/usr/local" && curl -L \
   "https://storage.googleapis.com/tensorflow/libtensorflow/libtensorflow-cpu-linux-x86_64-${TENSOR_VERSION}.tar.gz" | tar -C $TARGET_DIRECTORY -xz \
   && ldconfig
