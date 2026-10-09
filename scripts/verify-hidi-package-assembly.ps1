# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

<#
.SYNOPSIS
Verifies the Hidi tool package contains the exact Microsoft-signed staging DLL.
.DESCRIPTION
Run after tool packing and before NuGet signing. A signed NuGet container does
not prove that its assembly payload retained the ESRP signature.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$PackagePath,
    [Parameter(Mandatory = $true)]
    [string]$SignedAssemblyPath
)

$ErrorActionPreference = 'Stop'

foreach ($path in @($PackagePath, $SignedAssemblyPath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Missing Hidi signing verification input: $path"
    }
}

$temporaryDirectory = Join-Path ([IO.Path]::GetTempPath()) ([guid]::NewGuid().ToString())
New-Item -ItemType Directory -Path $temporaryDirectory | Out-Null
$packagedAssembly = Join-Path $temporaryDirectory 'Microsoft.OpenApi.Hidi.dll'
$archive = $null
try {
    $archive = [IO.Compression.ZipFile]::OpenRead((Resolve-Path -LiteralPath $PackagePath).Path)
    $assemblies = @($archive.Entries | Where-Object { $_.Name -ieq 'Microsoft.OpenApi.Hidi.dll' })
    if ($assemblies.Count -ne 1 -or $assemblies[0].FullName -cne 'tools/net8.0/any/Microsoft.OpenApi.Hidi.dll') {
        throw 'Expected exactly one Hidi DLL at tools/net8.0/any/Microsoft.OpenApi.Hidi.dll.'
    }
    [IO.Compression.ZipFileExtensions]::ExtractToFile($assemblies[0], $packagedAssembly)

    $stagingHash = (Get-FileHash -LiteralPath $SignedAssemblyPath -Algorithm SHA256).Hash
    $packageHash = (Get-FileHash -LiteralPath $packagedAssembly -Algorithm SHA256).Hash
    if ($packageHash -cne $stagingHash) {
        throw "Packaged Hidi DLL differs from the signed staging DLL: $packageHash != $stagingHash"
    }
    foreach ($assembly in @($SignedAssemblyPath, $packagedAssembly)) {
        $signature = Get-AuthenticodeSignature -LiteralPath $assembly
        if ($signature.Status -ne 'Valid' -or
            $signature.SignerCertificate.Subject -notmatch '(^|,\s*)O=Microsoft Corporation(,|$)') {
            throw "Hidi DLL must have a valid Microsoft Corporation Authenticode signature: $assembly ($($signature.Status))"
        }
    }
    Write-Host "Verified packaged Hidi DLL: valid Microsoft Authenticode signature; signed staging SHA256=$stagingHash"
}
finally {
    if ($null -ne $archive) { $archive.Dispose() }
    if (Test-Path -LiteralPath $packagedAssembly) {
        Remove-Item -LiteralPath $packagedAssembly -Force
    }
    Remove-Item -LiteralPath $temporaryDirectory -Force
}
