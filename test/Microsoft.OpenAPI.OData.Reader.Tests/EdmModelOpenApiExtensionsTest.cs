// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.IO;
using System.Text.Json.Nodes;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.OpenApi.OData.Tests
{
    public class EdmModelOpenApiExtensionsTest
    {
        private readonly ITestOutputHelper _output;

        public EdmModelOpenApiExtensionsTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void ConvertToOpenApiThrowsArgumentNullModel()
        {
            // Arrange
            IEdmModel model = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("model", () => model.ConvertToOpenApi());
        }

        [Theory]
        [InlineData(OpenApiSpecVersion.OpenApi2_0, OpenApiFormat.Json)]
        [InlineData(OpenApiSpecVersion.OpenApi3_0, OpenApiFormat.Json)]
        [InlineData(OpenApiSpecVersion.OpenApi2_0, OpenApiFormat.Yaml)]
        [InlineData(OpenApiSpecVersion.OpenApi3_0, OpenApiFormat.Yaml)]
        public void EmptyEdmModelToOpenApiJsonWorks(OpenApiSpecVersion specVersion, OpenApiFormat format)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.EmptyModel;
            var openApiConvertSettings = new OpenApiConvertSettings
            {
                OpenApiSpecVersion = specVersion,
                IncludeAssemblyInfo = false
            };

            // Act
            string result = WriteEdmModelAsOpenApi(model, format, openApiConvertSettings);
            var fileName = $"Empty.OpenApi.{GetVersion(specVersion)}{GetFormatExt(format)}";

            // Assert
            AssertDocumentsAreEqual(result, fileName, format);
        }

        [Theory]
        [InlineData(OpenApiSpecVersion.OpenApi2_0, OpenApiFormat.Json)]
        [InlineData(OpenApiSpecVersion.OpenApi3_0, OpenApiFormat.Json)]
        [InlineData(OpenApiSpecVersion.OpenApi2_0, OpenApiFormat.Yaml)]
        [InlineData(OpenApiSpecVersion.OpenApi3_0, OpenApiFormat.Yaml)]
        public void BasicEdmModelToOpenApiWorks(OpenApiSpecVersion specVersion, OpenApiFormat format)
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
            string result = WriteEdmModelAsOpenApi(model, format, openApiConvertSettings);
            var fileName = $"Basic.OpenApi.{GetVersion(specVersion)}{GetFormatExt(format)}";

            // Assert
            AssertDocumentsAreEqual(result, fileName, format);
        }

        [Theory]
        [InlineData(OpenApiSpecVersion.OpenApi2_0, OpenApiFormat.Json)]
        [InlineData(OpenApiSpecVersion.OpenApi3_0, OpenApiFormat.Json)]
        [InlineData(OpenApiSpecVersion.OpenApi2_0, OpenApiFormat.Yaml)]
        [InlineData(OpenApiSpecVersion.OpenApi3_0, OpenApiFormat.Yaml)]
        public void MultipleSchemasEdmModelToOpenApiWorks(OpenApiSpecVersion specVersion, OpenApiFormat format)
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
            string result = WriteEdmModelAsOpenApi(model, format, openApiConvertSettings);

            var fileName = $"Multiple.Schema.OpenApi.{GetVersion(specVersion)}{GetFormatExt(format)}";

            // Assert
            AssertDocumentsAreEqual(result, fileName, format);
        }

        [Theory]
        [InlineData(OpenApiSpecVersion.OpenApi2_0, OpenApiFormat.Json)]
        [InlineData(OpenApiSpecVersion.OpenApi3_0, OpenApiFormat.Json)]
        [InlineData(OpenApiSpecVersion.OpenApi2_0, OpenApiFormat.Yaml)]
        [InlineData(OpenApiSpecVersion.OpenApi3_0, OpenApiFormat.Yaml)]
        public void TripServiceMetadataToOpenApiWorks(OpenApiSpecVersion specVersion, OpenApiFormat format)
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
            string result = WriteEdmModelAsOpenApi(model, format, settings);

            var fileName = $"TripService.OpenApi.{GetVersion(specVersion)}{GetFormatExt(format)}";

            // Assert
            AssertDocumentsAreEqual(result, fileName, format);
        }

        private void AssertDocumentsAreEqual(string result, string fileName, OpenApiFormat format)
        {
            _output.WriteLine(result);
            if (format is OpenApiFormat.Json)
            {
                var parsedJson = JsonNode.Parse(result);
                Assert.True(JsonNode.DeepEquals(JsonNode.Parse(Resources.GetString(fileName)), parsedJson));
            }
            else
            {
                Assert.Equal(Resources.GetString(fileName).ChangeLineBreaks(), result);
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
            _ => throw new NotImplementedException()
        };

        private static string WriteEdmModelAsOpenApi(IEdmModel model, OpenApiFormat target,
            OpenApiConvertSettings settings = null)
        {
            settings ??= new OpenApiConvertSettings();
            var document = model.ConvertToOpenApi(settings);
            Assert.NotNull(document); // guard

            MemoryStream stream = new();
            document.Serialize(stream, settings.OpenApiSpecVersion, target);
            stream.Flush();
            stream.Position = 0;
            return new StreamReader(stream).ReadToEnd();
        }
    }
}
