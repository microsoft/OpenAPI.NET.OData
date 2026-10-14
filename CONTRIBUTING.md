# Contributing to OpenAPI.net.OData

OpenAPI.net.OData is a mono-repo containing source code for the following packages:

## Libraries

| Library                                                              | NuGet Release                                                                                                                                                                              |
|----------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| [Microsoft.OpenAPI.OData](./README.md)                         | [![NuGet Version](https://img.shields.io/nuget/vpre/Microsoft.OpenAPI.OData?label=Latest&logo=nuget)](https://www.nuget.org/packages/Microsoft.OpenAPI.OData/)                       |

## Hidi

[Microsoft.OpenApi.Hidi](src/Microsoft.OpenApi.Hidi/readme.md) is a .NET tool,
not an OData library package. Install the .NET 10 SDK and .NET 8 runtime to build
and test both projects. Both test projects use xUnit v3 and
Microsoft.Testing.Platform, selected by `global.json`. Reader tests still target
.NET 8; Hidi tests target .NET 10.

```powershell
dotnet build Microsoft.OpenApi.OData.sln -c Release
dotnet test --solution Microsoft.OpenApi.OData.sln -c Release --no-build -- --minimum-expected-tests 1
```

On non-Windows hosts, pass `-p:EnableWindowsTargeting=true` to both commands
because the solution also contains the Windows-only GUI project.
Use `--project <path>` instead of `--solution <path>` for a single test project.
MTP options replace VSTest's `--logger`, `--collect`, and MSBuild coverage options:

```powershell
dotnet test --solution Microsoft.OpenApi.OData.sln -c Release --no-build --results-directory artifacts\test-results -- --minimum-expected-tests 1 --report-trx --coverlet --coverlet-output-format cobertura --coverlet-include "[Microsoft.OpenApi.OData.Reader]*" --coverlet-include "[Microsoft.OpenApi.Hidi]*"
```

This produces separate TRX and coverage reports for each test assembly.
CI uses `--report-gh` for GitHub Actions and `--report-azdo` with TRX publication
for Azure DevOps. SonarCloud collects OpenCover reports for both projects in
`artifacts/test-results`, alongside the separately measured Pester helper coverage.

Hidi uses published OpenAPI core/YAML packages and the local OData project. Its
public-only signing key preserves assembly identity and friend-assembly access;
never add the source repository's private strong-name key. Production signing
is handled only by the official Azure pipeline.

Hidi's version is in its own project and the
`src/Microsoft.OpenApi.Hidi` entry in `.release-please-manifest.json`.
Its `hidi-v2.*` tags must not trigger OData publishing. Version 2.12.2 is the
already-published migration baseline, not a new release. The release configuration
enables tag-gated Hidi publishing once merged; hold its merge until official
signed-package verification and coordinated cutover approval. Source `support/v2`
publisher cleanup is already accepted in
[microsoft/OpenAPI.NET#3109](https://github.com/microsoft/OpenAPI.NET/pull/3109);
source-main cleanup is not a prerequisite for this branch. History-import
migration PRs must be merged with a merge commit, never squash or rebase.

### Independent automated versions

The standard Release Please action uses one `release-please-config.json` and
one `.release-please-manifest.json` with two components: root OData (`.`) and
Hidi (`src/Microsoft.OpenApi.Hidi`). Their versions advance independently in
the generated release PRs. The stock `separate-pull-requests` option preserves
OData's existing release branch and avoids the stock engine's componentless-root
parsing issue with combined release PRs.

Hidi source changes update its project `<Version>` and local `CHANGELOG.md`;
the root component excludes Hidi source and tests. OData changes update
`Directory.Build.props` and the root `CHANGELOG.md`, without changing Hidi.
Hidi keeps `hidi-v2.*` tags; OData keeps componentless `v2.*` tags. Hidi's
published OpenAPI dependency versions are not changed by its version updater.

Routing follows component paths, not commit scopes. Hidi-only tests do not
create a release by themselves, and root-level distribution/helper files remain
root-owned under this standard configuration. Production package, executable,
and container publishing remains gated in the official Azure pipeline.

### Component-tagged Azure releases

On `support/v2`, `.azure-pipelines/ci-build.yml` releases only the OData package
and attaches only its artifact for `v2.*` tags.
`.azure-pipelines/hidi-release.yml` handles only `hidi-v2.*` tags for Hidi's
NuGet package, Windows executable/ZIP, and container. `hidiPublishingEnabled`
is the string `'true'`; the publish stage still requires a successful build and
a `refs/tags/hidi-v2.*` ref, with all protected-environment approvals intact.
Branch/PR builds do not publish. Future publication requires a new corrected
release ref/version after cutover approval. Never move or reuse existing release
tags, rerun `hidi-v2.13.0` to publish, or republish its old unsigned artifacts.

Tag runs must exactly match the component's project version before staging
artifacts. Release jobs require the exact tag-derived package and symbols;
Hidi also requires its executable/ZIP or matching Docker-context version.
Wrong-component, malformed, other-major, or version-mismatched manually selected
tags fail validation instead of publishing. Ordinary branch/PR builds still
validate both projects as before, without running tag-only release stages.

OpenAPI.net.OData is open to contributions. There are a couple of different recommended paths to get contributions into the released version of this library.

__NOTE__ A signed a contribution license agreement is required for all contributions, and is checked automatically on new pull requests. Please read and sign [the agreement](https://cla.microsoft.com/) before starting any work for this repository.

## File issues

The best way to get started with a contribution is to start a dialog with the owners of this repository. Sometimes features will be under development or out of scope for this SDK and it's best to check before starting work on contribution. Discussions on bugs and potential fixes could point you to the write change to make.

## Submit pull requests for bug fixes and features

Feel free to submit a pull request with a linked issue against the __main__ branch.  The main branch will be updated frequently.
## Commit message format

To support our automated release process, pull requests are required to follow the [Conventional Commit](https://www.conventionalcommits.org/en/v1.0.0/)
format.
Each commit message consists of a __header__, an optional __body__ and an optional __footer__. The header is the first line of the commit and
MUST have a __type__ (see below for a list of types) and a __description__. An optional __scope__ can be added to the header to give extra context.

```
<type>[optional scope]: <short description>
<BLANK LINE>
<optional body>
<BLANK LINE>
<optional footer(s)>
```

The recommended commit types used are:

- __feat__ for feature updates (increments the _minor_ version)
- __fix__ for bug fixes (increments the _patch_ version)
- __perf__ for performance related changes e.g. optimizing an algorithm
- __refactor__ for code refactoring changes
- __test__ for test suite updates e.g. adding a test or fixing a test
- __style__ for changes that don't affect the meaning of code. e.g. formatting changes
- __docs__ for documentation updates e.g. ReadMe update or code documentation updates
- __build__ for build system changes (gradle updates, external dependency updates)
- __ci__ for CI configuration file changes e.g. updating a pipeline
- __chore__ for miscallaneous non-sdk changesin the repo e.g. removing an unused file

Adding an exclamation mark after the commit type (`feat!`) or footer with the prefix __BREAKING CHANGE:__ will cause an increment of the _major_ version.