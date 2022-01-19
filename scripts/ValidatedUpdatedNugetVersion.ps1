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

$versionProps = ".\tool\versioning.props"
$packageName = "Microsoft.OpenApi.OData"

[xml]$xmlDoc = Get-Content $versionProps

# Get the project major, minor and patch version numbers
$majorVersion = $xmlDoc.Project.PropertyGroup.VersionMajor.InnerText
$minorVersion = $xmlDoc.Project.PropertyGroup.VersionMinor.InnerText
$patchVersion = $xmlDoc.Project.PropertyGroup.VersionBuild.InnerText

[int]$currentProjectVersion = $majorVersion + $minorVersion + $patchVersion

# API is case-sensitive
$packageName = $packageName.ToLower()
$url = "https://api.nuget.org/v3/registration3/$packageName/index.json"

# Call the NuGet API for the package and get the current published version.
$nugetIndex = Invoke-RestMethod -Uri $url -Method Get
$currentPublishedVersion = $nugetIndex.items[0].upper

# Convert the published version into a number for comparison e.g. "1.0.9" -> 109
[int]$currentPublishedVersion = $currentPublishedVersion -replace "\."

# Validate that the version number has been updated.
if ($currentProjectVersion -le $currentPublishedVersion) {
    Write-Error "The project version in versioning.props file ($currentProjectVersion) `
    has not been bumped up. The current published version is $currentPublishedVersion. `
    Please increment the current project version."
}
else {
    Write-Host "Validated that the version has been updated from $currentPublishedVersion to $currentProjectVersion" -ForegroundColor Green
}
