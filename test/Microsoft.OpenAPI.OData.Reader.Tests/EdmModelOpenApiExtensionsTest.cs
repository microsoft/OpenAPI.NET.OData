// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.IO;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.OpenApi.OData.Tests;
public class EdmModelOpenApiExtensionsTest(ITestOutputHelper output)
{
    private readonly ITestOutputHelper _output = output;

    [Fact]
    public void ConvertToOpenApiThrowsArgumentNullModel()
    {
        // Arrange
        IEdmModel model = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>("model", model.ConvertToOpenApi);
    }

    [Theory]
    [InlineData(OpenApiSpecVersion.OpenApi2_0, OpenApiFormat.Json)]
    [InlineData(OpenApiSpecVersion.OpenApi3_0, OpenApiFormat.Json)]
    [InlineData(OpenApiSpecVersion.OpenApi3_1, OpenApiFormat.Json)]
    [InlineData(OpenApiSpecVersion.OpenApi2_0, OpenApiFormat.Yaml)]
    [InlineData(OpenApiSpecVersion.OpenApi3_0, OpenApiFormat.Yaml)]
    [InlineData(OpenApiSpecVersion.OpenApi3_1, OpenApiFormat.Yaml)]
    public async Task EmptyEdmModelToOpenApiWorks(OpenApiSpecVersion specVersion, OpenApiFormat format)
    {
        // Arrange
        IEdmModel model = EdmModelHelper.EmptyModel;
        var openApiConvertSettings = new OpenApiConvertSettings
        {
            OpenApiSpecVersion = specVersion,
            IncludeAssemblyInfo = false
        };

        // Act
        string result = await WriteEdmModelAsOpenApi(model, format, openApiConvertSettings);
        var fileName = $"Empty.OpenApi.{GetVersion(specVersion)}{GetFormatExt(format)}";

        // Assert
        AssertDocumentsAreEqual(result, fileName, format);
    }

    [Theory]
    [InlineData(OpenApiSpecVersion.OpenApi2_0, OpenApiFormat.Json)]
    [InlineData(OpenApiSpecVersion.OpenApi3_0, OpenApiFormat.Json)]
    [InlineData(OpenApiSpecVersion.OpenApi3_1, OpenApiFormat.Json)]
    [InlineData(OpenApiSpecVersion.OpenApi2_0, OpenApiFormat.Yaml)]
    [InlineData(OpenApiSpecVersion.OpenApi3_0, OpenApiFormat.Yaml)]
    [InlineData(OpenApiSpecVersion.OpenApi3_1, OpenApiFormat.Yaml)]
    public async Task BasicEdmModelToOpenApiWorks(OpenApiSpecVersion specVersion, OpenApiFormat format)
    {
        // Arrange
        IEdmModel model = EdmModelHelper.BasicEdmModel;
        var openApiConvertSettings = new OpenApiConvertSettings
        {
            OpenApiSpecVersion = specVersion,
            ShowSchemaExamples = true, // test for schema examples
            IncludeAssemblyInfo = false,
            UseStringArrayForQueryOptionsSchema = false
        };

        // Act
        string result = await WriteEdmModelAsOpenApi(model, format, openApiConvertSettings);
        var fileName = $"Basic.OpenApi.{GetVersion(specVersion)}{GetFormatExt(format)}";

        // Assert
        AssertDocumentsAreEqual(result, fileName, format);
    }

