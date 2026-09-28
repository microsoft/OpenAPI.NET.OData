# Hidi Relocation Implementation Plan

## 1. Purpose and scope

Move the Hidi command-line tool from `microsoft/OpenAPI.NET` to
`microsoft/OpenAPI.NET.OData`, including source, tests, package ownership,
documentation, continuous integration, ADO publishing, GitHub releases, and Docker/MCR
publishing.

### In scope

- `Microsoft.OpenApi.Hidi` source project and embedded `CsdlFilter.xslt`
- `Microsoft.OpenApi.Hidi.Tests` and test resources
- NuGet global/local tool package `Microsoft.OpenApi.Hidi`
- Windows self-contained executable release asset
- `mcr.microsoft.com/openapi/hidi` nightly and release images
- GitHub Actions, ADO YAML, release-please, solution, CodeQL, developer build/debug
  helpers, and repository documentation in both repos
- Filtered Hidi source/test history, path normalization, commit mapping, and history
  integrity validation
- ADO pipeline-definition settings, service-connection permissions, environments,
  triggers, and required-check updates that are managed outside YAML

### Out of scope

- Renaming the NuGet package, CLI command, namespaces, or MCR image
- Functional redesign of Hidi commands
- Preserving original OpenAPI.NET commit IDs or cryptographic commit signatures after
  history filtering
- Importing unrelated OpenAPI.NET source, branches, tags, or complete repository
  history
- Moving OpenAPI.NET core or YAML reader source into the OData repo
- Combining OData and Hidi release versions

## 2. Current-state findings

### OpenAPI.NET source repo

- The migration baseline is source commit
  `afd4967a9e6db390175e2df9e6f34ff77168d19d` (September 11, 2026). It includes
  Workbench removal commit `07fac9e07e53746ff14a6110338530f99531cc9e`.
- `Microsoft.OpenApi.Workbench`, its solution entry, README section, image, VS Code
  integration, and pipeline package-exclusion entry have been removed. Workbench is
  not a Hidi dependency and must not be reintroduced by filtering or cleanup.
- Hidi consists of 20 source/configuration files under `src/Microsoft.OpenApi.Hidi` and
  19 test/resource files under `test/Microsoft.OpenApi.Hidi.Tests` when generated
  `bin`/`obj` content is excluded.
- Across current and legacy paths, Hidi has 850 path-related commits in OpenAPI.NET;
  332 of those also modify non-Hidi paths and must be reduced to their Hidi portions
  by history filtering.
- Source history includes the path transitions
  `src/Microsoft.OpenApi.Tool` -> `src/Microsoft.Hidi` ->
  `src/Microsoft.OpenApi.Hidi`, plus the test-project move from
  `Microsoft.OpenApi.Hidi.Tests` at the repository root to
  `test/Microsoft.OpenApi.Hidi.Tests`.
- `Microsoft.OpenApi.Hidi.csproj` is a `net8.0` packed .NET tool with command name
  `hidi`, assembly signing, package generation, and package README support.
- The project references local `Microsoft.OpenApi` and
  `Microsoft.OpenApi.YamlReader` projects, but references `Microsoft.OpenApi.OData`
  `3.2.1` as a package. It also consumes `Microsoft.OpenApi.ApiManifest`,
  `Microsoft.OData.Edm`, `System.CommandLine`, and logging packages.
- Hidi tests target `net10.0`, use xUnit v3/Microsoft Testing Platform, are signed, and
  depend on Hidi internals through a strong-name `InternalsVisibleTo`.
- `Microsoft.OpenApi.Tests.csproj` has a Hidi project reference despite no source-level
  Hidi usage found; this should be verified and removed during source cleanup.
- `Microsoft.OpenApi.slnx`, `build.cmd`, `build.sh`, VS Code launch/tasks/settings,
  `install-tool.ps1`, root README, CONTRIBUTING, CodeQL, and the root Dockerfile all
  contain Hidi-specific integration.
- `.azure-pipelines/ci-build.yml` builds/tests Hidi, packs and signs its NuGet, emits a
  Windows executable, publishes the tool to NuGet, attaches the executable to a GitHub
  release, and builds/pushes multi-architecture nightly/release images.
