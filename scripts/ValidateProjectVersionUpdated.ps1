# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

<#
.Synopsis
    Validates that the package version has been updated
.Description
    Validates that the package version has been updated by comparing the version
    specified in the project file with the latest package version published on
    NuGet. If the version has not been updated, the script will fail and indicate
    that the project version neeeds to be updated.
#>
$packageName = "Microsoft.OpenApi.OData"
$csprojPath = Join-Path $PSScriptRoot "..\Directory.Build.props"

[XML]$csprojFile = Get-Content $csprojPath
$versionNode = Select-Xml $csprojFile -XPath "//Project/PropertyGroup/Version" | Select-Object -ExpandProperty Node
$projectVersion = $versionNode.InnerText

# If <Version> is missing, try <VersionPrefix> + <VersionSuffix>
if ($null -eq $projectVersion)
{
    $versionPrefixNode = Select-Xml $csprojFile -XPath "//Project/PropertyGroup/VersionPrefix" | Select-Object -ExpandProperty Node
    $versionSuffixNode = Select-Xml $csprojFile -XPath "//Project/PropertyGroup/VersionSuffix" | Select-Object -ExpandProperty Node
    $projectVersion = $versionPrefixNode.InnerText + $versionSuffixNode.InnerText
}

# Ensure a valid version exists
if (-not $projectVersion -or $projectVersion -eq "") {
    Write-Error "No valid version found in .csproj file. Please define <Version> or <VersionPrefix> for $packageName."
    Exit 1
}

# Cast the project version string to System.Version
$currentProjectVersion = [System.Management.Automation.SemanticVersion]$projectVersion
$currentMajorVersion = $currentProjectVersion.Major

# API is case-sensitive
$packageName = $packageName.ToLower()
$url = "https://api.nuget.org/v3/registration5-gz-semver2/$packageName/index.json"

# Call the NuGet API for the package and get the current published version.
Try {
    $nugetIndex = Invoke-RestMethod -Uri $url -Method Get -ErrorAction Stop
} Catch {
    if ($_.Exception.Response.StatusCode -eq 404) {
        Write-Host "No package exists. You will probably be publishing $packageName for the first time."
        Exit 0
    }
    Write-Error "Error fetching package details: $_"
    Exit 1
}

# Extract and sort all published versions (handling null/empty cases)
$publishedVersions = $nugetIndex.items | ForEach-Object { [System.Management.Automation.SemanticVersion]$_.upper } | Sort-Object -Descending

if (-not $publishedVersions -or $publishedVersions.Count -eq 0) {
    Write-Host "No previous versions found on NuGet. Proceeding with publish." -ForegroundColor Green
    Exit 0
}

# Find the highest published version within the same major version
$highestPublishedVersionInMajor = ($publishedVersions | Where-Object { $_.Major -eq $currentMajorVersion })

# Handle empty or null major versions list
if (-not $highestPublishedVersionInMajor -or $highestPublishedVersionInMajor.Count -eq 0) {
    Write-Host "No previous versions found for major version $currentMajorVersion. Proceeding with publish." -ForegroundColor Green
    Exit 0
}

# Get the latest version for the current major version
$latestMajorVersion = $highestPublishedVersionInMajor[0]

# Validate that the version number has increased
if ($currentProjectVersion -le $latestMajorVersion) {
    Write-Error "The version in .csproj ($currentProjectVersion) must be greater than the highest published version in the same major ($latestMajorVersion)."
    Exit 1
} else {
    Write-Host "Validated version update: $latestMajorVersion -> $currentProjectVersion" -ForegroundColor Green
}