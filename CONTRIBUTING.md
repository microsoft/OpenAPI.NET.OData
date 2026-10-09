# Contributing to OpenAPI.net.OData

OpenAPI.net.OData is a mono-repo containing source code for the following packages:

## Libraries

| Library                                                              | NuGet Release                                                                                                                                                                              |
|----------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| [Microsoft.OpenAPI.OData](./README.md)                         | [![NuGet Version](https://img.shields.io/nuget/vpre/Microsoft.OpenAPI.OData?label=Latest&logo=nuget)](https://www.nuget.org/packages/Microsoft.OpenAPI.OData/)                       |

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

## Independent Hidi version automation

OData and Hidi retain separate root release-please configs and version manifests.
Hidi's package directory is deliberately `.`: a package rooted at
`src/Microsoft.OpenApi.Hidi` would miss its tests, Dockerfile, installer and
NuGet helper outside that directory. Routing is based on changed files, not the
optional Conventional Commit scope. A mixed commit can affect both components.

The pinned runner in `.github/release-please` uses release-please's registered
plugin API to apply `exclude-paths` to exact files as well as directories.
The upstream engine's directory-only matching does not exclude a root filename.
Hidi source, tests, readme, Docker distribution, installer, public identity,
local NuGet config, private-feed helper and Hidi release config/pipeline affect
only Hidi. OData source/tests/docs, its release config/manifest/changelog and
`Directory.Build.props` affect only OData. Shared root README, contribution
guidance, solution, SDK and build files can affect both; shared GitHub CI and
the OData CI pipeline do not trigger releases. Existing OData exclusions,
including its public signing key, remain in place.

The OData job still creates GitHub releases before opening version PRs.
The Hidi job opens **version PRs only**, with separate component branches and
`autorelease: hidi-pending` labels. After a version PR merges, the runner verifies
its parsed version against the Hidi manifest, uses that merge as the next commit
boundary, and changes its label to `autorelease: hidi-versioned`. No Hidi tag or
GitHub release is created, and Hidi labels cannot block OData automation.
Missing, inconsistent or unreachable checkpoints fail rather than replaying
old commits. Production publishing and cutover remain separately gated.

Hidi starts at the already-published 3.10.2 on `main` and 2.12.2 on `support/v2`.
Fixes increment its patch; features increment its minor. Generated versions must
remain above the migration floor and within that branch's major, matching the
official pipeline's `hidi-v3.*` / `hidi-v2.*` contracts. A breaking-change major
bump fails before opening a version PR; moving to a new major requires a separate
branch/pipeline decision. Hidi PRs update the project `<Version>`, its manifest
and changelog, and annotated install examples, not OData's version or Hidi's
upstream package dependency versions.

Run the offline regression tests with Node 24:

```powershell
Push-Location .github\release-please
npm ci --ignore-scripts --no-audit --no-fund
npm test
Pop-Location
```

These tests exercise the pinned engine's actual plans and XML/changelog updates,
including two merged Hidi version-PR cycles without tags. The release workflow
runs them before any API writes, including on relevant pull requests. Sonar also
runs measured native Node coverage and imports its LCOV report alongside the
existing C# and PowerShell coverage; the runner is not excluded from analysis.