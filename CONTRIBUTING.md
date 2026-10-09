# Contributing to OpenAPI.net.OData

OpenAPI.net.OData is a mono-repo containing source code for the following packages:

## Libraries

| Library                                                              | NuGet Release                                                                                                                                                                              |
|----------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| [Microsoft.OpenAPI.OData](./README.md)                         | [![NuGet Version](https://img.shields.io/nuget/vpre/Microsoft.OpenAPI.OData?label=Latest&logo=nuget)](https://www.nuget.org/packages/Microsoft.OpenAPI.OData/)                       |

## Hidi

[Microsoft.OpenApi.Hidi](src/Microsoft.OpenApi.Hidi/readme.md) is a .NET tool,
not an OData library package. Install the .NET 10 SDK and .NET 8 runtime to build
and test both projects. Do not change the solution-wide test runner: OData uses
VSTest and hidi uses Microsoft.Testing.Platform.

```powershell
dotnet build Microsoft.OpenApi.OData.sln -c Release
dotnet test test\Microsoft.OpenAPI.OData.Reader.Tests\Microsoft.OpenAPI.OData.Reader.Tests.csproj -c Release
dotnet run --project test\Microsoft.OpenApi.Hidi.Tests\Microsoft.OpenApi.Hidi.Tests.csproj -c Release -- --minimum-expected-tests 1
```

Hidi uses published OpenAPI core/YAML packages and the local OData project. Its
public-only signing key preserves assembly identity and friend-assembly access;
never add the source repository's private strong-name key. Production signing
is handled only by the official Azure pipeline.

Hidi's version is in its own project and `.hidi-release-please-manifest.json`.
Its `hidi-v2.*` tags must not trigger OData publishing. Version 2.12.2 is the
already-published migration baseline, not a new release. Release Please now
opens independent version PRs on `support/v2`; Hidi tags, GitHub releases, and
production publishing remain disabled until source cutover. History-import
migration PRs must be merged with a merge commit, never squash or rebase.

### Independent automated versions

The Release Please workflow uses the pinned API runner in
`.github/release-please`. It registers `exact-path-exclusions` because the
upstream `exclude-paths` implementation matches directories, not individual
files. Keep the root OData config/manifest and the Hidi config/manifest separate;
the existing `simple` strategy updates `Directory.Build.props` for OData and
only the Hidi project's `<Version>` for Hidi. Hidi's changelog and annotated
installation examples are updated in the same version PR, without changing its
published OpenAPI dependencies.

| Changed paths | Version track |
| --- | --- |
| Hidi CLI source, tests, and packaged readme | Hidi only |
| `Dockerfile`, `.dockerignore`, `install-tool.ps1`, Hidi public keys/local NuGet config | Hidi only |
| `.azure-pipelines/hidi-release.yml`, private-feed helper and its Pester tests, Hidi config/manifest | Hidi only |
| OData reader, GUI/utilities, OData tests/assembly metadata, `docs`, Redocly config, legacy `build.cmd`/`build.ps1`, OData config/manifest, `Directory.Build.props` | OData only |
| Root README/contributing docs, solution, SDK, `Build.props`, `src/Build.props`, `build.root` | Both, for releasable commits |
| `.github`, editor configuration directories, OData Azure CI pipeline | Neither |

Routing is by changed paths, not commit scopes. Use `fix(hidi):` or
`feat(hidi):` for clarity, but a Hidi scope does not override file ownership.
A mixed change touching both components contributes to both tracks. An OData
version-only edit does not independently bump Hidi.

Hidi version PRs use the `hidi` component and their own branch and lifecycle
labels (`autorelease: hidi-pending` and `autorelease: hidi-versioned`). After a
version PR merges, the runner uses that merge commit as the next version's
checkpoint and retires the pending label. This allows subsequent version PRs
without tags and does not block OData's normal release lifecycle. No Hidi
GitHub release or publishing job is invoked by this runner; production cutover
remains a separate protected operation.

On its first Hidi run, the app token creates those two repository labels if
missing, using the GitHub labels API. Existing labels and OData's default labels
are not changed. API permission, network, or validation failures stop automation
explicitly; no publishing permissions or protected resources are granted.

Run the offline routing and version-file tests with Node.js 24:

```powershell
Push-Location .github\release-please
npm ci --ignore-scripts
npm test
npm run coverage
Pop-Location
```

The coverage command measures only the runner with Node's native coverage
instrumentation and writes `artifacts/release-please/lcov.info` relative to the
repository root. The existing SonarCloud workflow imports this report alongside
the unchanged C# and PowerShell coverage reports.

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