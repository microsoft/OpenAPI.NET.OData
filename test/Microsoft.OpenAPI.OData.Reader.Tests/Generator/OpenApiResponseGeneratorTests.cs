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
            var response = Assert.Single(responses);
            Assert.Equal("error", response.Key);
            Assert.NotNull(response.Value.Content);
            Assert.Single(response.Value.Content);
            Assert.Equal("application/json", response.Value.Content.First().Key);
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
            var response = Assert.Single(responses).Value;
            string json = response.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0_0);

            Assert.Equal(@"{
  ""description"": ""error"",
  ""content"": {
    ""application/json"": {
      ""schema"": {
        ""$ref"": ""#/components/schemas/odata.error""
      }
    }
  }
}".Replace(), json);
        }

        [Fact]
        public void CreateResponseForoperationImportThrowArgumentNullContext()
        {
            // Arrange
            ODataContext context = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context", () => context.CreateResponses(operationImport: null));
        }

        [Fact]
        public void CreateResponseForoperationImportThrowArgumentNullOperationImport()
        {
            // Arrange
            ODataContext context = new ODataContext(EdmCoreModel.Instance);

            // Act & Assert
            Assert.Throws<ArgumentNullException>("operationImport", () => context.CreateResponses(operationImport: null));
        }

        [Fact]
        public void CreateResponseForOperationThrowArgumentNullContext()
        {
            // Arrange
            ODataContext context = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context", () => context.CreateResponses(operation: null));
        }

        [Fact]
        public void CreateResponseForOperationThrowArgumentNullOperation()
        {
            // Arrange
            ODataContext context = new ODataContext(EdmCoreModel.Instance);

            // Act & Assert
            Assert.Throws<ArgumentNullException>("operation", () => context.CreateResponses(operation: null));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateResponseForEdmFunctionReturnCorrectResponses(bool isFunctionImport)
        {
            // Arrange
            string operationName = "GetPersonWithMostFriends";
            IEdmModel model = EdmModelHelper.TripServiceModel;
            ODataContext context = new ODataContext(model);

            // Act
            OpenApiResponses responses;
            if (isFunctionImport)
            {
                IEdmOperationImport operationImport = model.EntityContainer.OperationImports().First(o => o.Name == operationName);
                Assert.NotNull(operationImport); // guard
                responses = context.CreateResponses(operationImport);
            }
            else
            {
                IEdmOperation operation = model.SchemaElements.OfType<IEdmOperation>().First(o => o.Name == operationName);
                Assert.NotNull(operation); // guard
                responses = context.CreateResponses(operation);
            }

            // Assert
            Assert.NotNull(responses);
            Assert.NotEmpty(responses);
            Assert.Equal(2, responses.Count);
            Assert.Equal(new string[] { "200", "default" }, responses.Select(r => r.Key));

            OpenApiResponse response = responses["200"];
            Assert.NotNull(response.Content);
            OpenApiMediaType mediaType = response.Content["application/json"];

            Assert.NotNull(mediaType.Schema);
            Assert.True(mediaType.Schema.Nullable);
            Assert.Null(mediaType.Schema.Reference);
            Assert.NotNull(mediaType.Schema.AnyOf);
            var anyOf = Assert.Single(mediaType.Schema.AnyOf);
            Assert.Equal("Microsoft.OData.Service.Sample.TrippinInMemory.Models.Person", anyOf.Reference.Id);
        }

        [Theory]
        [InlineData("ShareTrip", false)]
        [InlineData("ResetDataSource", true)]
        public void CreateResponseForEdmActionReturnCorrectResponses(string actionName, bool isActionImport)
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
                responses = context.CreateResponses(operationImport);
            }
            else
            {
                IEdmOperation operation = model.SchemaElements.OfType<IEdmOperation>().First(o => o.Name == actionName);
                Assert.NotNull(operation); // guard
                responses = context.CreateResponses(operation);
            }

            // Assert
            Assert.NotNull(responses);
            Assert.NotEmpty(responses);
            Assert.Equal(2, responses.Count);
            Assert.Equal(new string[] { "204", "default" }, responses.Select(r => r.Key));
        }
    }
}