- Hidi currently inherits the source repo's repository-wide version (`3.10.2` at
  analysis time).

### OpenAPI.NET.OData destination repo

- The solution currently contains the OData reader and tests plus `OoasUtil` and
  `OoasGui`; its GitHub CI builds/tests only the reader path or solution depending on
  workflow.
- The OData project targets `net8.0`, references `Microsoft.OpenApi` `3.10.2`, and uses
  `tool/Microsoft.OpenApi.OData.snk` for signing.
- The destination repository version is `3.2.1` at analysis time and release-please
  treats the repository as one `Microsoft.OpenApi.OData` package with `v*` tags.
- Its ADO pipeline already builds/tests, ESRP-signs, packs, publishes to NuGet, and
  edits a GitHub release for OData. It does not currently publish Hidi or an MCR image.
- The pipeline already uses the `OpenAPI Nuget Connection`, federated ESRP connection,
  `nuget-org`, and `kiota-github-releases`. Hidi Docker publishing additionally needs
  the ACR push connection and `docker-images-deploy` environment used by the source
  pipeline.
- `src/OoasUtil/README.md` already directs users to Hidi in the source repo and must be
  relinked after the move.

## 3. Target architecture

### Project dependency direction

In `OpenAPI.NET.OData/src/Microsoft.OpenApi.Hidi/Microsoft.OpenApi.Hidi.csproj`:

- Replace the `Microsoft.OpenApi.OData` package reference with a project reference to
  `../Microsoft.OpenApi.OData.Reader/Microsoft.OpenAPI.OData.Reader.csproj`.
- Replace source-repo project references to `Microsoft.OpenApi` and
  `Microsoft.OpenApi.YamlReader` with explicit package references. Pin compatible
  released versions and update them through normal dependency automation.
- Retain the existing external package dependencies unless restore/build proves a
  version adjustment is required.
- Preserve `PackageId`, `ToolCommandName`, target framework, package README,
  `PackAsTool`, package description, embedded XSLT, and public namespace behavior.
- Give Hidi an explicit version property/file managed independently from
  `Directory.Build.props`.
- Preserve Hidi's existing OpenAPI.NET strong-name identity in a Hidi-specific key.
  Released `Microsoft.OpenApi` packages grant friend access to that identity, so using
  the OData key would break Hidi's required internal API access. Keep the matching
  `InternalsVisibleTo` public key for signed tests.

This direction lets Hidi validate against the exact OData source under change while
testing against released core/YAML packages, eliminating the current need to wait for
an OData package publication before Hidi can consume a fix.

### Release boundaries

- Keep the root OData component on `v<odata-version>` tags.
- Add a Hidi release-please component rooted at `src/Microsoft.OpenApi.Hidi`, with its
  own manifest version and `hidi-v<hidi-version>` tags.
- Ensure release-please updates Hidi's explicit version location and Hidi changelog,
  while the existing root component continues to update OData's
  `Directory.Build.props` and root changelog.
- Route ADO deployment by exact tag family:
  - `refs/tags/v*`: publish OData package/release only.
  - `refs/tags/hidi-v*`: publish Hidi package, executable/release asset, and release
    MCR image only.
  - `refs/heads/main`: build/test everything and publish only Hidi nightly images.
- Use distinct artifact selection rather than broad
  `Microsoft.OpenApi.*.nupkg` globs where a job owns one package.

## 4. Implementation phases

### Phase A - Establish provenance, filtered-history design, and release design

1. Record the source repository URL, branch, and exact commit SHA in the destination
   relocation PR and Hidi README. Use that immutable SHA for review comparisons.
2. Pin the initial filtering run to
   `afd4967a9e6db390175e2df9e6f34ff77168d19d`. If source HEAD advances before
   execution, deliberately select a new baseline and repeat commit counts, tree
   comparison, and mixed-commit analysis.
3. Create a disposable mirror/clone of OpenAPI.NET. Install and pin an approved
   `git filter-repo` version; never perform history rewriting in either working
   repository.
