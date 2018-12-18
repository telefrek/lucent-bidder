FROM debian:stretch-slim
WORKDIR /opt/perf
RUN apt-get update && apt-get install -y wget && \
    wget https://github.com/codesenberg/bombardier/releases/download/v1.2/bombardier-linux-386 && \
    mv bombardier-linux-386 bombardier
    
FROM debian:stretch-slim
WORKDIR /opt/perf
COPY --from=0 /opt/perf/bombardier ./
COPY ./request.json.gz ./
RUN apt-get update && apt-get install -y --reinstall ca-certificates && chmod a+x bombardier
ENTRYPOINT [ "./bombardier", "-m", "POST", "-f", "request.json.gz", "-H", "'Content-Type: application/json'", "-H", "'Content-Encoding: gzip'", "-d", "180s", "-l", "https://west.lucentbid.com/v1/bidder" ]