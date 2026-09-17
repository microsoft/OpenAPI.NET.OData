FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /app/hidi

COPY Directory.Build.props Build.props README.md ./
COPY src/Build.props src/Build.props
COPY tool/Microsoft.OpenApi.OData.snk tool/Microsoft.OpenApi.OData.snk
COPY tool/Microsoft.OpenApi.Hidi.snk tool/Microsoft.OpenApi.Hidi.snk
COPY src/Microsoft.OpenApi.OData.Reader src/Microsoft.OpenApi.OData.Reader
COPY src/Microsoft.OpenApi.Hidi src/Microsoft.OpenApi.Hidi
RUN dotnet publish ./src/Microsoft.OpenApi.Hidi/Microsoft.OpenApi.Hidi.csproj -c Release

FROM mcr.microsoft.com/dotnet/runtime:8.0-jammy-chiseled AS runtime
WORKDIR /app

COPY --from=build-env /app/hidi/src/Microsoft.OpenApi.Hidi/bin/Release/net8.0 ./

VOLUME /app/output
VOLUME /app/openapi.yml
VOLUME /app/api.csdl
VOLUME /app/collection.json
ENV HIDI_CONTAINER=true DOTNET_TieredPGO=1 DOTNET_TC_QuickJitForLoops=1
ENTRYPOINT ["dotnet", "Microsoft.OpenApi.Hidi.dll"]
LABEL description="# Welcome to Hidi \
To start transforming OpenAPI documents, see https://github.com/microsoft/OpenAPI.NET.OData/tree/main/src/Microsoft.OpenApi.Hidi. \
Source: https://github.com/microsoft/OpenAPI.NET.OData/blob/main/Dockerfile"
