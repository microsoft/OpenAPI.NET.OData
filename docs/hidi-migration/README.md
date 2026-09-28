# Hidi history migration provenance

Hidi was migrated from
[`microsoft/OpenAPI.NET`](https://github.com/microsoft/OpenAPI.NET) using filtered Git
history rather than a source-only copy.

| Item | Value |
| --- | --- |
| Source commit | `afd4967a9e6db390175e2df9e6f34ff77168d19d` |
| Filtered tip before merge | `b9cf23ea24fcc34085796f446eb91a109d1a4944` |
| Destination paths | `src/Microsoft.OpenApi.Hidi`, `test/Microsoft.OpenApi.Hidi.Tests` |
| Reproduction script | [`scripts/import-hidi-history.ps1`](../../scripts/import-hidi-history.ps1) |
| Commit map | [`commit-map.txt`](./commit-map.txt) |

The filter retained the historical `src/Microsoft.OpenApi.Tool`,
`src/Microsoft.Hidi`, `src/Microsoft.OpenApi.Hidi`,
`Microsoft.OpenApi.Hidi.Tests`, and `test/Microsoft.OpenApi.Hidi.Tests` paths and
normalized them to the destination paths.

Filtering preserves commit authors, committers, timestamps, messages, and useful
file-history traversal. It rewrites commit IDs and invalidates source commit signatures.
Commits that also changed unrelated OpenAPI.NET files retain only their Hidi changes.
The commit map relates original commit IDs to rewritten IDs.

OpenAPI.NET branches, tags, Workbench content, and unrelated source files were excluded
from the imported ref.