4. Filter the source history to retain these historical paths:
   - `src/Microsoft.OpenApi.Tool`
   - `src/Microsoft.Hidi`
   - `src/Microsoft.OpenApi.Hidi`
   - `Microsoft.OpenApi.Hidi.Tests`
   - `test/Microsoft.OpenApi.Hidi.Tests`
5. Normalize all retained source paths to `src/Microsoft.OpenApi.Hidi` and all retained
   test paths to `test/Microsoft.OpenApi.Hidi.Tests`. Account for commits where old and
   new names coexist so the rewrite does not create path collisions.
6. Exclude OpenAPI.NET branches and tags from the deliverable import. Import one audited
   filtered branch/ref so source release tags cannot collide with OData's existing
   `v*` refs.
7. Preserve author, committer, timestamps, and messages. Document that filtering
   rewrites commit IDs, invalidates cryptographic signatures, and retains only the Hidi
   portions of the 332 cross-cutting commits.
8. Export and retain the `git filter-repo` source-to-rewritten commit map as a migration
   artifact. Use it with the recorded source SHA to trace destination history back to
   original OpenAPI.NET commits and PRs.
9. Record the latest published `Microsoft.OpenApi.Hidi` NuGet version and current MCR
   image digests/tags before selecting the first destination-managed Hidi version.
10. Add the Hidi component to `release-please-config.json` and
   `.release-please-manifest.json` without changing OData's root component/tag.
11. Configure an explicit Hidi version source that starts above the latest published
   package version; do not inherit OData's `3.2.x` repository version.
12. Document conventional commit scopes so Hidi-only changes feed the Hidi release
   component and OData changes remain independently releasable.

**Exit criteria:** The filtered branch and commit map are reproducible from the recorded
source SHA; release automation can distinguish an OData release from a Hidi release;
and the proposed next Hidi version cannot collide with or regress an existing NuGet
version.

### Phase B - Audit and import history, then adapt the destination

1. Audit the filtered repository before importing it:
   - Compare the filtered tip's Hidi source/test trees with the recorded source SHA.
   - Confirm no unrelated OpenAPI.NET paths, generated `bin`/`obj` files, secrets, or
     unexpected large objects remain.
   - Confirm no deleted Workbench source, documentation, image, solution, VS Code, or
     pipeline content appears in the filtered tree.
   - Confirm `git log --follow` traverses the known source and test renames.
   - Sample cross-cutting dependency, test, and feature commits using the commit map.
2. Fetch the filtered branch into OpenAPI.NET.OData under a temporary namespace and
   inspect the incoming graph and tree before merging.
3. Merge the filtered tip with `--allow-unrelated-histories`. Do not squash, replay the
   history as one patch, cherry-pick hundreds of commits, or rebase it onto OData,
   because those alternatives prevent first-class blame/history preservation.
4. Resolve only destination layout conflicts at the merge boundary. Historical commits
   are evidence and are not expected to build against OData's repository layout.
5. Add a new integration commit after the merge that applies the target
   project-reference/package-reference inversion described above.
6. Add the Hidi-specific copy of the existing signing key and retain the matching
   `InternalsVisibleTo` public key data.
7. Add both projects to `Microsoft.OpenApi.OData.sln`.
8. Preserve the Hidi test project's xUnit v3/Microsoft Testing Platform setup unless a
   destination runner incompatibility is demonstrated; install both .NET 8 and .NET 10
   in CI because production and test targets differ.
9. Copy/adapt `install-tool.ps1`, relevant VS Code launch/task/settings entries, and
   developer build helpers. Prefer solution-level build commands over duplicating
   per-project build lists.
10. Move/adapt the Hidi Dockerfile into the destination. Update build context, project
   paths, copied props/files, runtime output path, and source/documentation labels.
11. Update the destination root README and `src/OoasUtil/README.md` to make OData the
   canonical source and documentation location for Hidi.
12. Add a relocation note containing the source SHA, filtered tip, commit-map location,
    and links to the previous location.

