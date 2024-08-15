// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.IO;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.OpenApi.OData.Tests
{
    public class EdmModelOpenApiExtensionsTest
    {
        private ITestOutputHelper _output;

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
            var openApiConvertSettings = new OpenApiConvertSettings();
            openApiConvertSettings.OpenApiSpecVersion = specVersion;
            openApiConvertSettings.IncludeAssemblyInfo = false;

            // Act
            string json = WriteEdmModelAsOpenApi(model, OpenApiFormat.Json, openApiConvertSettings);
            _output.WriteLine(json);

            // Assert
            if (specVersion == OpenApiSpecVersion.OpenApi2_0)
            {
                Assert.Equal(Resources.GetString("Empty.OpenApi.V2.json").ChangeLineBreaks(), json);
            }
            else
            {
                Assert.Equal(Resources.GetString("Empty.OpenApi.json").ChangeLineBreaks(), json);
            }
        }

        [Theory]
        [InlineData(OpenApiSpecVersion.OpenApi2_0)]
        [InlineData(OpenApiSpecVersion.OpenApi3_0)]
        public void EmptyEdmModelToOpenApiYamlWorks(OpenApiSpecVersion specVersion)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.EmptyModel;
            var openApiConvertSettings = new OpenApiConvertSettings();
            openApiConvertSettings.OpenApiSpecVersion = specVersion; 
            openApiConvertSettings.IncludeAssemblyInfo = false;

            // Act
            string yaml = WriteEdmModelAsOpenApi(model, OpenApiFormat.Yaml, openApiConvertSettings);
            _output.WriteLine(yaml);

            // Assert
            if (specVersion == OpenApiSpecVersion.OpenApi2_0)
            {
                Assert.Equal(Resources.GetString("Empty.OpenApi.V2.yaml").ChangeLineBreaks(), yaml);
            }
            else
            {
                Assert.Equal(Resources.GetString("Empty.OpenApi.yaml").ChangeLineBreaks(), yaml);
            }
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

            // Assert
            if (specVersion == OpenApiSpecVersion.OpenApi2_0)
            {
                Assert.Equal(Resources.GetString("Basic.OpenApi.V2.json").ChangeLineBreaks(), json);
            }
            else
            {
                Assert.Equal(Resources.GetString("Basic.OpenApi.json").ChangeLineBreaks(), json);
            }
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

            // Assert
            if (specVersion == OpenApiSpecVersion.OpenApi2_0)
            {
                Assert.Equal(Resources.GetString("Basic.OpenApi.V2.yaml").ChangeLineBreaks(), yaml);
            }
            else
            {
                Assert.Equal(Resources.GetString("Basic.OpenApi.yaml").ChangeLineBreaks(), yaml);
            }
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

            // Assert
            if (specVersion == OpenApiSpecVersion.OpenApi2_0)
            {
                Assert.Equal(Resources.GetString("Multiple.Schema.OpenApi.V2.json").ChangeLineBreaks(), json);
            }
            else
            {
                Assert.Equal(Resources.GetString("Multiple.Schema.OpenApi.json").ChangeLineBreaks(), json);
            }
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

            // Assert
            if (specVersion == OpenApiSpecVersion.OpenApi2_0)
            {
                Assert.Equal(Resources.GetString("Multiple.Schema.OpenApi.V2.yaml").ChangeLineBreaks(), yaml);
            }
            else
            {
                Assert.Equal(Resources.GetString("Multiple.Schema.OpenApi.yaml").ChangeLineBreaks(), yaml);
            }
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

            // Assert
            if (specVersion == OpenApiSpecVersion.OpenApi2_0)
            {
                Assert.Equal(Resources.GetString("TripService.OpenApi.V2.json").ChangeLineBreaks(), json);
            }
            else
            {
                Assert.Equal(Resources.GetString("TripService.OpenApi.json").ChangeLineBreaks(), json);
            }
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

            // Assert
            if (specVersion == OpenApiSpecVersion.OpenApi2_0)
            {
                Assert.Equal(Resources.GetString("TripService.OpenApi.V2.yaml").ChangeLineBreaks(), yaml);
            }
            else
            {
                Assert.Equal(Resources.GetString("TripService.OpenApi.yaml").ChangeLineBreaks(), yaml);
            }
        }

        private static string WriteEdmModelAsOpenApi(IEdmModel model, OpenApiFormat target,
            OpenApiConvertSettings settings = null)
        {
            settings = settings ?? new OpenApiConvertSettings();
            var document = model.ConvertToOpenApi(settings);
            Assert.NotNull(document); // guard

            MemoryStream stream = new MemoryStream();
            document.Serialize(stream, settings.OpenApiSpecVersion, target);
            stream.Flush();
            stream.Position = 0;
            return new StreamReader(stream).ReadToEnd();
        }
    }
}
