$latest = Get-ChildItem .\artifacts\Microsoft.OpenApi.Hidi*.nupkg |
    Sort-Object LastWriteTime |
    Select-Object -Last 1

if ($null -eq $latest) {
    throw "No Microsoft.OpenApi.Hidi package was found in .\artifacts."
}

$version = $latest.BaseName -replace '^Microsoft\.OpenApi\.Hidi\.', ''

if (Test-Path -Path .\artifacts\hidi.exe) {
    dotnet tool uninstall --tool-path artifacts Microsoft.OpenApi.Hidi
}

dotnet tool install --tool-path artifacts --add-source .\artifacts\ --version $version Microsoft.OpenApi.Hidi
