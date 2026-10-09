# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeDiscovery {
    $cases = @(
        @{ Name = 'published version'; Versions = @('3.10.2', '3.10.3'); Expected = 'true' }
        @{ Name = 'case-insensitive prerelease'; Versions = @('3.10.3-PREVIEW.1'); PackageVersion = '3.10.3-preview.1'; Expected = 'true' }
        @{ Name = 'missing version'; Versions = @('3.10.2'); Expected = 'false' }
        @{ Name = 'empty version list'; Versions = @(); Expected = 'false' }
        @{ Name = 'package 404'; PackageStatus = 404; Expected = 'false' }
        @{ Name = 'package 401'; PackageStatus = 401; Fail = $true }
        @{ Name = 'package 403'; PackageStatus = 403; Fail = $true }
        @{ Name = 'package 500'; PackageStatus = 500; Fail = $true }
        @{ Name = 'service index 404'; IndexStatus = 404; Fail = $true }
        @{ Name = 'service index 401'; IndexStatus = 401; Fail = $true }
        @{ Name = 'network failure'; NetworkFailure = $true; Fail = $true }
        @{ Name = 'missing versions'; MissingVersions = $true; Fail = $true }
        @{ Name = 'string instead of version array'; Versions = '3.10.2'; Fail = $true }
        @{ Name = 'object instead of version array'; Versions = @{ error = 'unauthorized' }; Fail = $true }
        @{ Name = 'invalid version'; Versions = @('not-a-version'); Fail = $true }
        @{ Name = 'null version'; Versions = @($null); Fail = $true }
        @{ Name = 'missing content resource'; MissingResource = $true; Fail = $true }
        @{ Name = 'unsafe content resource'; UnsafeResource = $true; Fail = $true }
        @{ Name = 'missing package'; Files = @(); Fail = $true; Requests = 0 }
        @{ Name = 'wrong package'; Files = @('Microsoft.OpenApi.OData.3.10.3.nupkg'); Fail = $true; Requests = 0 }
        @{ Name = 'ambiguous Hidi versions'; Files = @('Microsoft.OpenApi.Hidi.3.10.3.nupkg', 'Microsoft.OpenApi.Hidi.3.10.4.nupkg'); Fail = $true; Requests = 0 }
        @{ Name = 'missing token'; Token = ''; Fail = $true; Requests = 0 }
        @{ Name = 'public service index'; Url = 'https://api.nuget.org/v3/index.json'; Fail = $true; Requests = 0 }
        @{ Name = 'mixed artifacts'; Files = @('Microsoft.OpenApi.Hidi.3.10.3.nupkg', 'Microsoft.OpenApi.Hidi.3.10.3.snupkg', 'Microsoft.OpenApi.OData.3.10.3.nupkg', 'Microsoft.OpenApi.3.10.3.nupkg'); Versions = @('3.10.3'); Expected = 'true' }
    )
}

Describe 'Authenticated Hidi NuGet idempotency' {
    BeforeAll {
        $helper = Join-Path $PSScriptRoot '..\..\scripts\check-nuget-package-published.ps1'
        $serviceIndex = 'https://microsoftgraph.pkgs.visualstudio.com/project/_packaging/feed/nuget/v3/index.json'
        $baseAddress = 'https://microsoftgraph.pkgs.visualstudio.com/project/_packaging/feed/nuget/v3/flat2'
    }

    It '<Name>' -ForEach $cases {
        $testCase = $_
        $directory = Join-Path $TestDrive ([guid]::NewGuid().ToString())
        New-Item -ItemType Directory -Path $directory | Out-Null
        $version = if ($PackageVersion) { $PackageVersion } else { '3.10.3' }
        $packageFiles = if ($testCase.ContainsKey('Files')) { $Files } else { @("Microsoft.OpenApi.Hidi.$version.nupkg") }
        foreach ($file in $packageFiles) { New-Item -ItemType File -Path (Join-Path $directory $file) | Out-Null }
        $token = if ($testCase.ContainsKey('Token')) { $Token } else { 'mock-test-token' }
        $url = if ($Url) { $Url } else { $serviceIndex }

        Mock Invoke-RestMethod {
            param($Uri, $Headers, $MaximumRedirection)
            $expectedAuth = 'Basic ' + [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes('AzureDevOps:mock-test-token'))
            $Headers.Authorization | Should -BeExactly $expectedAuth
            $MaximumRedirection | Should -Be 0
            if ($Uri -eq $serviceIndex) {
                if ($IndexStatus) {
                    $response = [System.Net.Http.HttpResponseMessage]::new([System.Net.HttpStatusCode]$IndexStatus)
                    throw [Microsoft.PowerShell.Commands.HttpResponseException]::new('Mock service-index failure', $response)
                }
                if ($MissingResource) { return @{ resources = @() } }
                $address = if ($UnsafeResource) { 'https://example.invalid/flat2' } else { $baseAddress }
                return @{ resources = @(@{ '@type' = 'PackageBaseAddress/3.0.0'; '@id' = $address }) }
            }
            $Uri | Should -BeExactly "$baseAddress/microsoft.openapi.hidi/index.json"
            if ($NetworkFailure) { throw [System.Net.Http.HttpRequestException]::new('Mock network failure') }
            if ($PackageStatus) {
                $response = [System.Net.Http.HttpResponseMessage]::new([System.Net.HttpStatusCode]$PackageStatus)
                throw [Microsoft.PowerShell.Commands.HttpResponseException]::new('Mock package failure', $response)
            }
            if ($MissingVersions) { return @{} }
            return @{ versions = $Versions }
        }

        $output = [Collections.Generic.List[string]]::new()
        $failure = $null
        try {
            & $helper -PackageDirectory $directory -PackageId 'Microsoft.OpenApi.Hidi' -NuGetServiceIndexUrl $url -FeedAccessToken $token 6>&1 |
                ForEach-Object { $output.Add($_.ToString()) }
        }
        catch { $failure = $_ }
        $text = $output -join "`n"
        if ($Fail) {
            $failure | Should -Not -BeNullOrEmpty
            $text | Should -Not -Match '##vso\[task.setvariable'
        }
        else {
            $failure | Should -BeNullOrEmpty
            $text | Should -Match "##vso\[task.setvariable variable=nugetAlreadyPublished\]$Expected"
        }
        $text | Should -Not -Match 'mock-test-token|AzureDevOps:'
        $expectedRequests = if ($testCase.ContainsKey('Requests')) { $Requests }
            elseif ($IndexStatus -or $MissingResource -or $UnsafeResource) { 1 }
            else { 2 }
        Should -Invoke Invoke-RestMethod -Times $expectedRequests -Exactly
    }
}
