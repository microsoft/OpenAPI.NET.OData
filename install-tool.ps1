$ErrorActionPreference = 'Stop'

$projectPath = Join-Path $PSScriptRoot 'src\Microsoft.OpenApi.Hidi\Microsoft.OpenApi.Hidi.csproj'
$version = ([xml](Get-Content $projectPath)).Project.PropertyGroup.Version
$packageDirectory = Join-Path $PSScriptRoot 'artifacts\hidi\nuget'
$packagePath = Join-Path $packageDirectory "Microsoft.OpenApi.Hidi.$version.nupkg"
$toolDirectory = Join-Path $PSScriptRoot 'artifacts\hidi\installed'
$configPath = Join-Path $PSScriptRoot 'tool\hidi-local-nuget.config'

if (-not (Test-Path $packagePath)) {
  throw "Missing $packagePath. Pack the hidi project in Release configuration before installing."
}
if ((Test-Path (Join-Path $toolDirectory 'hidi.exe')) -or (Test-Path (Join-Path $toolDirectory 'hidi'))) {
  dotnet tool uninstall --tool-path $toolDirectory Microsoft.OpenApi.Hidi
  if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}
dotnet tool install --tool-path $toolDirectory --configfile $configPath --add-source $packageDirectory --no-cache --version $version Microsoft.OpenApi.Hidi
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$installedDlls = @(Get-ChildItem $toolDirectory -Force -Recurse -Filter Microsoft.OpenApi.Hidi.dll)
if ($installedDlls.Count -ne 1) { throw 'Expected exactly one installed hidi DLL.' }
$builtDll = Join-Path $PSScriptRoot 'src\Microsoft.OpenApi.Hidi\bin\Release\net8.0\Microsoft.OpenApi.Hidi.dll'
if ((Get-FileHash $installedDlls[0].FullName).Hash -ne (Get-FileHash $builtDll).Hash) {
  throw 'Installed hidi does not match the locally built package.'
}