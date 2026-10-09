FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /app

COPY ./src ./hidi/src
COPY ./Directory.Build.props ./hidi/Directory.Build.props
COPY ./Build.props ./hidi/Build.props
COPY ./build.root ./hidi/build.root
COPY ./global.json ./hidi/global.json
COPY ./tool/Microsoft.OpenApi.OData.public.snk ./hidi/tool/Microsoft.OpenApi.OData.public.snk
COPY ./tool/Microsoft.OpenApi.Hidi.public.snk ./hidi/tool/Microsoft.OpenApi.Hidi.public.snk
COPY ./README.md ./hidi/README.md
WORKDIR /app/hidi
# Official CI supplies the central feed config as a secret; local builds use default NuGet sources.
RUN --mount=type=secret,id=nuget_config,target=/app/hidi/NuGet.Config \
    dotnet publish ./src/Microsoft.OpenApi.Hidi/Microsoft.OpenApi.Hidi.csproj -c Release -o /app/publish -p:GeneratePackageOnBuild=false -p:HidiPublicSignBuild=true

FROM mcr.microsoft.com/dotnet/runtime:8.0-jammy-chiseled AS runtime
WORKDIR /app

COPY --from=build-env /app/publish ./

VOLUME /app/output
VOLUME /app/openapi.yml
VOLUME /app/api.csdl
VOLUME /app/collection.json
ENV HIDI_CONTAINER=true DOTNET_TieredPGO=1 DOTNET_TC_QuickJitForLoops=1
ENTRYPOINT ["dotnet", "Microsoft.OpenApi.Hidi.dll"]
LABEL description="# Welcome to Hidi \
To start transforming OpenAPI documents checkout [the getting started documentation](https://github.com/microsoft/OpenAPI.NET.OData/tree/support/v2/src/Microsoft.OpenApi.Hidi)  \
[Source dockerfile](https://github.com/microsoft/OpenAPI.NET.OData/blob/support/v2/Dockerfile)"
LABEL org.opencontainers.image.source="https://github.com/microsoft/OpenAPI.NET.OData"
