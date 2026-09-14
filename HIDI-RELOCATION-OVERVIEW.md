# Hidi Relocation Overview

## Objective

Relocate the `Microsoft.OpenApi.Hidi` .NET tool from
[`microsoft/OpenAPI.NET`](https://github.com/microsoft/OpenAPI.NET) to
[`microsoft/OpenAPI.NET.OData`](https://github.com/microsoft/OpenAPI.NET.OData)
without changing the NuGet package identity, `hidi` command name, public behavior, or
`mcr.microsoft.com/openapi/hidi` image location.

The move puts Hidi beside the OData conversion library it directly consumes and
reverses the current dependency arrangement:

- Hidi will reference `Microsoft.OpenApi.OData` as a project in the destination repo.
- Hidi will consume `Microsoft.OpenApi` and `Microsoft.OpenApi.YamlReader` as released
  packages instead of projects from the source repo.

## Agreed migration model

| Decision | Approach |
| --- | --- |
| Release versioning | Give Hidi an independent version and `hidi-v*` tag stream in the OData repo. |
| Cutover | Use a destination-first, two-phase cutover. Publish successfully from OData before removing Hidi from OpenAPI.NET. |
| Git history | Import an audited, path-normalized Hidi history with `git filter-repo`, preserving authorship, dates, messages, renames, and useful blame. |
| Signing | Preserve Hidi's existing strong-name identity so it retains friend access to released `Microsoft.OpenApi` packages. |
| Distribution | Move NuGet tool, Windows executable artifact, GitHub release asset, and MCR nightly/release image publishing. |

## Delivery outline

1. Establish package/version boundaries in OpenAPI.NET.OData so its library releases
   remain on `v*` tags while Hidi releases use `hidi-v*`.
2. Filter Hidi's source and test history from a disposable OpenAPI.NET clone, normalize
   legacy paths, audit the result, and merge it into OpenAPI.NET.OData.
3. Update project references, signing, solution membership, documentation, Docker
   support, local build tooling, and repository links in destination integration
   commits after the history import.
4. Extend OData GitHub Actions and ADO YAML to build and test Hidi on every relevant
   change, sign its binaries/packages, and publish only on the matching Hidi tag.
5. Authorize the OData ADO pipeline for the existing NuGet, ESRP, GitHub, and ACR
   connections and configure required checks/triggers.
6. Validate history integrity, package contents, CLI behavior, Docker behavior, and
   both release paths;
   publish one Hidi version from the destination.
7. Remove Hidi projects and all Hidi-specific CI/CD, Docker, local tooling, and
   documentation from OpenAPI.NET, replacing user-facing links with the new location.

## Key safeguards

- Never enable both repositories to publish the same Hidi version.
- Baseline the initial history filter at OpenAPI.NET commit
  `afd4967a9e6db390175e2df9e6f34ff77168d19d`. If source HEAD advances before
  execution, deliberately re-baseline and repeat the history/tree audit.
- Perform filtering only in a disposable mirror/clone; never rewrite either working
  repository in place.
- Retain legacy source paths (`src/Microsoft.OpenApi.Tool`, `src/Microsoft.Hidi`, and
  `src/Microsoft.OpenApi.Hidi`) and both historical test locations, then normalize them
  to the destination layout.
- Exclude OpenAPI.NET branches and tags from the import. Filtering rewrites commit IDs
  and signatures, so retain a source-to-filtered commit map for traceability.
- Audit the filtered graph for unrelated files, generated output, secrets, oversized
  objects, and correct rename traversal before merging it with
  `--allow-unrelated-histories`.
- Do not resurrect the removed `Microsoft.OpenApi.Workbench` project or its solution,
  README, image, VS Code, or pipeline integration. Workbench removal is part of the
  source baseline and is unrelated to Hidi relocation.
- Do not derive the Hidi version from OData's repository-wide `Directory.Build.props`;
  this would regress Hidi from the source repo's `3.10.x` stream to OData's `3.2.x`
  stream and would couple unrelated releases.
- Keep package ID `Microsoft.OpenApi.Hidi`, tool command `hidi`, namespaces, supported
  target framework, and MCR repository unchanged.
- Gate NuGet and release-image publishing on `refs/tags/hidi-v*`; keep OData publishing
  gated on `refs/tags/v*`.
- Keep the old source-repo publishing path available until the destination pipeline
  has produced and smoke-tested a real release.

## Completion criteria

The relocation is complete when Hidi's audited history supports useful blame and rename
traversal in OpenAPI.NET.OData, Hidi builds and tests there, a destination-created
package installs and executes as `hidi`, both MCR architectures are available under the
existing image name, destination release automation owns future Hidi versions, and
OpenAPI.NET no longer builds, tests, packages, publishes, or documents Hidi as a
resident project.

See [HIDI-RELOCATION-IMPLEMENTATION-PLAN.md](./HIDI-RELOCATION-IMPLEMENTATION-PLAN.md)
for the detailed implementation and cutover checklist.
