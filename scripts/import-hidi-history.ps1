param(
    [Parameter(Mandatory)]
    [string] $SourceRepository,

    [Parameter(Mandatory)]
    [string] $OutputRepository
)

$ErrorActionPreference = 'Stop'
$sourceCommit = 'afd4967a9e6db390175e2df9e6f34ff77168d19d'

if (Test-Path $OutputRepository) {
    throw "Output path already exists: $OutputRepository"
}

git filter-repo --version | Out-Null
if ($LASTEXITCODE -ne 0) {
    throw 'git-filter-repo is required.'
}

git clone --no-local $SourceRepository $OutputRepository
git -C $OutputRepository checkout --detach $sourceCommit
$sourceBranches = git -C $OutputRepository for-each-ref --format='%(refname:short)' refs/heads
foreach ($branch in $sourceBranches) {
    git -C $OutputRepository branch -D $branch
}
git -C $OutputRepository switch -c hidi-history
git -C $OutputRepository filter-repo --force --refs hidi-history `
    --path src/Microsoft.OpenApi.Tool `
    --path src/Microsoft.Hidi `
    --path src/Microsoft.OpenApi.Hidi `
    --path Microsoft.OpenApi.Hidi.Tests `
    --path test/Microsoft.OpenApi.Hidi.Tests `
    --path-rename src/Microsoft.OpenApi.Tool/:src/Microsoft.OpenApi.Hidi/ `
    --path-rename src/Microsoft.Hidi/:src/Microsoft.OpenApi.Hidi/ `
    --path-rename Microsoft.OpenApi.Hidi.Tests/:test/Microsoft.OpenApi.Hidi.Tests/

$retainedRef = 'refs/heads/hidi-history'
$otherRefs = git -C $OutputRepository for-each-ref --format='%(refname)' |
    Where-Object { $_ -ne $retainedRef }

foreach ($ref in $otherRefs) {
    git -C $OutputRepository update-ref -d $ref
}

$files = @(git -C $OutputRepository ls-tree -r --name-only hidi-history)
if ($files.Count -ne 39) {
    throw "Expected 39 files at the filtered tip, found $($files.Count)."
}

$unexpectedFiles = $files | Where-Object {
    $_ -notlike 'src/Microsoft.OpenApi.Hidi/*' -and
    $_ -notlike 'test/Microsoft.OpenApi.Hidi.Tests/*'
}
if ($unexpectedFiles) {
    throw "Unexpected files remain:`n$($unexpectedFiles -join "`n")"
}

Write-Host "Filtered Hidi history created at $OutputRepository"
Write-Host "Filtered tip: $(git -C $OutputRepository rev-parse hidi-history)"
Write-Host "Commit map: $OutputRepository\.git\filter-repo\commit-map"
