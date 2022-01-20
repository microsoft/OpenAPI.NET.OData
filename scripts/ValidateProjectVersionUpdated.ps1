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

.Parameter projectVersion
    Specifies the current project version. The project version currently is maintained 
    in the versioning.props file
#>

Param(
	[parameter(Mandatory = $true)]
	[string]$projectVersion
)

$packageName = "Microsoft.OpenApi.OData"

# Convert the project semantic version into a number e.g. "1.0.9" -> 109
[int]$currentProjectVersionNumber = $projectVersion -replace "\."

# API is case-sensitive
$packageName = $packageName.ToLower()
$url = "https://api.nuget.org/v3/registration5-semver1/$packageName/index.json"

# Call the NuGet API for the package and get the current published version.
$nugetIndex = Invoke-RestMethod -Uri $url -Method Get
$currentPublishedVersion = $nugetIndex.items[0].upper

# Convert the published version into a number e.g. "1.0.9" -> 109
[int]$currentPublishedVersionNumber = $currentPublishedVersion -replace "\."

# Validate that the version number has been updated.
if ($currentProjectVersionNumber -le $currentPublishedVersionNumber) {
    Write-Error "The project version in versioning.props file ($currentProjectVersion) `
    has not been bumped up. The current published version is $currentPublishedVersion. `
    Please increment the current project version."
}
else {
    Write-Host "Validated that the version has been updated from $currentPublishedVersion to $currentProjectVersion" -ForegroundColor Green
}
