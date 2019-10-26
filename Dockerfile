########################
# Build solution
########################
FROM telefrek/lucent-builder:3.0 AS build-env
WORKDIR /opt/lucent
LABEL component=lucentbuild
COPY . ./
RUN dotnet restore \
    && dotnet test ./Test/Common/CommonTest.csproj \
    && dotnet publish -c Release

########################
# Create runtime images
########################

FROM telefrek/aspnet-core-ffmpeg:3.0
WORKDIR /opt/lucent
LABEL component=bidder
COPY --from=build-env /opt/lucent/Bidder/bin/Release/netcoreapp3.0/publish .
RUN rm -rf appsettings*.json 
ENV COMPlus_PerfMapEnabled=1
ENV COMPlus_EnableEventLog=1
ENTRYPOINT ["dotnet", "Bidder.dll"]

FROM telefrek/aspnet-core-ffmpeg:3.0
WORKDIR /opt/lucent
LABEL component=orchestrator
COPY --from=build-env /opt/lucent/Orchestration/bin/Release/netcoreapp3.0/publish .
RUN rm -rf appsettings*.json
ENTRYPOINT ["dotnet", "Orchestration.dll"]

FROM build-env