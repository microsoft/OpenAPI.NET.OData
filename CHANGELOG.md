# Changelog

## [3.3.0](https://github.com/microsoft/OpenAPI.NET.OData/compare/v3.2.1...v3.3.0) (2026-10-09)


### Features

* **hidi:** import filtered main history ([5574ccc](https://github.com/microsoft/OpenAPI.NET.OData/commit/5574ccc6d8d129d7a49729619fac3e3e36d334fd))


### Bug Fixes

* **hidi:** authenticate official Docker restores ([59c8956](https://github.com/microsoft/OpenAPI.NET.OData/commit/59c8956c8ef819d9722f042b4a7e7144f3f75cbf))
* **hidi:** authenticate official Docker restores via BuildKit secrets ([a86a146](https://github.com/microsoft/OpenAPI.NET.OData/commit/a86a14613e4e1cfff966aed55248a4f0e06b658b))
* **hidi:** automate independent main version PRs ([ec73824](https://github.com/microsoft/OpenAPI.NET.OData/commit/ec73824ba13d70538a30a31a11b8430519592079))
* **hidi:** bootstrap version labels and isolate legacy tag history ([6d21bbe](https://github.com/microsoft/OpenAPI.NET.OData/commit/6d21bbecdc813480b2366a67222ce92ca033eb53))
* **hidi:** preserve main ESRP NuGet release handoff ([db4c6dc](https://github.com/microsoft/OpenAPI.NET.OData/commit/db4c6dc68c72aa8ab949c1ddfa13004ceaa05446))
* **hidi:** preserve main ESRP NuGet release handoff ([125e4ff](https://github.com/microsoft/OpenAPI.NET.OData/commit/125e4ff6f92ac26bb2771ef0469b08be1d0a2313))
* **hidi:** track independent versions with standard release-please components ([0b8e521](https://github.com/microsoft/OpenAPI.NET.OData/commit/0b8e521a1c7fc8be74d9070ab7229d66da5d9549))
* **hidi:** use standard multi-component release-please config ([6c34176](https://github.com/microsoft/OpenAPI.NET.OData/commit/6c34176c33645548608544c6e5e3c5133f9567b4))
* **release:** publish only the component selected by its tag ([e686e8c](https://github.com/microsoft/OpenAPI.NET.OData/commit/e686e8c6730ad9ddc8ee2e8931e71c695b39fe38))

## [3.2.1](https://github.com/microsoft/OpenAPI.NET.OData/compare/v3.2.0...v3.2.1) (2026-04-14)


### Bug Fixes

* TryAddPath exact-match check for duplicate bound operation paths ([c49b868](https://github.com/microsoft/OpenAPI.NET.OData/commit/c49b8688abb3eeebac1ef4cb7806e72daccc6a41)), closes [#807](https://github.com/microsoft/OpenAPI.NET.OData/issues/807)
* TryAddPath exact-match check for duplicate bound operation paths (OpenAPI 3.2) ([8b75828](https://github.com/microsoft/OpenAPI.NET.OData/commit/8b758281ac2b287895237ea8864b94a41d7dd4fd))

## [3.2.0](https://github.com/microsoft/OpenAPI.NET.OData/compare/v3.1.0...v3.2.0) (2026-03-19)


### Features

* add int32 format configuration for pagination parameters and count responses ([#793](https://github.com/microsoft/OpenAPI.NET.OData/issues/793)) ([0dd9445](https://github.com/microsoft/OpenAPI.NET.OData/commit/0dd9445e792cadcc9125ab2b675d58b222b009dd)), closes [#792](https://github.com/microsoft/OpenAPI.NET.OData/issues/792)
* add switch to use put as default update verb ([ed79e42](https://github.com/microsoft/OpenAPI.NET.OData/commit/ed79e42a664450cf9a165e022ec1eefa9e840515))

## [3.1.0](https://github.com/microsoft/OpenAPI.NET.OData/compare/v3.0.0...v3.1.0) (2026-01-16)


### Features

* allow optional body parameter ([#773](https://github.com/microsoft/OpenAPI.NET.OData/issues/773)) ([61e4de8](https://github.com/microsoft/OpenAPI.NET.OData/commit/61e4de83021105b873f48263c872a816ba3f2d07))

## [3.0.0](https://github.com/microsoft/OpenAPI.NET.OData/compare/v2.0.0...v3.0.0) (2025-11-12)


### ⚠ BREAKING CHANGES

* adds support for OpenAPI 3.2.0

### Features

* adds support for OpenAPI 3.2.0 ([f5f69eb](https://github.com/microsoft/OpenAPI.NET.OData/commit/f5f69ebc29530891862ca7f9ce57ffbfb79c9d92))

## [2.0.0](https://github.com/microsoft/OpenAPI.NET.OData/compare/v2.0.0-preview.18...v2.0.0) (2025-07-10)


### Features

* general availability of version 2 🎉🎉🎉 ([abe5188](https://github.com/microsoft/OpenAPI.NET.OData/commit/abe518896160b574787a2634489091f59142c22f))
* upgrades to Microsoft.OpenAPI GA ([254d37c](https://github.com/microsoft/OpenAPI.NET.OData/commit/254d37c8718f1308ff06bb831f3cce2c827b09cf))

## [2.0.0-preview.18](https://github.com/microsoft/OpenAPI.NET.OData/compare/v2.0.0-preview.17...v2.0.0-preview.18) (2025-07-02)


### Bug Fixes

* a bug where checking the whether a type is referenced would lead to a null reference exception ([39bbbc2](https://github.com/microsoft/OpenAPI.NET.OData/commit/39bbbc23ca4c12b0c117f8016d18542f82af10fd))
* a bug where checking the whether a type is referenced would lead to a null reference exception ([f0dc51d](https://github.com/microsoft/OpenAPI.NET.OData/commit/f0dc51d544f5727ad813cbc30385837a92ad8d73))
* a bug where empty enums would make the conversion fail ([b3a645b](https://github.com/microsoft/OpenAPI.NET.OData/commit/b3a645bd7b9302f4a1e64937d475a5f024144d4b))
* a bug where empty enums would make the conversion fail ([1abcf49](https://github.com/microsoft/OpenAPI.NET.OData/commit/1abcf4971d985c0ebd5a267842d6254a13a5b90d))

## [2.0.0-preview.17](https://github.com/microsoft/OpenAPI.NET.OData/compare/v2.0.0-preview.16...v2.0.0-preview.17) (2025-07-02)


### Bug Fixes

* trigger a new release to unblock hidi ([60f887e](https://github.com/microsoft/OpenAPI.NET.OData/commit/60f887e871c1031c70f82110ced332188f832e74))
* trigger a new release to unblock hidi ([f236734](https://github.com/microsoft/OpenAPI.NET.OData/commit/f236734e73d70b744c0604c3019f3d8a77dee955))

## [2.0.0-preview.16](https://github.com/microsoft/OpenAPI.NET.OData/compare/v2.0.0-preview.15...v2.0.0-preview.16) (2025-06-10)


### Features

* upgrades openapi.net.odata to the latest version ([743c85d](https://github.com/microsoft/OpenAPI.NET.OData/commit/743c85ddf149a3472279ccf19c21e8e5a9315244))

## [2.0.0-preview.15](https://github.com/microsoft/OpenAPI.NET.OData/compare/v2.0.0-preview.14...v2.0.0-preview.15) (2025-06-03)


### Features

* upgrade oai.net ([cdb20af](https://github.com/microsoft/OpenAPI.NET.OData/commit/cdb20af267ee53bee2fb90d0129905374be52a7f))
* upgrades OpenApi.NET to preview22 ([8253d67](https://github.com/microsoft/OpenAPI.NET.OData/commit/8253d67478db1ae8146483de51f7105abfe99001))

## [2.0.0-preview.14](https://github.com/microsoft/OpenAPI.NET.OData/compare/v2.0.0-preview.13...v2.0.0-preview.14) (2025-05-14)


### Features

* upgrades OpenApi.Net to preview18 ([16e559c](https://github.com/microsoft/OpenAPI.NET.OData/commit/16e559cd5ce907b9eff4b8ed8d8a683a3766fa1e))
* upgrades OpenApi.Net to preview18 ([c7cc418](https://github.com/microsoft/OpenAPI.NET.OData/commit/c7cc418fddc7a26db52216f6c22a8e49ef1bbc90))

## [2.0.0-preview.13](https://github.com/microsoft/OpenAPI.NET.OData/compare/v2.0.0-preview.12...v2.0.0-preview.13) (2025-04-17)


### Features

* upgrades to OAI.net preview17 ([08b5efc](https://github.com/microsoft/OpenAPI.NET.OData/commit/08b5efcea3ad3c8dae4404a8d00218070080cd47))
* upgrades to OAI.net preview17 ([6e38b40](https://github.com/microsoft/OpenAPI.NET.OData/commit/6e38b406ad63d74d3a13a0397707d6ef242bdbe8))

## [2.0.0-preview.12](https://github.com/microsoft/OpenAPI.NET.OData/compare/v2.0.0-preview.11...v2.0.0-preview.12) (2025-04-02)


### Features

* enables null reference types for the library ([93f46f0](https://github.com/microsoft/OpenAPI.NET.OData/commit/93f46f09b2204e3f18ec19b68850ec7acd209e65))


### Bug Fixes

* filter out not found alternate keys properties ([2f6459e](https://github.com/microsoft/OpenAPI.NET.OData/commit/2f6459e30fc08b3f8617bdc182f72d3fbb8f5895))
* filter out not found alternate keys properties ([6672d76](https://github.com/microsoft/OpenAPI.NET.OData/commit/6672d76724c188957c1aea2069bf49640c2f7b15))

## [2.0.0-preview.11](https://github.com/microsoft/OpenAPI.NET.OData/compare/v2.0.0-preview.10...v2.0.0-preview.11) (2025-03-19)


### Bug Fixes

* bump oai version ([caa2055](https://github.com/microsoft/OpenAPI.NET.OData/commit/caa2055e039298d4471f265797b6f6be381a03ce))
* bump oai version ([d40746c](https://github.com/microsoft/OpenAPI.NET.OData/commit/d40746c055af4d7d7a5ae8fec3baeb862ab8ad56))

## [2.0.0-preview.10](https://github.com/microsoft/OpenAPI.NET.OData/compare/v2.0.0-preview10...v2.0.0-preview.10) (2025-03-18)


### Miscellaneous Chores

* release 2.0.0-preview.10 ([5cd79d7](https://github.com/microsoft/OpenAPI.NET.OData/commit/5cd79d76cc63ab3c311351f8e661b2bd7b88b35b))

## [2.0.0-preview10](https://github.com/microsoft/OpenAPI.NET.OData/compare/v2.0.0-preview9...v2.0.0-preview10) (2025-03-18)


### Features

* bump openapi.net packages to the latest preview. ([d8b9f3f](https://github.com/microsoft/OpenAPI.NET.OData/commit/d8b9f3f15a2586646fb78bf803ecf7a11db1d053))
* migrates to the latest preview of OAI.net ([3295eb9](https://github.com/microsoft/OpenAPI.NET.OData/commit/3295eb9faec51b77bfd089a80539a0cbeea41641))


### Bug Fixes

* disable failing test for now. ([6f52acc](https://github.com/microsoft/OpenAPI.NET.OData/commit/6f52acc33d66dd9c55ed6952e8b4472364ccefb7))

## [2.0.0-preview9](https://github.com/microsoft/OpenAPI.NET.OData/compare/v2.0.0-preview8...v2.0.0-preview9) (2025-02-25)


### Bug Fixes

* removes duplicated package reference ([4f7c5aa](https://github.com/microsoft/OpenAPI.NET.OData/commit/4f7c5aadd950964d7a0d23a49a5edf2fd89da7fe))
* removes duplicated package reference ([42957e3](https://github.com/microsoft/OpenAPI.NET.OData/commit/42957e3091dce37c610f5ea1ec49d3ae2d2c8690))

## [2.0.0-preview8](https://github.com/microsoft/openapi.net.odata/compare/v2.0.0-preview7...v2.0.0-preview8) (2025-02-11)


### Features

* adds support for open API 3.1 ([6fbcebc](https://github.com/microsoft/openapi.net.odata/commit/6fbcebc21da90f98ebed1c59049b343f1a03db76))


### Bug Fixes

* target OAS versions ([861cf42](https://github.com/microsoft/openapi.net.odata/commit/861cf42e62ac51295af1d0588a7fdaab8e9b8478))

## Changelog
