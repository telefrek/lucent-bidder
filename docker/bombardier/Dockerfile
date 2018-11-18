FROM debian:stretch-slim
WORKDIR /opt/perf
RUN apt-get update && apt-get install -y wget && \
    wget https://github.com/codesenberg/bombardier/releases/download/v1.2/bombardier-linux-386 && \
    mv bombardier-linux-386 bombardier
    
FROM debian:stretch-slim
WORKDIR /opt/perf
COPY --from=0 /opt/perf/bombardier ./
RUN apt-get update && apt-get install -y --reinstall ca-certificates && chmod a+x bombardier
ENTRYPOINT [ "./bombardier" ]
CMD []