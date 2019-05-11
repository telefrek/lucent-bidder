########################
# Build solution
########################
FROM telefrek/lucent-builder:2.2 AS build-env
WORKDIR /opt/lucent
LABEL component=lucentbuild
COPY . ./
RUN dotnet restore \
    && dotnet test ./Test/Common/CommonTest.csproj \
    && dotnet publish -c Release

########################
# Create runtime images
########################

FROM telefrek/aspnet-core-ffmpeg:2.2
WORKDIR /opt/lucent
LABEL component=content
COPY --from=build-env /opt/lucent/ContentServer/bin/Release/netcoreapp2.2/publish .
RUN rm -rf appsettings*.json
ENTRYPOINT ["dotnet", "ContentServer.dll"]

FROM telefrek/aspnet-core-ffmpeg:2.2
WORKDIR /opt/lucent
LABEL component=bidder
COPY --from=build-env /opt/lucent/Bidder/bin/Release/netcoreapp2.2/publish .
RUN rm -rf appsettings*.json 
ENV COMPlus_PerfMapEnabled=1
ENV COMPlus_EnableEventLog=1
ENTRYPOINT ["dotnet", "Bidder.dll"]

FROM telefrek/aspnet-core-ffmpeg:2.2
WORKDIR /opt/lucent
LABEL component=orchestrator
COPY --from=build-env /opt/lucent/Orchestration/bin/Release/netcoreapp2.2/publish .
RUN rm -rf appsettings*.json
ENTRYPOINT ["dotnet", "Orchestration.dll"]

FROM telefrek/aspnet-core-ffmpeg:2.2
WORKDIR /opt/lucent
LABEL component=scoring
COPY --from=build-env /opt/lucent/Scoring/bin/Release/netcoreapp2.2/publish .
ENV TARGET_DIRECTORY="/usr/local"
RUN curl -L \
   "https://storage.googleapis.com/tensorflow/libtensorflow/libtensorflow-cpu-linux-x86_64-1.9.0.tar.gz" | \
    tar -C $TARGET_DIRECTORY -xz \
    && ldconfig \
    && rm -rf appsettings*.json
ENTRYPOINT ["dotnet", "Scoring.dll"]

FROM build-env