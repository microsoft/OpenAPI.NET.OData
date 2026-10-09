# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeDiscovery {
    $cases = @(
        @{ Name = 'matching signed DLL before NuGet signing' }
        @{ Name = 'different payload bytes'; Payload = 'unsigned-build'; ExpectedError = '*differs from the signed staging DLL*' }
        @{ Name = 'missing tool DLL'; Entries = @(); ExpectedError = '*exactly one Hidi DLL*' }
        @{ Name = 'wrong tool location'; Entries = @('lib/net8.0/Microsoft.OpenApi.Hidi.dll'); ExpectedError = '*exactly one Hidi DLL*' }
        @{ Name = 'duplicate tool DLL'; Entries = @('tools/net8.0/any/Microsoft.OpenApi.Hidi.dll', 'tools/net8.0/any/Microsoft.OpenApi.Hidi.dll'); ExpectedError = '*exactly one Hidi DLL*' }
        @{ Name = 'extra tool DLL elsewhere'; Entries = @('tools/net8.0/any/Microsoft.OpenApi.Hidi.dll', 'lib/net8.0/Microsoft.OpenApi.Hidi.dll'); ExpectedError = '*exactly one Hidi DLL*' }
        @{ Name = 'wrong entry casing'; Entries = @('tools/net8.0/any/microsoft.openapi.hidi.dll'); ExpectedError = '*exactly one Hidi DLL*' }
        @{ Name = 'unsigned package payload'; PackageStatus = 'NotSigned'; ExpectedError = '*valid Microsoft Corporation Authenticode signature*' }
        @{ Name = 'damaged package signature'; PackageStatus = 'HashMismatch'; ExpectedError = '*valid Microsoft Corporation Authenticode signature*' }
        @{ Name = 'unverifiable package signature'; PackageStatus = 'UnknownError'; ExpectedError = '*valid Microsoft Corporation Authenticode signature*' }
        @{ Name = 'unsigned staging DLL'; StagingStatus = 'NotSigned'; ExpectedError = '*valid Microsoft Corporation Authenticode signature*' }
        @{ Name = 'damaged staging signature'; StagingStatus = 'HashMismatch'; ExpectedError = '*valid Microsoft Corporation Authenticode signature*' }
        @{ Name = 'non-Microsoft payload signer'; PackageSubject = 'CN=Example, O=Example'; ExpectedError = '*valid Microsoft Corporation Authenticode signature*' }
        @{ Name = 'non-Microsoft staging signer'; StagingSubject = 'CN=Example, O=Example'; ExpectedError = '*valid Microsoft Corporation Authenticode signature*' }
        @{ Name = 'misleading Microsoft signer name'; PackageSubject = 'CN=Microsoft Corporation, O=Example'; ExpectedError = '*valid Microsoft Corporation Authenticode signature*' }
        @{ Name = 'missing payload certificate'; MissingPackageCertificate = $true; ExpectedError = '*valid Microsoft Corporation Authenticode signature*' }
        @{ Name = 'missing staging certificate'; MissingStagingCertificate = $true; ExpectedError = '*valid Microsoft Corporation Authenticode signature*' }
        @{ Name = 'missing package'; MissingPackage = $true; ExpectedError = '*Missing Hidi signing verification input*' }
        @{ Name = 'missing staging DLL'; MissingStaging = $true; ExpectedError = '*Missing Hidi signing verification input*' }
        @{ Name = 'invalid package archive'; InvalidArchive = $true; ExpectedError = '*' }
        @{ Name = 'signature verification error'; SignatureError = $true; ExpectedError = '*Signature verification unavailable*' }
    )
}

Describe 'Hidi package signed-assembly provenance' {
    BeforeAll {
        $helper = Join-Path $PSScriptRoot '..\..\scripts\verify-hidi-package-assembly.ps1'
    }

    It '<Name>' -ForEach $cases {
        $testCase = $_
        $directory = Join-Path $TestDrive ([guid]::NewGuid().ToString())
        New-Item -ItemType Directory -Path $directory | Out-Null
        $package = Join-Path $directory 'Microsoft.OpenApi.Hidi.3.10.2.nupkg'
        $staging = Join-Path $directory 'Microsoft.OpenApi.Hidi.dll'
        if (-not $MissingStaging) { [IO.File]::WriteAllText($staging, 'signed-staging') }
        if (-not $MissingPackage) {
            if ($InvalidArchive) {
                [IO.File]::WriteAllText($package, 'not-a-zip')
            }
            else {
                $archive = [IO.Compression.ZipFile]::Open($package, 'Create')
                try {
                    $entryNames = if ($testCase.ContainsKey('Entries')) { $Entries } else { @('tools/net8.0/any/Microsoft.OpenApi.Hidi.dll') }
                    foreach ($name in $entryNames) {
                        $writer = [IO.StreamWriter]::new($archive.CreateEntry($name).Open())
                        try { $writer.Write($(if ($Payload) { $Payload } else { 'signed-staging' })) }
                        finally { $writer.Dispose() }
                    }
                }
                finally { $archive.Dispose() }
            }
        }
        $inspectedPaths = [Collections.Generic.List[string]]::new()
        Mock Get-AuthenticodeSignature {
            param($LiteralPath)
            $inspectedPaths.Add($LiteralPath)
            if ($SignatureError) { throw 'Signature verification unavailable' }
            $isStaging = $LiteralPath -eq $staging
            $status = if ($isStaging -and $StagingStatus) { $StagingStatus }
                elseif (-not $isStaging -and $PackageStatus) { $PackageStatus }
                else { 'Valid' }
            $subject = if ($isStaging -and $StagingSubject) { $StagingSubject }
                elseif (-not $isStaging -and $PackageSubject) { $PackageSubject }
                else { 'CN=Microsoft Corporation, O=Microsoft Corporation, C=US' }
            $certificate = if (($isStaging -and $MissingStagingCertificate) -or
                (-not $isStaging -and $MissingPackageCertificate)) { $null }
                else { [pscustomobject]@{ Subject = $subject } }
            [pscustomobject]@{ Status = $status; SignerCertificate = $certificate }
        }

        if ($ExpectedError) {
            { & $helper -PackagePath $package -SignedAssemblyPath $staging } | Should -Throw $ExpectedError
        }
        else {
            $output = & $helper -PackagePath $package -SignedAssemblyPath $staging 6>&1
            $output | Should -Match "signed staging SHA256=$((Get-FileHash -LiteralPath $staging).Hash)"
            Should -Invoke Get-AuthenticodeSignature -Times 2 -Exactly
        }
        foreach ($path in $inspectedPaths | Where-Object { $_ -ne $staging }) {
            Test-Path -LiteralPath $path | Should -BeFalse
            Test-Path -LiteralPath (Split-Path $path -Parent) | Should -BeFalse
        }
    }
}
