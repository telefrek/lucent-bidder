########################
# Build solution
########################
FROM microsoft/dotnet:latest AS build-env
WORKDIR /opt/lucent
LABEL component=lucentbuild
COPY . ./
RUN dotnet restore
RUN dotnet publish -c Release

########################
# Create runtime images
########################
FROM microsoft/dotnet:aspnetcore-runtime
WORKDIR /opt/lucent
LABEL component=portal
COPY --from=build-env /opt/lucent/Portal/bin/Release/netcoreapp2.1/publish .
RUN rm -rf appsettings*.json
ENTRYPOINT ["dotnet", "Portal.dll"]

FROM microsoft/dotnet:aspnetcore-runtime
WORKDIR /opt/lucent
LABEL component=content
COPY --from=build-env /opt/lucent/ContentServer/bin/Release/netcoreapp2.1/publish .
RUN rm -rf appsettings*.json
ENTRYPOINT ["dotnet", "ContentServer.dll"]

FROM microsoft/dotnet:aspnetcore-runtime
WORKDIR /opt/lucent
LABEL component=bidder
COPY --from=build-env /opt/lucent/Bidder/bin/Release/netcoreapp2.1/publish .
RUN rm -rf appsettings*.json
ENTRYPOINT ["dotnet", "Bidder.dll"]

FROM microsoft/dotnet:aspnetcore-runtime
WORKDIR /opt/lucent
LABEL component=orchestrator
COPY --from=build-env /opt/lucent/Orchestration/bin/Release/netcoreapp2.1/publish .
RUN rm -rf appsettings*.json
ENTRYPOINT ["dotnet", "Orchestration.dll"]

FROM microsoft/dotnet:aspnetcore-runtime
WORKDIR /opt/lucent
LABEL component=scoring
COPY --from=build-env /opt/lucent/Scoring/bin/Release/netcoreapp2.1/publish .
ENV TARGET_DIRECTORY="/usr/local"
RUN curl -L \
   "https://storage.googleapis.com/tensorflow/libtensorflow/libtensorflow-cpu-linux-x86_64-1.9.0.tar.gz" | \
    tar -C $TARGET_DIRECTORY -xz \
    && ldconfig \
    && rm -rf appsettings*.json
ENTRYPOINT ["dotnet", "Scoring.dll"]

FROM build-env