**Exit criteria:** The destination contains only the audited filtered history plus
explicit integration commits, and a clean checkout can restore, build, test, pack,
install, and run Hidi without checking out OpenAPI.NET or consuming an unpublished
OData package.

### Phase C - Extend destination GitHub CI

1. Update `.github/workflows/ci-cd.yml` to install .NET 8 and .NET 10, build the full
   solution, and run all solution tests, including Hidi tests.
2. Produce a Hidi `.nupkg` as a non-publishing CI artifact and add a tool-install smoke
   test from that artifact. Exercise at least `--help`, `validate`, `transform`, `show`,
   and `plugin` using copied fixtures where applicable.
3. Update `.github/workflows/codeql-analysis.yml` to build the solution or explicitly
   include Hidi and OData, instead of building only the reader project.
4. Confirm SonarCloud and any path-based/required workflows include the new source and
   test directories; update explicit project lists only where present.
5. Update release-please workflow permissions/configuration so it can create separate
   OData and Hidi release PRs/tags without one component advancing the other.
6. Update branch-protection required checks if job names change or new Hidi smoke checks
   become required.

**Exit criteria:** Destination pull requests block on Hidi compilation, unit tests,
package installation, and security analysis.

### Phase D - Extend destination ADO build and publishing

#### YAML changes

1. Add .NET 10 setup and build/test the full destination solution.
2. Pack Hidi separately into the pipeline artifact, include symbols/source consistently,
   and ESRP-sign its binaries and NuGet package with the established signing tasks.
3. Publish a self-contained `win-x64` single-file Hidi executable to a dedicated
   artifact directory.
4. Add distinct deployment jobs and conditions:
   - OData NuGet job on `refs/tags/v*`.
   - Hidi NuGet job on `refs/tags/hidi-v*`.
   - OData GitHub release job on `refs/tags/v*`.
   - Hidi GitHub release job on `refs/tags/hidi-v*`, attaching the executable and
     intended package assets.
5. Add a repository-files/Docker-context artifact containing only files required to
   build Hidi's image.
6. Move the multi-architecture Docker build/push steps and variables
   (`REGISTRY=msgraphprodregistry.azurecr.io`,
   `IMAGE_NAME=public/openapi/hidi`) from the source pipeline.
7. Derive Docker tags from Hidi's explicit version, not OData's
   `Directory.Build.props`:
   - Main: `nightly` and a unique prerelease/nightly tag.
   - Hidi release tag: `latest` and `<hidi-version>`.
8. Keep package/release jobs independent where safe, but make the Hidi release-image and
   GitHub-release jobs depend on successful Hidi build/sign/package output.
9. Tighten file globs so OData jobs cannot publish Hidi and Hidi jobs cannot publish
   OData.

#### ADO definition and resource changes

1. Update the existing OData pipeline definition's repository YAML path if necessary
   and add CI/tag triggers for `main`, supported branches, `v*`, and `hidi-v*`.
2. Authorize the pipeline to use:
   - `Federated DevX ESRP Managed Identity Connection`
   - `OpenAPI Nuget Connection`
   - `Github-MaggieKimani1` or its approved replacement
   - `ACR Images Push Service Connection`
3. Grant the pipeline access to `nuget-org`, `kiota-github-releases`, and
   `docker-images-deploy` environments. Preserve production approvals/checks.
4. Confirm the ACR connection can push the existing
   `public/openapi/hidi` repository; no image rename should occur.
5. Configure retention for signed packages, executable, Docker context, test results,
   and logs sufficient to diagnose the first destination releases.
6. Run the YAML through 1ES template validation and pipeline security/compliance review.

**Exit criteria:** An ordinary destination commit runs build/test/package validation
without publishing; only matching tag families can enter their corresponding production
deployment jobs; main can update only Hidi nightly images.

### Phase E - Destination-first validation and release

1. Compare the filtered tip against the recorded source SHA and account for every
   retained, omitted, and path-normalized file.
