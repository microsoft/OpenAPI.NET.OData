// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.OData.Tests;
using Microsoft.OpenApi.Models;
using Xunit;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.Generator.Tests
{
    public class OpenApiResponseGeneratorTest
    {
        [Theory]
        [InlineData("default1")]
        [InlineData("Default")]
        [InlineData("200")]
        public void GetResponseReturnsNullResponseObject(string input)
        {
            // Arrange & Act
            var response = input.GetResponse();

            // Assert
            Assert.Null(response);
        }

        [Theory]
        [InlineData("default")]
        [InlineData("204")]
        public void GetResponseReturnsResponseObject(string input)
        {
            // Arrange & Act
            var response = input.GetResponse();

            // Assert
            Assert.NotNull(response);
        }

        [Fact]
        public void CreateResponsesThrowArgumentNullContext()
        {
            // Arrange
            ODataContext context = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context", () => context.CreateResponses());
        }

        [Fact]
        public void CreatesCollectionResponses()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            OpenApiConvertSettings settings = new()
            {
                    EnableOperationId = true,
                    EnablePagination = true,
            };
            ODataContext context = new(model, settings);

            // Act & Assert
            var responses = context.CreateResponses();

            var flightCollectionResponse = responses["Microsoft.OData.Service.Sample.TrippinInMemory.Models.FlightCollectionResponse"];
            var stringCollectionResponse = responses["StringCollectionResponse"];

            Assert.Equal("Microsoft.OData.Service.Sample.TrippinInMemory.Models.FlightCollectionResponse", flightCollectionResponse.Content["application/json"].Schema.Reference.Id);
            Assert.Equal("StringCollectionResponse", stringCollectionResponse.Content["application/json"].Schema.Reference.Id);
        }

        [Fact]
        public void CreateResponsesReturnsCreatedResponses()
        {
            // Arrange
            IEdmModel model = EdmCoreModel.Instance;
            ODataContext context = new ODataContext(model);

            // Act
            var responses = context.CreateResponses();

            // Assert
            Assert.NotNull(responses);
            Assert.NotEmpty(responses);
            var response = responses["error"];
            Assert.NotNull(response);
            Assert.NotNull(response.Content);
            Assert.Single(response.Content);
            Assert.Equal("application/json", response.Content.First().Key);
        }

        [Fact]
        public void CanSerializeAsJsonFromTheCreatedResponses()
        {
            // Arrange
            IEdmModel model = EdmCoreModel.Instance;
            ODataContext context = new ODataContext(model);

            // Act
            var responses = context.CreateResponses();

            // Assert
            var response = responses["error"];
            Assert.NotNull(response);
            string json = response.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);

            Assert.Equal(@"{
  ""description"": ""error"",
  ""content"": {
    ""application/json"": {
      ""schema"": {
        ""$ref"": ""#/components/schemas/odataerrors.odataerror""
      }
    }
  }
}".ChangeLineBreaks(), json);
        }

        [Fact]
        public void CreateResponseForoperationImportThrowArgumentNullContext()
        {
            // Arrange
            ODataContext context = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context", () => context.CreateResponses(operationImport: null, path: null));
        }

        [Fact]
        public void CreateResponseForoperationImportThrowArgumentNullOperationImport()
        {
            // Arrange
            ODataContext context = new ODataContext(EdmCoreModel.Instance);

            // Act & Assert
            Assert.Throws<ArgumentNullException>("operationImport", () => context.CreateResponses(operationImport: null, path: null));
        }

        [Fact]
        public void CreateResponseForoperationImportThrowArgumentNullPath()
        {
            // Arrange
            ODataContext context = new ODataContext(EdmCoreModel.Instance);
            EdmFunction function = new EdmFunction("NS", "MyFunction", EdmCoreModel.Instance.GetString(false));
            EdmFunctionImport functionImport = new EdmFunctionImport(new EdmEntityContainer("NS", "Default"), "MyFunctionImport", function);

            // Act & Assert
            Assert.Throws<ArgumentNullException>("path", () => context.CreateResponses(operationImport: functionImport, path: null));
        }

        [Fact]
        public void CreateResponseForOperationThrowArgumentNullContext()
        {
            // Arrange
            ODataContext context = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context", () => context.CreateResponses(operation: null, path: null));
        }

        [Fact]
        public void CreateResponseForOperationThrowArgumentNullOperation()
        {
            // Arrange
            ODataContext context = new ODataContext(EdmCoreModel.Instance);

            // Act & Assert
            Assert.Throws<ArgumentNullException>("operation", () => context.CreateResponses(operation: null, path: null));
        }

        [Fact]
        public void CreateResponseForOperationThrowArgumentNullPath()
        {
            // Arrange
            ODataContext context = new ODataContext(EdmCoreModel.Instance);
            EdmFunction function = new EdmFunction("NS", "MyFunction", EdmCoreModel.Instance.GetString(false));
            EdmFunctionImport functionImport = new EdmFunctionImport(new EdmEntityContainer("NS", "Default"), "MyFunctionImport", function);

            // Act & Assert
            Assert.Throws<ArgumentNullException>("path", () => context.CreateResponses(operation: function, path: null));
        }

        [Theory]
        [InlineData(true, OpenApiSpecVersion.OpenApi3_0)]
        [InlineData(false, OpenApiSpecVersion.OpenApi3_0)]
        [InlineData(true, OpenApiSpecVersion.OpenApi2_0)]
        [InlineData(false, OpenApiSpecVersion.OpenApi2_0)]
        public void CreateResponseForEdmFunctionReturnCorrectResponses(bool isFunctionImport, OpenApiSpecVersion specVersion)
        {
            // Arrange
            string operationName = "GetPersonWithMostFriends";
            IEdmModel model = EdmModelHelper.TripServiceModel;
            ODataContext context = new ODataContext(model);

            context.Settings.OpenApiSpecVersion = specVersion;

            // Act
            OpenApiResponses responses;
            if (isFunctionImport)
            {
                IEdmOperationImport operationImport = model.EntityContainer.OperationImports().First(o => o.Name == operationName);
                Assert.NotNull(operationImport); // guard
                ODataPath path = new ODataPath(new ODataOperationImportSegment(operationImport));
                responses = context.CreateResponses(operationImport, path);
            }
            else
            {
                IEdmOperation operation = model.SchemaElements.OfType<IEdmOperation>().First(o => o.Name == operationName);
                Assert.NotNull(operation); // guard
                ODataPath path = new ODataPath(new ODataOperationSegment(operation));
                responses = context.CreateResponses(operation, path);
            }

            // Assert
            Assert.NotNull(responses);
            Assert.NotEmpty(responses);
            Assert.Equal(2, responses.Count);
            Assert.Equal(new string[] { "200", "default" }, responses.Select(r => r.Key));

            OpenApiResponse response = responses["200"];
            Assert.NotNull(response.Content);
            OpenApiMediaType mediaType = response.Content["application/json"];

            // For either version, nullable should be set
            // and the serializer will ignore for v2
            Assert.True(mediaType.Schema.Nullable);

            // openApi version 2 should have not use nullable
            if (specVersion == OpenApiSpecVersion.OpenApi2_0)
            {
                Assert.NotNull(mediaType.Schema);
                Assert.Null(mediaType.Schema.AnyOf);
                Assert.NotNull(mediaType.Schema.Reference);
                Assert.Equal("Microsoft.OData.Service.Sample.TrippinInMemory.Models.Person", mediaType.Schema.Reference.Id);
            }
            else
            {
                Assert.NotNull(mediaType.Schema);
                Assert.Null(mediaType.Schema.Reference);
                Assert.NotNull(mediaType.Schema.AnyOf);
                var anyOf = Assert.Single(mediaType.Schema.AnyOf);
                Assert.Equal("Microsoft.OData.Service.Sample.TrippinInMemory.Models.Person", anyOf.Reference.Id);
            }
        }

        [Theory]
        [InlineData("ShareTrip", false, "204")]
        [InlineData("ResetDataSource", true, "204")]
        [InlineData("GetPeersForTrip", false, "200")]
        public void CreateResponseForEdmActionReturnCorrectResponses(string actionName, bool isActionImport, string responseCode)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            ODataContext context = new ODataContext(model);

            // Act
            OpenApiResponses responses;
            if (isActionImport)
            {
                IEdmOperationImport operationImport = model.EntityContainer.OperationImports().First(o => o.Name == actionName);
                Assert.NotNull(operationImport); // guard
                ODataPath path = new ODataPath(new ODataOperationImportSegment(operationImport));
                responses = context.CreateResponses(operationImport, path);
            }
            else
            {
                IEdmOperation operation = model.SchemaElements.OfType<IEdmOperation>().First(o => o.Name == actionName);
                Assert.NotNull(operation); // guard
                ODataPath path = new ODataPath(new ODataOperationSegment(operation));
                responses = context.CreateResponses(operation, path);
            }

            // Assert
            Assert.NotNull(responses);
            Assert.NotEmpty(responses);
            Assert.Equal(2, responses.Count);
            Assert.Equal(new string[] { responseCode, "default" }, responses.Select(r => r.Key));
        }
    }
}
