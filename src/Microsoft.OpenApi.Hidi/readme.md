# Overview

Hidi is a command line tool that makes it easy to work with and transform OpenAPI documents. The tool enables you validate and apply transformations to and from different file formats using various commands to do different actions on the files.

## Capabilities

Hidi has these key capabilities that enable you to build different scenarios off the tool

	• Validation of OpenAPI files 
	• Conversion of OpenAPI files into different file formats: convert files from JSON to YAML, YAML to JSON
	• Slice or filter OpenAPI documents to smaller subsets using operationIDs and tags
    • Generate a Mermaid diagram of the API from an OpenAPI document

## Installation

Install [Microsoft.OpenApi.Hidi](https://www.nuget.org/packages/Microsoft.OpenApi.Hidi)
from NuGet. Hidi 3.x is maintained on this repository's `main` branch and requires
.NET 8. Hidi 2.x is maintained separately on `support/v2`.

### .NET CLI(Global)

```bash
dotnet tool install --global Microsoft.OpenApi.Hidi --version 3.10.2
```
 
### .NET CLI(local)

```bash
dotnet new tool-manifest # if the repository does not have a tool manifest
dotnet tool install --local Microsoft.OpenApi.Hidi --version 3.10.2
```

### Build and install from this repository

Run these commands from the repository root with the .NET 10 SDK and .NET 8 runtime installed:

```powershell
dotnet pack src\Microsoft.OpenApi.Hidi\Microsoft.OpenApi.Hidi.csproj -c Release -o artifacts\hidi\nuget
.\install-tool.ps1
artifacts\hidi\installed\hidi --help
```

Hidi uses published `Microsoft.OpenApi` and `Microsoft.OpenApi.YamlReader` 3.10.2
packages, and builds the OData converter from this repository. No sibling source
checkout is required. Its version is independent of the OData library version.
The local NuGet configuration excludes remote feeds so this smoke test installs
the newly built artifact, not the already-published package with the same version.

### Tests and coverage

Hidi tests use Microsoft.Testing.Platform; the existing OData tests continue to
use VSTest. Run Hidi tests and collect coverage independently:

```powershell
dotnet run --project test\Microsoft.OpenApi.Hidi.Tests\Microsoft.OpenApi.Hidi.Tests.csproj -c Release -- --minimum-expected-tests 1
dotnet run --project test\Microsoft.OpenApi.Hidi.Tests\Microsoft.OpenApi.Hidi.Tests.csproj -c Release --no-build -- --minimum-expected-tests 1 --coverlet --results-directory artifacts\hidi\coverage
```

The Hidi-only `testconfig.json` explicitly includes the Hidi assembly and excludes
test/generated code. This authoritative configuration prevents Coverlet's
dynamic namespace exclusions from excluding `Microsoft.OpenApi.Hidi` itself.
The Sonar workflow requires an OpenCover report with covered Hidi sequence points.
Repository-wide MTP migration is tracked separately in
[#885](https://github.com/microsoft/OpenAPI.NET.OData/issues/885); this migration
does not change the existing OData test platform.

### Official NuGet releases

The gated main Hidi publisher preserves the ESRP NuGet contract from
[microsoft/OpenAPI.NET#3107](https://github.com/microsoft/OpenAPI.NET/pull/3107)
(source commit `e1a75437b76ebfc7c9eb446e9fd0b59a21afb14b`). Official builds include
the signed Hidi `.nupkg`, a matching `.snupkg` and the private-feed version-check
script in the `Hidi` artifact. The `nuget-org` release job consumes that artifact
without a repository checkout or the Docker context.

Before ESRP publication, the job requires the exact `hidi-v3.*` release package
and symbols with a version newer than 3.10.2. It checks
`GraphDeveloperExperiences_Public` using `System.AccessToken` through
`FEED_ACCESS_TOKEN`; only a missing package (HTTP 404) or version permits
publication. Authentication, network and other feed failures stop the job.
An existing version skips ESRP publication but still allows the existing GitHub
release's signed package, executable and ZIP attachments to be updated.

ESRP receives only the exact Hidi package and matching symbols under the
`openapinet` organization, retaining the source's federated DevX connection,
publisher identity, owners and approvers. This handoff does not enable publishing
or grant pipeline permissions. Owners must separately verify private-feed token
access, the ESRP connection and protected `nuget-org` environment authorization
before cutover; local packaging does not prove production signing or publishing.

### Windows executable

```powershell
dotnet publish src\Microsoft.OpenApi.Hidi\Microsoft.OpenApi.Hidi.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PackAsTool=false -p:GeneratePackageOnBuild=false -o artifacts\hidi\win-x64
artifacts\hidi\win-x64\Microsoft.OpenApi.Hidi.exe --help
```

CI produces a Windows executable artifact without publishing a production release.

### Docker

Hidi is also available as a Docker image:

```bash
docker pull mcr.microsoft.com/openapi/hidi
```

Build the destination image locally without pushing it:

```powershell
docker build --tag hidi:local .
docker run --rm hidi:local --help
```

Mount input files and a writable output directory when transforming documents:

```powershell
docker run --rm --mount "type=bind,source=$PWD\test\Microsoft.OpenApi.Hidi.Tests\UtilityFiles\SampleOpenApi.yml,target=/app/openapi.yml,readonly" --mount "type=bind,source=$PWD\artifacts\hidi,target=/app/output" hidi:local transform --openapi /app/openapi.yml --output /app/output/result.json --format json
```

Local/CI builds use the public-only Hidi strong-name identity. Official release
artifacts are signed in Azure Pipelines. Docker builds opt into
`HidiPublicSignBuild=true` for the local OData project, using its own public-only
key without changing normal OData signing. Private `.snk` resources are excluded
from the Docker context and must not be copied into new Hidi resources.

The official build stages a separate `HidiDockerContext` artifact containing only
the Docker build inputs, Hidi/OData source projects and their two public keys,
without `bin`, `obj`, `artifacts`, Git metadata or private signing resources. Stable and preview
container release jobs consume that same build artifact without checking out the
repository, as required by the 1ES release-job policy.
Container jobs retain the source publisher's existing `docker-images-deploy`
environment and its approvals/checks. The destination pipeline must be authorized
for that protected environment before official publishing can be enabled.

Official container jobs authenticate to the approved
`GraphDeveloperExperiences_Public` central feed using the existing Azure DevOps
job identity. A credential-bearing NuGet config is created with owner-only
permissions in the agent temp directory, passed as the BuildKit `nuget_config`
secret, and removed in `finally`, including on build failure. It is never staged
in `HidiDockerContext`, published as an artifact, or copied into an image layer.
The destination pipeline identity must already be authorized to read the feed;
this handoff does not grant permissions or bypass service-connection approvals.
The Dockerfile secret mount is optional, so local and GitHub Actions builds
without a secret continue to use their default NuGet sources.

The migration baseline 3.10.2 is already published. Destination Hidi NuGet,
GitHub release, stable Docker and preview Docker publishing are disabled until
source cutover. The first destination stable release must advance the Hidi
version; OData releases use separate artifacts and tags.

The gated official pipeline retains the consumer image
`mcr.microsoft.com/openapi/hidi`, backed by
`msgraphprodregistry.azurecr.io/public/openapi/hidi`. Stable images use `latest`
and the independent Hidi version. Main previews use `nightly` and
`VERSION.YYYYMMDDRUNNUM`, for `linux/amd64` and `linux/arm64/v8`.
No image is pushed during this migration. Privileged cross-platform emulation
setup belongs only in the authorized CI pipeline, not a shared local environment.
 
## How to use Hidi

Once you've installed the package locally, you can invoke the Hidi by running: `hidi [command]`. You can access the list of command options we have by running `hidi -h` 
The tool avails the following commands: 

	• Validate  
	• Transform 
	• Show
	 
### Validate

This command option accepts an OpenAPI document as an input parameter, visits multiple OpenAPI elements within the document and returns statistics count report on the following elements: 

	• Path Items  
	• Operations  
	• Parameters  
	• Request bodies 
	• Responses 
	• Links 
	• Callbacks 
	• Schemas 
	 
It accepts the following command: 

	• --openapi(-d) - OpenAPI description file path or URL 
	• --loglevel(-ll) - The log level to use when logging messages to the main output 
	 

#### Example:

```bash
hidi validate --openapi C:\OpenApidocs\Mail.yml --loglevel trace` 
```

> Run `hidi validate -h` to see the options available.

### Transform

Used to convert file formats from JSON to YAML and vice versa and performs slicing of OpenAPI documents. 

This command accepts the following parameters:


	• --openapi, (-d) - OpenAPI description file path in the local filesystem or a valid URL hosted on a HTTPS server 
	• --csdl (--cs) - CSDL file path in the local filesystem or a valid URL hosted on a HTTPS server 
	• --csdl-filter (--csf) - a filter parameter that a user can use to select a subset of a large CSDL file. They do so by providing a comma delimited list of EntitySet and Singleton names that appear in the EntityContainer. 
	• --output (-o) - Output directory path for the transformed document.
	• --clean-output (--co) - an optional param that allows a user to overwrite an existing file.  
	• --version (-v) - OpenAPI specification version (2.0, 3.0, 3.1 or 3.2; defaults to 3.2).
    • --metadata-version (--mv) - the metadata version to use.
	• --format (-f) - File format 
    • --terse-output (--to) - Produce terse json output
    • --settings-path (--sp) - The configuration file with CSDL conversion settings.
	• --log-level (--ll) - The log level to use when logging messages to the main output 
	• --inline-local (--il) - Inline local $ref instances 
	• --inline-external (--ie) - Inline external $refs instances
	• --filter-by-operationids(--op) - Slice document based on OperationId(s) provided. Accepts a comma delimited list of operation ids. 
	• --filter-by-tags (--t) - Slice document based on tag(s) provided. Accepts a comma delimited list of tags. 
	• --filter-by-collection (-c) - Slices the OpenAPI document based on the Postman Collection file generated by Resource Explorer 
 
 #### Examples:  

1. Filtering by OperationIds  

```bash
hidi transform -d files\People.yml -f yaml -o files\People.yml -v 3.0 --op users_UpdateInsights --co 
```

2. Filtering by Postman collection 

```bash	
hidi transform --openapi files\People.yml --format yaml --output files\People2.yml --version 3.0 --filter-by-collection Graph-Collection-0017059134807617005.postman_collection.json 
```

3. CSDL--->OpenAPI conversion and filtering 

```bash
hidi transform --csdl Files/Todo.xml --output Files/Todo-subset.yml --format yaml --version 3.0 --filter-by-operationids Todos.Todo.UpdateTodo 
```	 
	
4. CSDL Filtering by EntitySets and Singletons 

```bash
hidi transform --cs dataverse.csdl --csdl-filter "appointments,opportunities" -o appointmentsAndOpportunities.yaml --ll trace 
```

> Run `hidi transform -h` to see all the available usage options.

### Show

This command accepts an OpenAPI document as an input parameter and generates a Markdown file that contains a diagram of the API using Mermaid syntax.

#### Examples:

```bash
hidi show -d files\People.yml -o People.md -ll trace
```

### Plugin

This command generates an OpenAI style Plugin manifest and minimal OpenAPI file based on the provided API Manifest

#### Examples:

```bash
hidi plugin -m exampleApiManifest.yml -o mypluginfolder 
```


> Run `hidi plugin -h` to see all the available usage options.