    [Theory]
    [InlineData(OpenApiSpecVersion.OpenApi2_0, OpenApiFormat.Json)]
    [InlineData(OpenApiSpecVersion.OpenApi3_0, OpenApiFormat.Json)]
    [InlineData(OpenApiSpecVersion.OpenApi3_1, OpenApiFormat.Json)]
    [InlineData(OpenApiSpecVersion.OpenApi2_0, OpenApiFormat.Yaml)]
    [InlineData(OpenApiSpecVersion.OpenApi3_0, OpenApiFormat.Yaml)]
    [InlineData(OpenApiSpecVersion.OpenApi3_1, OpenApiFormat.Yaml)]
    public async Task MultipleSchemasEdmModelToOpenApiWorks(OpenApiSpecVersion specVersion, OpenApiFormat format)
    {
        // Arrange
        IEdmModel model = EdmModelHelper.MultipleSchemasEdmModel;
        var openApiConvertSettings = new OpenApiConvertSettings
        {
            OpenApiSpecVersion = specVersion,
            ShowLinks = true, // test Links
            ShowSchemaExamples = true,
            IncludeAssemblyInfo = false,
            UseStringArrayForQueryOptionsSchema = false
        };

        // Act
        string result = await WriteEdmModelAsOpenApi(model, format, openApiConvertSettings);

        var fileName = $"Multiple.Schema.OpenApi.{GetVersion(specVersion)}{GetFormatExt(format)}";

        // Assert
        AssertDocumentsAreEqual(result, fileName, format);
    }

    [Theory]
    [InlineData(OpenApiSpecVersion.OpenApi2_0, OpenApiFormat.Json)]
    [InlineData(OpenApiSpecVersion.OpenApi3_0, OpenApiFormat.Json)]
    [InlineData(OpenApiSpecVersion.OpenApi3_1, OpenApiFormat.Json)]
    [InlineData(OpenApiSpecVersion.OpenApi2_0, OpenApiFormat.Yaml)]
    [InlineData(OpenApiSpecVersion.OpenApi3_0, OpenApiFormat.Yaml)]
    [InlineData(OpenApiSpecVersion.OpenApi3_1, OpenApiFormat.Yaml)]
    public async Task TripServiceMetadataToOpenApiWorks(OpenApiSpecVersion specVersion, OpenApiFormat format)
    {
        // Arrange
        IEdmModel model = EdmModelHelper.TripServiceModel;
        OpenApiConvertSettings settings = new OpenApiConvertSettings
        {
            EnableKeyAsSegment = true,
            SemVerVersion = "1.0.1",
            ServiceRoot = new Uri("http://services.odata.org/TrippinRESTierService"),
            IEEE754Compatible = true,
            OpenApiSpecVersion = specVersion,
            AddSingleQuotesForStringParameters = true,
            AddEnumDescriptionExtension = true,
            AppendBoundOperationsOnDerivedTypeCastSegments = true,
            IncludeAssemblyInfo = false
        };
        // Act
        string result = await WriteEdmModelAsOpenApi(model, format, settings);

        var fileName = $"TripService.OpenApi.{GetVersion(specVersion)}{GetFormatExt(format)}";

        // Assert
        AssertDocumentsAreEqual(result, fileName, format);
    }

    private void AssertDocumentsAreEqual(string result, string fileName, OpenApiFormat format)
    {
        _output.WriteLine(result);
        var expected = Resources.GetString(fileName);
        if (format is OpenApiFormat.Json)
        {
            var parsedJson = JsonNode.Parse(result);
            Assert.True(JsonNode.DeepEquals(JsonNode.Parse(expected), parsedJson));
        }
        else
        {
            Assert.Equal(expected.ChangeLineBreaks(), result);
        }
    }

    private static string GetFormatExt(OpenApiFormat format) =>
    format switch {
        OpenApiFormat.Json => "json",
        OpenApiFormat.Yaml => "yaml",
        _ => throw new NotImplementedException()
    };

    private static string GetVersion(OpenApiSpecVersion version) =>
    version switch {
        OpenApiSpecVersion.OpenApi2_0 => "V2.",
        OpenApiSpecVersion.OpenApi3_0 => string.Empty,
        OpenApiSpecVersion.OpenApi3_1 => "V3.1.",
        _ => throw new NotImplementedException()
    };

    private static async Task<string> WriteEdmModelAsOpenApi(IEdmModel model, OpenApiFormat target,
        OpenApiConvertSettings settings = null)
    {
        settings ??= new OpenApiConvertSettings();
        var document = model.ConvertToOpenApi(settings);
        Assert.NotNull(document); // guard

        MemoryStream stream = new();
        await document.SerializeAsync(stream, settings.OpenApiSpecVersion, target);
        await stream.FlushAsync();
        stream.Position = 0;
        return await new StreamReader(stream).ReadToEndAsync();
    }
}
