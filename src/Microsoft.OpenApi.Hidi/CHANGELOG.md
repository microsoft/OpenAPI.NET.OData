# Hidi changelog

## [3.11.0](https://github.com/microsoft/OpenAPI.NET.OData/compare/hidi-v3.10.2...hidi-v3.11.0) (2026-10-09)


### Features

* adds parsing infrastructure for version 3.2 ([015cbf8](https://github.com/microsoft/OpenAPI.NET.OData/commit/015cbf82aa64922c3dfd416ff2dc4c4b65a3b8c6))
* hidi validate command now logs warnings ([c636714](https://github.com/microsoft/OpenAPI.NET.OData/commit/c636714ad59bbbb0bd1964f95e461f1334c41f19))
* hidi validate command now logs warnings ([da8c888](https://github.com/microsoft/OpenAPI.NET.OData/commit/da8c888c8d8a26d2fd1bdddb225e00433035df64))
* hidi validate command now logs warnings ([aba21ce](https://github.com/microsoft/OpenAPI.NET.OData/commit/aba21cef5356e45df26226967c99738d532b1468))
* hidi validate command now logs warnings ([0b2c8c0](https://github.com/microsoft/OpenAPI.NET.OData/commit/0b2c8c02d2020d0d2111587e09bac8b50e6f7375))
* **hidi:** import filtered main history ([5574ccc](https://github.com/microsoft/OpenAPI.NET.OData/commit/5574ccc6d8d129d7a49729619fac3e3e36d334fd))


### Bug Fixes

* bumps openapi.net.odata to fix two critical bugs in hidi ([2722ac2](https://github.com/microsoft/OpenAPI.NET.OData/commit/2722ac2f726e435c5e80784d6ca7928ebfae1f19))
* bumps openapi.net.odata to fix two critical bugs in hidi ([9f11d27](https://github.com/microsoft/OpenAPI.NET.OData/commit/9f11d27de4300bc1657a739447de0fa85c20bf54))
* **hidi:** authenticate official Docker restores ([59c8956](https://github.com/microsoft/OpenAPI.NET.OData/commit/59c8956c8ef819d9722f042b4a7e7144f3f75cbf))
* **hidi:** authenticate official Docker restores via BuildKit secrets ([a86a146](https://github.com/microsoft/OpenAPI.NET.OData/commit/a86a14613e4e1cfff966aed55248a4f0e06b658b))
* **hidi:** automate independent main version PRs ([ec73824](https://github.com/microsoft/OpenAPI.NET.OData/commit/ec73824ba13d70538a30a31a11b8430519592079))
* **hidi:** hand off Docker context through pipeline artifacts ([82cae10](https://github.com/microsoft/OpenAPI.NET.OData/commit/82cae10897c3ab7e268e9bdc20a3cc75b93271bd))
* **hidi:** place Pester test inside its test project ([#896](https://github.com/microsoft/OpenAPI.NET.OData/issues/896)) ([cb766cb](https://github.com/microsoft/OpenAPI.NET.OData/commit/cb766cb7a16925d1967f92ed9e48a583eaa934aa))
* **hidi:** port v2 review updates to main migration ([a263805](https://github.com/microsoft/OpenAPI.NET.OData/commit/a263805e83daa587791a6899db083d50045ae88e))
* **hidi:** preserve main ESRP NuGet release handoff ([db4c6dc](https://github.com/microsoft/OpenAPI.NET.OData/commit/db4c6dc68c72aa8ab949c1ddfa13004ceaa05446))
* **hidi:** preserve main ESRP NuGet release handoff ([125e4ff](https://github.com/microsoft/OpenAPI.NET.OData/commit/125e4ff6f92ac26bb2771ef0469b08be1d0a2313))
* **hidi:** remove Humanizer.Inflections namespace for Humanizer 3.x compatibility ([94dac6b](https://github.com/microsoft/OpenAPI.NET.OData/commit/94dac6bf53517bc492a9bc31af0d7d8ffca803cb))
* **hidi:** reuse the protected container release environment ([e9ac0b6](https://github.com/microsoft/OpenAPI.NET.OData/commit/e9ac0b6da5fb1e3ac97d25b7498b01e4c6811c16))
* **hidi:** update Microsoft.OpenApi.OData to 3.2.1 ([d637e51](https://github.com/microsoft/OpenAPI.NET.OData/commit/d637e5190eefc2b3af6b78ff7619bccfca095f6c))
* **hidi:** update Microsoft.OpenApi.OData to 3.2.1 ([00da2b3](https://github.com/microsoft/OpenAPI.NET.OData/commit/00da2b34075647dd826c05132dbb522e642b1458)), closes [#2811](https://github.com/microsoft/OpenAPI.NET.OData/issues/2811)
* **hidi:** use standard multi-component release-please config ([6c34176](https://github.com/microsoft/OpenAPI.NET.OData/commit/6c34176c33645548608544c6e5e3c5133f9567b4))
* migration of hidi to the latest version of system.commandline ([d7700cc](https://github.com/microsoft/OpenAPI.NET.OData/commit/d7700cc70582190696975121ad17a7f3b028506b))
* removes public mermaid types that were not usuable ([4c33a02](https://github.com/microsoft/OpenAPI.NET.OData/commit/4c33a02e6615535da1e4b8b1afafd0f1b94140e9))
* upgrades openapi.odata to avoid hidi failing to load ([24ac6dc](https://github.com/microsoft/OpenAPI.NET.OData/commit/24ac6dca7574f52fc65f19fd913445a8e8710827))
* upgrades openapi.odata to avoid hidi failing to load ([d16a907](https://github.com/microsoft/OpenAPI.NET.OData/commit/d16a907ddf55b7be3d6cd74c8649c78af38b4349))
* use settings for terse output in serialization extension methods ([9a67cd7](https://github.com/microsoft/OpenAPI.NET.OData/commit/9a67cd7862075bfbb19272f9664c2eccbb1622c3))
* use settings for terse output in serialization extension methods ([77a6747](https://github.com/microsoft/OpenAPI.NET.OData/commit/77a67475c939915f0fd888c65da5cb0fa6721c47))

## 3.10.2

This is the source migration baseline from OpenAPI.NET. It is already published
and must not be republished from this repository. Hidi 3.x is maintained on
`main`, independently of the OData library and the Hidi 2.x `support/v2` track.
Destination publishing remains disabled until source cutover.
