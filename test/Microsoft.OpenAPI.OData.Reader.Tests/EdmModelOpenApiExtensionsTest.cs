//---------------------------------------------------------------------
// <copyright file="EdmModelOpenApiExtensionsTest.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using System.IO;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.OData.OpenAPI.Tests
{
    public class EdmModelOpenApiExtensionsTest
    {
        [Fact]
        public void EmptyEdmModelToOpenApiJsonWorks()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.EmptyModel;

            // Act
            string json = WriteEdmModelToOpenApi(model, OpenApiTarget.Json);

            // Assert
            Assert.Equal(Resources.GetString("Empty.OpenApi.json").Replace(), json);
        }

        [Fact]
        public void EmptyEdmModelToOpenApiYamlWorks()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.EmptyModel;

            // Act
            string yaml = WriteEdmModelToOpenApi(model, OpenApiTarget.Yaml);

            // Assert
            Assert.Equal(Resources.GetString("Empty.OpenApi.yaml").Replace(), yaml);
        }

        [Fact]
        public void BasicEdmModelToOpenApiJsonWorks()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.BasicEdmModel;

            // Act
            string json = WriteEdmModelToOpenApi(model, OpenApiTarget.Json);

            // Assert
            Assert.Equal(Resources.GetString("Basic.OpenApi.json").Replace(), json);
        }

        [Fact]
        public void BasicEdmModelToOpenApiYamlWorks()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.BasicEdmModel;

            // Act
            string yaml = WriteEdmModelToOpenApi(model, OpenApiTarget.Yaml);

            // Assert
            Assert.Equal(Resources.GetString("Basic.OpenApi.yaml").Replace(), yaml);
        }

        [Fact]
        public void TripServiceMetadataToOpenApiJsonWorks()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            OpenApiWriterSettings settings = new OpenApiWriterSettings
            {
                Version = new Version(1, 0, 1),
                BaseUri = new Uri("http://services.odata.org/TrippinRESTierService/")
            };

            // Act
            string json = WriteEdmModelToOpenApi(model, OpenApiTarget.Json, settings);

            // Assert
            Assert.Equal(Resources.GetString("TripService.OpenApi.json").Replace(), json);
        }

        [Fact]
        public void TripServiceMetadataToOpenApiYamlWorks()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            OpenApiWriterSettings settings = new OpenApiWriterSettings
            {
                Version = new Version(1, 0, 1),
                BaseUri = new Uri("http://services.odata.org/TrippinRESTierService/")
            };

            // Act
            string yaml = WriteEdmModelToOpenApi(model, OpenApiTarget.Yaml, settings);

            // Assert
            Assert.Equal(Resources.GetString("TripService.OpenApi.yaml").Replace(), yaml);
        }

        private static string WriteEdmModelToOpenApi(IEdmModel model, OpenApiTarget target,
            OpenApiWriterSettings settings = null)
        {
            MemoryStream stream = new MemoryStream();
            model.WriteOpenApi(stream, target, settings);
            stream.Flush();
            stream.Position = 0;
            return new StreamReader(stream).ReadToEnd();
        }
    }
}
