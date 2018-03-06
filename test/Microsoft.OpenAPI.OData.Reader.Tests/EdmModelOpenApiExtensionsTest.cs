﻿// ------------------------------------------------------------
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

        [Fact]
        public void EmptyEdmModelToOpenApiJsonWorks()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.EmptyModel;

            // Act
            string json = WriteEdmModelAsOpenApi(model, OpenApiFormat.Json);
            _output.WriteLine(json);

            // Assert
            Assert.Equal(Resources.GetString("Empty.OpenApi.json").ChangeLineBreaks(), json);
        }

        [Fact]
        public void EmptyEdmModelToOpenApiYamlWorks()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.EmptyModel;

            // Act
            string yaml = WriteEdmModelAsOpenApi(model, OpenApiFormat.Yaml);
            _output.WriteLine(yaml);

            // Assert
            Assert.Equal(Resources.GetString("Empty.OpenApi.yaml").ChangeLineBreaks(), yaml);
        }

        [Fact]
        public void BasicEdmModelToOpenApiJsonWorks()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.BasicEdmModel;

            // Act
            string json = WriteEdmModelAsOpenApi(model, OpenApiFormat.Json);
            _output.WriteLine(json);

            // Assert
            Assert.Equal(Resources.GetString("Basic.OpenApi.json").ChangeLineBreaks(), json);
        }

        [Fact]
        public void BasicEdmModelToOpenApiYamlWorks()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.BasicEdmModel;

            // Act
            string yaml = WriteEdmModelAsOpenApi(model, OpenApiFormat.Yaml);
            _output.WriteLine(yaml);

            // Assert
            Assert.Equal(Resources.GetString("Basic.OpenApi.yaml").ChangeLineBreaks(), yaml);
        }

        [Fact]
        public void TripServiceMetadataToOpenApiJsonWorks()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                Version = new Version(1, 0, 1),
                ServiceRoot = new Uri("http://services.odata.org/TrippinRESTierService"),
                IEEE754Compatible = true
            };
            // Act
            string json = WriteEdmModelAsOpenApi(model, OpenApiFormat.Json, settings);
            _output.WriteLine(json);

            // Assert
            Assert.Equal(Resources.GetString("TripService.OpenApi.json").ChangeLineBreaks(), json);
        }

        [Fact]
        public void TripServiceMetadataToOpenApiYamlWorks()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;

            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                Version = new Version(1, 0, 1),
                ServiceRoot = new Uri("http://services.odata.org/TrippinRESTierService"),
                IEEE754Compatible = true
            };

            // Act
            string yaml = WriteEdmModelAsOpenApi(model, OpenApiFormat.Yaml, settings);
            _output.WriteLine(yaml);

            // Assert
            Assert.Equal(Resources.GetString("TripService.OpenApi.yaml").ChangeLineBreaks(), yaml);
        }

        [Fact]
        public void GraphBetaMetadataToOpenApiJsonWorks()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.GraphBetaModel;

            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                Version = new Version(1, 0, 1),
                ServiceRoot = new Uri("http://graph.microsoft.com/beta"),
                IEEE754Compatible = true
            };

            // Act
            string json = WriteEdmModelAsOpenApi(model, OpenApiFormat.Json, settings);
            _output.WriteLine(json);

            // Assert
            Assert.Equal(Resources.GetString("Graph.Beta.OpenApi.json").ChangeLineBreaks(), json);
        }

        [Fact]
        public void GraphBetaMetadataToOpenApiYamlWorks()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.GraphBetaModel;

            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                Version = new Version(1, 0, 1),
                ServiceRoot = new Uri("http://graph.microsoft.com/beta"),
                IEEE754Compatible = true
            };

            // Act
            string yaml = WriteEdmModelAsOpenApi(model, OpenApiFormat.Yaml, settings);
            _output.WriteLine(yaml);

            // Assert
            Assert.Equal(Resources.GetString("Graph.Beta.OpenApi.yaml").ChangeLineBreaks(), yaml);
        }

        private static string WriteEdmModelAsOpenApi(IEdmModel model, OpenApiFormat target,
            OpenApiConvertSettings settings = null)
        {
            var document = model.ConvertToOpenApi(settings);
            Assert.NotNull(document); // guard

            MemoryStream stream = new MemoryStream();
            document.Serialize(stream, OpenApiSpecVersion.OpenApi3_0, target);
            stream.Flush();
            stream.Position = 0;
            return new StreamReader(stream).ReadToEnd();
        }
    }
}