2. Verify history integrity:
   - `git log --follow` reaches the January 2020 command-line tool history.
   - `git blame` on representative source and test files resolves to original authors
     and dates.
   - No OpenAPI.NET tags, unrelated branches, or unrelated source paths were imported.
   - No Workbench files or integration were resurrected by the filtered import.
   - The commit map resolves sampled original commits, including cross-cutting changes.
3. Treat the imported graph as historical evidence, not as a sequence expected to
   build against OData. Require only the filtered tip plus integration commits to build.
4. Validate clean restore/build/test in Release configuration on the same SDKs used by
   GitHub and ADO.
5. Pack Hidi and inspect the `.nupkg` for:
   - Package ID and version
   - `DotnetToolSettings.xml` command `hidi`
   - README, license, repository URL/commit, symbols, and dependency versions
   - No destination-only project paths or unintended files
6. Install the package into an isolated tool path and run CLI smoke tests against known
   OpenAPI, CSDL, API manifest, and output fixtures.
7. Build and run the Docker image locally for the host architecture; confirm the same
   CLI smoke cases and output-volume behavior.
8. Run destination GitHub CI and an ADO non-tag validation build. Confirm no NuGet,
   GitHub release, or release-image deployment is reachable.
9. Exercise a controlled Hidi release tag using the next non-colliding version. Verify:
   - Signed NuGet package is available and installable from NuGet.org.
   - GitHub release uses `hidi-v*` and contains expected assets.
   - `linux/amd64` and `linux/arm64/v8` manifests exist under the existing MCR image.
   - `latest` points to the released version and `nightly` remains independently
     updateable.
10. Monitor downstream installation/smoke tests before beginning source removal.

**Exit criteria:** One real Hidi version has been produced entirely by the destination
repo and all existing distribution identities remain functional.

### Phase F - Remove Hidi ownership from OpenAPI.NET

Perform this phase only after Phase E succeeds.

1. Remove `src/Microsoft.OpenApi.Hidi` and `test/Microsoft.OpenApi.Hidi.Tests`.
2. Remove Hidi entries from `Microsoft.OpenApi.slnx`.
3. Remove the stale Hidi project reference from
   `test/Microsoft.OpenApi.Tests/Microsoft.OpenApi.Tests.csproj` after confirming no
   compile-time dependency.
4. Update `.azure-pipelines/ci-build.yml` to remove:
   - Hidi pack and executable publish steps
   - Hidi NuGet deployment
   - Hidi package exclusion workaround in the core deployment
   - Docker variables, repository-files artifact, ACR login, and all image deployment
     steps
   - Main-branch deploy-stage behavior that existed only for nightly Hidi images
   The current core-package exclusion workaround removes Hidi and YAML Reader packages;
   it no longer includes Workbench.
5. Remove or repurpose the root Hidi Dockerfile and remove Hidi-specific entries from
   CodeQL, `build.cmd`, `build.sh`, VS Code files, and `install-tool.ps1`.
6. Update README and CONTRIBUTING entries to link to Hidi's OData repository source,
   documentation, issues, and contribution flow. Retain a concise relocation notice
   where it helps existing deep-link users.
7. Confirm the OpenAPI.NET release pipeline now packages/publishes only core and YAML
   reader assets and no broad glob can pick up Hidi.
8. Update source-repo branch protection/required checks if removed jobs changed check
   names.

**Exit criteria:** OpenAPI.NET has no Hidi build or publishing responsibility, while
user-facing links direct consumers to the new canonical location.

## 5. Validation matrix

| Surface | Required validation |
| --- | --- |
| Build | Destination solution builds in Release on CI SDKs; source solution builds after removal. |
| Unit tests | All Hidi and OData tests pass together; all remaining OpenAPI.NET tests pass without a Hidi reference. |
| NuGet tool | Package metadata and signing are valid; isolated install exposes `hidi`; upgrade from the prior version succeeds. |
| CLI compatibility | Help and representative validate/transform/show/plugin commands preserve exit codes and outputs. |
| CSDL integration | Hidi tests run against the destination OData project and cover conversion/filtering behavior. |
| Security | Destination CodeQL and existing security workflows analyze Hidi; ESRP verification succeeds. |
| ADO routing | `v*`, `hidi-v*`, main, PR, and ordinary branch runs reach only their intended jobs. |
| GitHub release | Hidi creates its own tag/release and assets without advancing the OData component. |
| Docker | Local smoke test succeeds; release and nightly tags contain amd64/arm64 manifests and use Hidi's version. |
| Documentation | NuGet README, repository links, Docker labels, badges, OoasUtil link, and relocation links resolve correctly. |
| Git history | Known renames traverse correctly; blame retains authors/dates; no source refs or unrelated files are imported; sampled original commits resolve through the commit map. |

