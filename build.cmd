SET VERSION=0.9.1
SET PROJ=%~dp0src\Microsoft.OpenAPI.OData.Reader.csproj\Microsoft.OpenAPI.OData.Reader.csproj.csproj 
msbuild %PROJ% /t:restore /p:Configuration=Release
msbuild %PROJ% /t:build /p:Configuration=Release
msbuild %PROJ% /t:pack /p:Configuration=Release;PackageOutputPath=%~dp0artifacts;Version=%VERSION%
