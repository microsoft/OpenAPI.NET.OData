
VSTS Build  | Status
--------|---------
Rolling | <img src="https://identitydivision.visualstudio.com/_apis/public/build/definitions/2cfe7ec3-b94f-4ab9-85ab-2ebff928f3fd/410/badge"/>
Nightly | <img src="https://identitydivision.visualstudio.com/_apis/public/build/definitions/2cfe7ec3-b94f-4ab9-85ab-2ebff928f3fd/427/badge"/>

# Convert OData to OpenAPI.NET [Preview]

[**Be noted:This repository is in a preview state. Feedback and contribution is welcome!**]

## Introduction

The **Microsoft.OpenAPI.OData.Reader** SDK contains APIs to convert [OData](http://www.odata.org) [CSDL](http://docs.oasis-open.org/odata/odata-csdl-xml/v4.01/odata-csdl-xml-v4.01.html), the XML represetntation of the Entity Data Model (EDM) to [Open API](https://github.com/OAI/OpenAPI-Specification) based on [OpenAPI.NET](http://aka.ms/openapi) object model.

The converting is based on the mapping doc from [OASIS OData OpenAPI v1.0](https://www.oasis-open.org/committees/document.php?document_id=61852&wg_abbrev=odata) and the following contents:

1. Capabilites annotation
2. Authorization annotation
3. HttpRequest annotation
4. Navigation property path
5. Edm operation and operation import path
6. Others

## Overview

In general, the below image describes the general concept of how this SDK can convert the EDM model to an [OpenAPI.NET document](https://github.com/Microsoft/OpenAPI.NET/blob/master/src/Microsoft.OpenApi/Models/OpenApiDocument.cs) object.

![Convert OData CSDL to OpenAPI](docs/images/odata-2-openapi.png "Map /// OData CSDL --> OpenAPI.NET")

For detail information about the CSDL and Entity Data model, please refer to http://www.odata.org/documentation.
For detail information about the Open API object of model, please refer to http://github.com/microsoft/OpenAPI.NET

## Simple Example Code

Here's a simple example codes illustrating how to use the APIs in this SDK

```csharp
IEdmModel model = GetEdmModel();
OpenApiDocument document = model.ConvertToOpenApi();
```
Or with the convert settings:

```csharp
IEdmModel model = GetEdmModel();
OpenApiConvertSettings settings = new OpenApiConvertSettings
{
   // configuration
};
OpenApiDocument document = model.ConvertToOpenApi(settings);
```

Where, `GetEdmModel()` is a method to return the `IEdmModel` object. You can:

1. Create the Edm model from scratch, see detail [here](http://odata.github.io/odata.net/#02-01-build-basic-model)
2. Load the Edm model from CSDL file, see sample codes below
```csharp
string csdlFile = @"c:\csdl.xml";
string csdl = System.IO.File.ReadAllText(csdlFile);
IEdmModel model = CsdlReader.Parse(XElement.Parse(csdl).CreateReader());
```
3. Create the Edm model using Web API OData model builder, see detail [here](http://odata.github.io/WebApi/#02-01-model-builder-abstract)

## Nightly builds

The nightly build process will upload a Nuget package for OpenAPI.OData.reader to [OpenAPIOData MyGet gallery](https://www.myget.org/gallery/openapiodata).

To connect to OpenAPI.OData.reader feed, use [this](https://www.myget.org/F/openapiodata/api/v3/index.json) URL source.

## Nuget packages

The OpenAPI.OData.reader nuget package is at: https://www.nuget.org/packages/Microsoft.OpenApi.OData/

---
# Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.microsoft.com.

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