## 6. Cutover controls and rollback

### Before the first destination publication

- Keep source ADO Hidi publishing intact.
- Prevent destination publishing jobs from receiving production approval until package,
  signing, routing, and smoke validation pass.
- If validation fails, disable destination Hidi tag deployment and continue releasing
  from the source repo.

### After NuGet publication

- NuGet packages are immutable; never attempt to overwrite or reuse the released
  version. Fix forward with a new Hidi version.
- If the destination pipeline is unavailable, temporarily retain or restore the source
  publishing path and release a new non-colliding version only after confirming exactly
  one pipeline can publish it.

### Docker rollback

- Record the previous `latest` and `nightly` image digests before cutover.
- If the new image fails smoke or production checks, repoint the mutable tag to the
  previous approved digest while preserving the failed immutable version tag for
  diagnosis.

### Source cleanup rollback

- Land source removal in a standalone PR after destination release success so it can be
  reverted without reverting unrelated OpenAPI.NET changes.
- Do not delete ADO service connections or environments as part of source cleanup;
  remove only the old pipeline's authorization when the destination has proven stable.

### History import rollback

- Record the destination commit before the history merge and keep the filtered import
  under a temporary ref until validation and cutover complete.
- If the import is invalid before the relocation branch is shared, recreate the branch
  from the recorded pre-import commit and rerun filtering.
- If the relocation branch is already under review, replace it through the agreed
  migration-branch workflow rather than rewriting `main` or another shared branch.
- Do not attempt to repair a faulty import with mass follow-up deletions; correct and
  rerun the deterministic filter specification so the resulting graph remains auditable.

## 7. Pull request and ownership sequence

1. **OData release-boundary PR:** Independent Hidi release-please component and version
   source, with no production publishing enabled.
2. **OData history-import PR:** Audited filtered source/test graph, deterministic filter
   specification, source SHA, filtered tip, and commit map.
3. **OData relocation PR:** Dependency/signing/solution/docs/developer tooling
   integration plus GitHub CI.
4. **OData ADO PR:** Build, signing, artifacts, tag-routed publishing, GitHub release,
   and Docker jobs; complete external ADO authorizations alongside it.
5. **Destination release operation:** Create and verify the first `hidi-v*` release.
6. **OpenAPI.NET cleanup PR:** Remove Hidi and all old CI/CD/distribution ownership;
   add relocation links.

Each PR should identify an owner for repository code, release-please, ADO/1ES, ESRP
signing, NuGet publishing, ACR/MCR, and branch-protection changes. The release operation
requires an explicit go/no-go review from those owners.

## 8. Definition of done

- The destination repo is the canonical source for Hidi and contains audited filtered
  history through the recorded source SHA.
- Representative files retain useful `git blame` and `git log --follow` history; the
  migration artifact maps original OpenAPI.NET commits to rewritten commits.
- Hidi uses a destination OData project reference and released core/YAML packages.
- Hidi has an independent version/changelog/tag stream and cannot be published by an
  OData tag.
- GitHub and ADO CI build, test, analyze, package, and smoke-test Hidi.
- ADO signs and publishes the Hidi NuGet package and executable only on `hidi-v*`.
- ADO publishes existing-name nightly/release multi-architecture MCR images from the
  destination.
- A destination-created release has passed package, CLI, GitHub release, and Docker
  verification.
- OpenAPI.NET contains no Hidi project, test, build, release, Docker, or resident-project
  documentation ownership.
- Both repositories' required checks and external ADO resource permissions reflect the
  final ownership model.
