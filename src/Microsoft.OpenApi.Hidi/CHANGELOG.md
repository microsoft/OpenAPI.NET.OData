# Hidi changelog

## [2.13.0](https://github.com/microsoft/OpenAPI.NET.OData/compare/hidi-v2.12.2...hidi-v2.13.0) (2026-10-09)


### Features

* Add writer settings to enable collection sorting using a comparer ([94dbfa0](https://github.com/microsoft/OpenAPI.NET.OData/commit/94dbfa0204ad267cec6dbc38cd67412bc20117da))
* enable null reference type support ([#2146](https://github.com/microsoft/OpenAPI.NET.OData/issues/2146)) ([a0273ad](https://github.com/microsoft/OpenAPI.NET.OData/commit/a0273ad10f71897a3cddbfc09602b7cb2caf7eb5))
* hidi validate command now logs warnings ([da8c888](https://github.com/microsoft/OpenAPI.NET.OData/commit/da8c888c8d8a26d2fd1bdddb225e00433035df64))
* hidi validate command now logs warnings ([aba21ce](https://github.com/microsoft/OpenAPI.NET.OData/commit/aba21cef5356e45df26226967c99738d532b1468))
* **hidi:** import filtered support/v2 history ([8ebc6ab](https://github.com/microsoft/OpenAPI.NET.OData/commit/8ebc6abbb1da683db4b5bc0928183dec947f2f02))
* openapiformat enum cleanup ([#2326](https://github.com/microsoft/OpenAPI.NET.OData/issues/2326)) ([efc6d77](https://github.com/microsoft/OpenAPI.NET.OData/commit/efc6d77f82a06a84afc2673f0af2774c5a094b31))
* Remove default collection initialization for perf reasons ([#2284](https://github.com/microsoft/OpenAPI.NET.OData/issues/2284)) ([134c598](https://github.com/microsoft/OpenAPI.NET.OData/commit/134c598c794f98ac5eff9d103d99a44d7f8945a5))
* upgrades OData lib in Hidi to preview15 ([084c2c9](https://github.com/microsoft/OpenAPI.NET.OData/commit/084c2c9035af876e14ae6f43e7f66a26e721265e))
* upgrades OData lib to preview15 ([b9557d0](https://github.com/microsoft/OpenAPI.NET.OData/commit/b9557d03aceef95ff1d408694cedd80a83757517))
* upgrades openapi.net.odata and apimanifest to the latest version ([dce6708](https://github.com/microsoft/OpenAPI.NET.OData/commit/dce67085091a7ecb06bf7c6111f58f735743636f))
* use http method object instead of enum ([313fafe](https://github.com/microsoft/OpenAPI.NET.OData/commit/313fafe880b2ff7ab639423a49dfcfd591f63197))


### Bug Fixes

* an issue where deprecation extension parsing would fail ([5548da3](https://github.com/microsoft/OpenAPI.NET.OData/commit/5548da3c38db3a155c847ed47ebde3f686e3a0ba))
* bumps openapi.net.odata to fix two critical bugs in hidi ([2722ac2](https://github.com/microsoft/OpenAPI.NET.OData/commit/2722ac2f726e435c5e80784d6ca7928ebfae1f19))
* bumps openapi.net.odata to fix two critical bugs in hidi ([9f11d27](https://github.com/microsoft/OpenAPI.NET.OData/commit/9f11d27de4300bc1657a739447de0fa85c20bf54))
* callback reference proxy implementation ([979c386](https://github.com/microsoft/OpenAPI.NET.OData/commit/979c386ee47e418419c3569a62f37c291581cbb2))
* callback reference proxy implementation ([0f401be](https://github.com/microsoft/OpenAPI.NET.OData/commit/0f401be84a1f4fa4a770847dad02db3962ca5d5c))
* **ci:** hand off hidi Docker context through a pipeline artifact ([b5736df](https://github.com/microsoft/OpenAPI.NET.OData/commit/b5736df5e07ee801075f84451313d39d5ff4e335))
* **ci:** safely restore v2 Hidi Docker dependencies from approved feed ([e7372e4](https://github.com/microsoft/OpenAPI.NET.OData/commit/e7372e4609ac9a10485fe8e56808e441f108c2f0))
* **ci:** safely restore v2 Hidi Docker dependencies from approved feed ([34aae73](https://github.com/microsoft/OpenAPI.NET.OData/commit/34aae7388fcf02b0e0e83d845cf7c9101d5734f8))
* **ci:** use a safe artifact for the hidi container release handoff ([ba2f31f](https://github.com/microsoft/OpenAPI.NET.OData/commit/ba2f31f47d9fb84b30df2f67cbe98c5d26fc69f5))
* hidi fails to parse yaml files when fixing references ([336446d](https://github.com/microsoft/OpenAPI.NET.OData/commit/336446d7819b6462887f9724228d364c600d8e6d))
* **hidi:** automate independent v2 version PRs ([a838bc4](https://github.com/microsoft/OpenAPI.NET.OData/commit/a838bc4ce5ace50000c56d4bf627cf3b63931d9f))
* **hidi:** separate v2 versions and route releases by component tag ([7ab04f2](https://github.com/microsoft/OpenAPI.NET.OData/commit/7ab04f294e34d397e47dc4a99e54d7ed6724e82f))
* **hidi:** update Microsoft.OpenApi.OData to 2.2.1 ([2cdbfbe](https://github.com/microsoft/OpenAPI.NET.OData/commit/2cdbfbe690e9d475a4698672352bacb1122fd2f5))
* **hidi:** update Microsoft.OpenApi.OData to 2.2.1 ([0bfab03](https://github.com/microsoft/OpenAPI.NET.OData/commit/0bfab03431adaae241c3269c92566b4e3337000e)), closes [#2812](https://github.com/microsoft/OpenAPI.NET.OData/issues/2812)
* **hidi:** use standard multi-component release configuration ([9b21c9a](https://github.com/microsoft/OpenAPI.NET.OData/commit/9b21c9a2af8acc612d8f98a9961d835a07932b59))
* inconsistant API surface usage ([131a86d](https://github.com/microsoft/OpenAPI.NET.OData/commit/131a86df8f4326754157ebd7aad8f81d1ca2bbfa))
* migration of hidi to the latest version of system.commandline ([d7700cc](https://github.com/microsoft/OpenAPI.NET.OData/commit/d7700cc70582190696975121ad17a7f3b028506b))
* Open API header proxy design pattern implementation ([0c775c1](https://github.com/microsoft/OpenAPI.NET.OData/commit/0c775c18a7c9f8436313ea225806b6da8f421f12))
* open API link reference proxy design pattern implementation ([9e49632](https://github.com/microsoft/OpenAPI.NET.OData/commit/9e4963288657580328a3347d6d95c23794a1e1e3))
* open API link reference proxy design pattern implementation ([4133fd3](https://github.com/microsoft/OpenAPI.NET.OData/commit/4133fd3d8c22ded9615919ceac962ae7ab7b7fbd))
* open api schema reference proxy design pattern implementation ([e6abee5](https://github.com/microsoft/OpenAPI.NET.OData/commit/e6abee5cda99659255c7daecbf6a754ddebe78fd))
* open api schema reference proxy design pattern implementation ([73dc49f](https://github.com/microsoft/OpenAPI.NET.OData/commit/73dc49f6b670c1f31138dcfce019c79d0abc109d))
* parameter reference proxy design pattern implementation ([15b587d](https://github.com/microsoft/OpenAPI.NET.OData/commit/15b587d0951860ac7708a6beb08b6664f9861a63))
* parameter reference proxy design pattern implementation ([ace09e0](https://github.com/microsoft/OpenAPI.NET.OData/commit/ace09e0bca13e4cc9fc0692e5f9e58afa85fd6b1))
* path item reference implementation ([ec1d0ae](https://github.com/microsoft/OpenAPI.NET.OData/commit/ec1d0ae8ee31b6b2bd5bd48d707b99a606002662))
* path item reference implementation ([e4bb070](https://github.com/microsoft/OpenAPI.NET.OData/commit/e4bb0701e2bba62fb722cb08d90cce2ddec9d4bf))
* proxy design pattern implementation for request body ([97e84e0](https://github.com/microsoft/OpenAPI.NET.OData/commit/97e84e0bd61442aa3342f971b0950d1e3b6a7e93))
* removes nullable property that shouldn't be part of dom ([ef7eb17](https://github.com/microsoft/OpenAPI.NET.OData/commit/ef7eb17319beb1cbd481a04d209e4f47225c53a0))
* removes public mermaid types that were not usuable ([4c33a02](https://github.com/microsoft/OpenAPI.NET.OData/commit/4c33a02e6615535da1e4b8b1afafd0f1b94140e9))
* removes static readers registry ([d6291f4](https://github.com/microsoft/OpenAPI.NET.OData/commit/d6291f413ece30ad98e6933c880a570b3da520f0))
* revert to using IDictionary for collections ([cc4c56e](https://github.com/microsoft/OpenAPI.NET.OData/commit/cc4c56e2dea63028b7cbdb870f8189052835d665))
* sets hidi version to a preview ([808d709](https://github.com/microsoft/OpenAPI.NET.OData/commit/808d70949c537723667b06829f2943c487c97069))
* sets hidi version to a preview ([6aadf90](https://github.com/microsoft/OpenAPI.NET.OData/commit/6aadf90a66b599cf6e62d9244d71796e73c0b50c))
* support non-standard MIME type in response header ([6de8103](https://github.com/microsoft/OpenAPI.NET.OData/commit/6de81038526a8e382f7e371b31d72b371412b2e3))
* upgrades openapi.odata to avoid hidi failing to load ([24ac6dc](https://github.com/microsoft/OpenAPI.NET.OData/commit/24ac6dca7574f52fc65f19fd913445a8e8710827))
* upgrades openapi.odata to avoid hidi failing to load ([d16a907](https://github.com/microsoft/OpenAPI.NET.OData/commit/d16a907ddf55b7be3d6cd74c8649c78af38b4349))
* use a single http client in hidi ([5e9d3ce](https://github.com/microsoft/OpenAPI.NET.OData/commit/5e9d3ce5b8f0d9abb9cdebe469e0bc4432f815be))
* use settings for terse output in serialization extension methods ([9a67cd7](https://github.com/microsoft/OpenAPI.NET.OData/commit/9a67cd7862075bfbb19272f9664c2eccbb1622c3))
* use settings for terse output in serialization extension methods ([77a6747](https://github.com/microsoft/OpenAPI.NET.OData/commit/77a67475c939915f0fd888c65da5cb0fa6721c47))

## 2.12.2

Migration baseline from OpenAPI.NET `support/v2`. This version is already
published and must not be republished from this repository. Earlier release
notes remain in [OpenAPI.NET releases](https://github.com/microsoft/OpenAPI.NET/releases).

Hidi has its own component entry in `.release-please-manifest.json` and
`hidi-v2.*` tags, independent of OData.
Destination publishing remains disabled until the source publishing cutover.
