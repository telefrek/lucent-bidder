FROM microsoft/dotnet:latest AS build-env
WORKDIR /

# Copy everything else and build
COPY . ./
RUN dotnet restore
RUN dotnet publish -c Release

# Build runtime image
FROM microsoft/dotnet:aspnetcore-runtime
WORKDIR /opt/lucent
COPY --from=build-env /Portal/bin/Release/netcoreapp2.1/publish .
ENTRYPOINT ["dotnet", "Portal.dll"]