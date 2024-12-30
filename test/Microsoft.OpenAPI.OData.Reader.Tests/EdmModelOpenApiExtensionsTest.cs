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
        [InlineData(OpenApiSpecVersion.OpenApi2_0)]
        [InlineData(OpenApiSpecVersion.OpenApi3_0)]
        public void EmptyEdmModelToOpenApiJsonWorks(OpenApiSpecVersion specVersion)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.EmptyModel;
            var openApiConvertSettings = new OpenApiConvertSettings
            {
                OpenApiSpecVersion = specVersion,
                IncludeAssemblyInfo = false
            };

            // Act
            string json = WriteEdmModelAsOpenApi(model, OpenApiFormat.Json, openApiConvertSettings);
            _output.WriteLine(json);
            var parsedJson = JsonNode.Parse(json);
            var fileName = specVersion switch {
                OpenApiSpecVersion.OpenApi2_0 => "Empty.OpenApi.V2.json",
                OpenApiSpecVersion.OpenApi3_0 => "Empty.OpenApi.json",
                _ => throw new NotImplementedException()
            };

            // Assert
            Assert.True(JsonNode.DeepEquals(JsonNode.Parse(Resources.GetString(fileName)), parsedJson));
        }

        [Theory]
        [InlineData(OpenApiSpecVersion.OpenApi2_0)]
        [InlineData(OpenApiSpecVersion.OpenApi3_0)]
        public void EmptyEdmModelToOpenApiYamlWorks(OpenApiSpecVersion specVersion)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.EmptyModel;
            var openApiConvertSettings = new OpenApiConvertSettings
            {
                OpenApiSpecVersion = specVersion,
                IncludeAssemblyInfo = false
            };

            // Act
            string yaml = WriteEdmModelAsOpenApi(model, OpenApiFormat.Yaml, openApiConvertSettings);
            _output.WriteLine(yaml);

            var fileName = specVersion switch {
                OpenApiSpecVersion.OpenApi2_0 => "Empty.OpenApi.V2.yaml",
                OpenApiSpecVersion.OpenApi3_0 => "Empty.OpenApi.yaml",
                _ => throw new NotImplementedException()
            };

            // Assert
            Assert.Equal(Resources.GetString(fileName).ChangeLineBreaks(), yaml);
        }

        [Theory]
        [InlineData(OpenApiSpecVersion.OpenApi2_0)]
        [InlineData(OpenApiSpecVersion.OpenApi3_0)]
        public void BasicEdmModelToOpenApiJsonWorks(OpenApiSpecVersion specVersion)
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
            string json = WriteEdmModelAsOpenApi(model, OpenApiFormat.Json, openApiConvertSettings);
            _output.WriteLine(json);
            var parsedJson = JsonNode.Parse(json);
            var fileName = specVersion switch {
                OpenApiSpecVersion.OpenApi2_0 => "Basic.OpenApi.V2.json",
                OpenApiSpecVersion.OpenApi3_0 => "Basic.OpenApi.json",
                _ => throw new NotImplementedException()
            };

            // Assert
            Assert.True(JsonNode.DeepEquals(JsonNode.Parse(Resources.GetString(fileName)), parsedJson));
        }

        [Theory]
        [InlineData(OpenApiSpecVersion.OpenApi2_0)]
        [InlineData(OpenApiSpecVersion.OpenApi3_0)]
        public void BasicEdmModelToOpenApiYamlWorks(OpenApiSpecVersion specVersion)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.BasicEdmModel;
            var openApiConvertSettings = new OpenApiConvertSettings
            {
                OpenApiSpecVersion = specVersion,
                ShowSchemaExamples = true,
                IncludeAssemblyInfo = false,
                UseStringArrayForQueryOptionsSchema = false
            };

            // Act
            string yaml = WriteEdmModelAsOpenApi(model, OpenApiFormat.Yaml, openApiConvertSettings);
            _output.WriteLine(yaml);

            var fileName = specVersion switch {
                OpenApiSpecVersion.OpenApi2_0 => "Basic.OpenApi.V2.yaml",
                OpenApiSpecVersion.OpenApi3_0 => "Basic.OpenApi.yaml",
                _ => throw new NotImplementedException()
            };

            // Assert
            Assert.Equal(Resources.GetString(fileName).ChangeLineBreaks(), yaml);
        }

        [Theory]
        [InlineData(OpenApiSpecVersion.OpenApi2_0)]
        [InlineData(OpenApiSpecVersion.OpenApi3_0)]
        public void MultipleSchemasEdmModelToOpenApiJsonWorks(OpenApiSpecVersion specVersion)
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
            string json = WriteEdmModelAsOpenApi(model, OpenApiFormat.Json, openApiConvertSettings);
            _output.WriteLine(json);

            var parsedJson = JsonNode.Parse(json);
            var fileName = specVersion switch {
                OpenApiSpecVersion.OpenApi2_0 => "Multiple.Schema.OpenApi.V2.json",
                OpenApiSpecVersion.OpenApi3_0 => "Multiple.Schema.OpenApi.json",
                _ => throw new NotImplementedException()
            };

            // Assert
            Assert.True(JsonNode.DeepEquals(JsonNode.Parse(Resources.GetString(fileName)), parsedJson));
        }

        [Theory]
        [InlineData(OpenApiSpecVersion.OpenApi2_0)]
        [InlineData(OpenApiSpecVersion.OpenApi3_0)]
        public void MultipleSchemasEdmModelToOpenApiYamlWorks(OpenApiSpecVersion specVersion)
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
            string yaml = WriteEdmModelAsOpenApi(model, OpenApiFormat.Yaml, openApiConvertSettings);
            _output.WriteLine(yaml);

            var fileName = specVersion switch {
                OpenApiSpecVersion.OpenApi2_0 => "Multiple.Schema.OpenApi.V2.yaml",
                OpenApiSpecVersion.OpenApi3_0 => "Multiple.Schema.OpenApi.yaml",
                _ => throw new NotImplementedException()
            };

            // Assert
            Assert.Equal(Resources.GetString(fileName).ChangeLineBreaks(), yaml);
        }

        [Theory]
        [InlineData(OpenApiSpecVersion.OpenApi2_0)]
        [InlineData(OpenApiSpecVersion.OpenApi3_0)]
        public void TripServiceMetadataToOpenApiJsonWorks(OpenApiSpecVersion specVersion)
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
            string json = WriteEdmModelAsOpenApi(model, OpenApiFormat.Json, settings);
            _output.WriteLine(json);
            var parsedJson = JsonNode.Parse(json);

            var fileName = specVersion switch {
                OpenApiSpecVersion.OpenApi2_0 => "TripService.OpenApi.V2.json",
                OpenApiSpecVersion.OpenApi3_0 => "TripService.OpenApi.json",
                _ => throw new NotImplementedException()
            };

            // Assert
            Assert.True(JsonNode.DeepEquals(JsonNode.Parse(Resources.GetString(fileName)), parsedJson));
        }

        [Theory]
        [InlineData(OpenApiSpecVersion.OpenApi2_0)]
        [InlineData(OpenApiSpecVersion.OpenApi3_0)]
        public void TripServiceMetadataToOpenApiYamlWorks(OpenApiSpecVersion specVersion)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;

            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableKeyAsSegment = true,
                SemVerVersion = "1.2.3",
                ServiceRoot = new Uri("http://services.odata.org/TrippinRESTierService"),
                IEEE754Compatible = true,
                OpenApiSpecVersion = specVersion,
                AddSingleQuotesForStringParameters = true,
                AddEnumDescriptionExtension = true,
                AppendBoundOperationsOnDerivedTypeCastSegments = true,
                IncludeAssemblyInfo = false
            };

            // Act
            string yaml = WriteEdmModelAsOpenApi(model, OpenApiFormat.Yaml, settings);
            _output.WriteLine(yaml);

            var fileName = specVersion switch {
                OpenApiSpecVersion.OpenApi2_0 => "TripService.OpenApi.V2.yaml",
                OpenApiSpecVersion.OpenApi3_0 => "TripService.OpenApi.yaml",
                _ => throw new NotImplementedException()
            };

            // Assert
            Assert.Equal(Resources.GetString(fileName).ChangeLineBreaks(), yaml);
        }

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
