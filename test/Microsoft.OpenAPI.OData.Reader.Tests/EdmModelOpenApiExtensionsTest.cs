//---------------------------------------------------------------------
// <copyright file="EdmModelOpenApiExtensionsTest.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using System.IO;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
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
        public void EmptyEdmModelToOpenApiJsonWorks()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.EmptyModel;

            // Act
            string json = WriteEdmModelAsOpenApi(model, OpenApiFormat.Json);
            _output.WriteLine(json);

            // Assert
            Assert.Equal(Resources.GetString("Empty.OpenApi.json").Replace(), json);
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
            Assert.Equal(Resources.GetString("Empty.OpenApi.yaml").Replace(), yaml);
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
            Assert.Equal(Resources.GetString("Basic.OpenApi.json").Replace(), json);
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
            Assert.Equal(Resources.GetString("Basic.OpenApi.yaml").Replace(), yaml);
        }

        [Fact]
        public void TripServiceMetadataToOpenApiJsonWorks()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            Action<OpenApiDocument> action = (d) =>
            {
                d.Info.Version = "1.0.1";
                d.Servers[0].Url = "http://services.odata.org/TrippinRESTierService/";
            };

            // Act
            string json = WriteEdmModelAsOpenApi(model, OpenApiFormat.Json, action);
            _output.WriteLine(json);

            // Assert
            Assert.Equal(Resources.GetString("TripService.OpenApi.json").Replace(), json);
        }

        [Fact]
        public void TripServiceMetadataToOpenApiYamlWorks()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            Action<OpenApiDocument> action = (d) =>
            {
                d.Info.Version = "1.0.1";
                d.Servers[0].Url = "http://services.odata.org/TrippinRESTierService/";
            };

            // Act
            string yaml = WriteEdmModelAsOpenApi(model, OpenApiFormat.Yaml, action);
            _output.WriteLine(yaml);

            // Assert
            Assert.Equal(Resources.GetString("TripService.OpenApi.yaml").Replace(), yaml);
        }

        private static string WriteEdmModelAsOpenApi(IEdmModel model, OpenApiFormat target,
            Action<OpenApiDocument> action = null)
        {
            var document = model.Convert(action);
            MemoryStream stream = new MemoryStream();
            document.Serialize(stream, OpenApiSpecVersion.OpenApi3_0, target);
            stream.Flush();
            stream.Position = 0;
            return new StreamReader(stream).ReadToEnd();
        }
    }
}
