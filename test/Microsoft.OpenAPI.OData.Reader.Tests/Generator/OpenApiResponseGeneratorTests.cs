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
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Moq;
using Microsoft.OpenApi.Models.References;

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
            var response = input.GetResponse(new());

            // Assert
            Assert.Null(response);
        }

        [Theory]
        [InlineData("default")]
        [InlineData("204")]
        public void GetResponseReturnsResponseObject(string input)
        {
            // Arrange & Act
            var response = input.GetResponse(new());

            // Assert
            Assert.NotNull(response);
        }

        [Fact]
        public void CreateResponsesThrowArgumentNullContext()
        {
            // Arrange
            ODataContext context = null;
            var mockModel = new Mock<IEdmModel>().Object;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context", () => context.AddResponsesToDocument(new()));
            Assert.Throws<ArgumentNullException>("document", () => new ODataContext(mockModel).AddResponsesToDocument(null));
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
            OpenApiDocument document = new();

            // Act & Assert
            context.AddResponsesToDocument(document);
            var responses = document.Components.Responses;

            var flightCollectionResponse = responses["Microsoft.OData.Service.Sample.TrippinInMemory.Models.FlightCollectionResponse"];
            var stringCollectionResponse = responses["StringCollectionResponse"];

            var flightCollectionResponseSchemaReference = Assert.IsType<OpenApiSchemaReference>(flightCollectionResponse.Content["application/json"].Schema);
            Assert.Equal("Microsoft.OData.Service.Sample.TrippinInMemory.Models.FlightCollectionResponse", flightCollectionResponseSchemaReference.Reference.Id);
            var stringCollectionResponseSchemaReference = Assert.IsType<OpenApiSchemaReference>(stringCollectionResponse.Content["application/json"].Schema);
            Assert.Equal("StringCollectionResponse", stringCollectionResponseSchemaReference.Reference.Id);
        }

        [Fact]
        public void CreateResponsesReturnsCreatedResponses()
        {
            // Arrange
            IEdmModel model = EdmCoreModel.Instance;
            ODataContext context = new ODataContext(model);
            OpenApiDocument document = new();

            // Act
            context.AddResponsesToDocument(document);
            var responses = document.Components.Responses;

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
        public async Task CanSerializeAsJsonFromTheCreatedResponses()
        {
            // Arrange
            IEdmModel model = EdmCoreModel.Instance;
            ODataContext context = new ODataContext(model);
            OpenApiDocument document = new();

            // Act
            context.AddResponsesToDocument(document);
            var responses = document.Components.Responses;

            // Assert
            var response = responses["error"];
            Assert.NotNull(response);
            string json = await response.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0);

            var expectedJson = @"{
  ""description"": ""error"",
  ""content"": {
    ""application/json"": {
      ""schema"": {
        ""$ref"": ""#/components/schemas/ODataErrors.ODataError""
      }
    }
  }
}";
            var expectedJsonNode = JsonNode.Parse(expectedJson);
            var actualJsonNode = JsonNode.Parse(json);
            Assert.True(JsonNode.DeepEquals(expectedJsonNode, actualJsonNode));
        }

        [Fact]
        public void CreateResponseForoperationImportThrowArgumentNullContext()
        {
            // Arrange
            ODataContext context = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context", () => context.CreateResponses(operationImport: null, new()));
        }

        [Fact]
        public void CreateResponseForoperationImportThrowArgumentNullOperationImport()
        {
            // Arrange
            ODataContext context = new ODataContext(EdmCoreModel.Instance);

            // Act & Assert
            Assert.Throws<ArgumentNullException>("operationImport", () => context.CreateResponses(operationImport: null, new()));
        }

        [Fact]
        public void CreateResponseForOperationThrowArgumentNullContext()
        {
            // Arrange
            ODataContext context = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context", () => context.CreateResponses(operation: null, new()));
        }

        [Fact]
        public void CreateResponseForOperationThrowArgumentNullOperation()
        {
            // Arrange
            ODataContext context = new ODataContext(EdmCoreModel.Instance);

            // Act & Assert
            Assert.Throws<ArgumentNullException>("operation", () => context.CreateResponses(operation: null, new()));
        }

        [Theory]
        [InlineData(true, OpenApiSpecVersion.OpenApi3_1)]
        [InlineData(false, OpenApiSpecVersion.OpenApi3_1)]
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
                responses = context.CreateResponses(operationImport, new());
            }
            else
            {
                IEdmOperation operation = model.SchemaElements.OfType<IEdmOperation>().First(o => o.Name == operationName);
                Assert.NotNull(operation); // guard
                ODataPath path = new ODataPath(new ODataOperationSegment(operation));
                responses = context.CreateResponses(operation, new());
            }

            // Assert
            Assert.NotNull(responses);
            Assert.NotEmpty(responses);
            Assert.Equal(2, responses.Count);
            Assert.Equal(new string[] { "200", "default" }, responses.Select(r => r.Key));

            var response = responses["200"];
            Assert.NotNull(response.Content);
            OpenApiMediaType mediaType = response.Content["application/json"];
// openApi version 2 should not use AnyOf
            if (specVersion == OpenApiSpecVersion.OpenApi2_0)
            {
                Assert.NotNull(mediaType.Schema);
                Assert.Null(mediaType.Schema.AnyOf);
                var mediaTypeSchemaReference = Assert.IsType<OpenApiSchemaReference>(mediaType.Schema);
                Assert.NotNull(mediaTypeSchemaReference.Reference);
                Assert.Equal("Microsoft.OData.Service.Sample.TrippinInMemory.Models.Person", mediaTypeSchemaReference.Reference.Id);
            }
            else
            {
                Assert.NotNull(mediaType.Schema);
                Assert.IsNotType<OpenApiSchemaReference>(mediaType.Schema);
                Assert.NotNull(mediaType.Schema.AnyOf);
                Assert.Equal(2, mediaType.Schema.AnyOf.Count);
                var anyOfRef = Assert.IsType<OpenApiSchemaReference>(mediaType.Schema.AnyOf.FirstOrDefault());
                Assert.NotNull(anyOfRef);
                Assert.Equal("Microsoft.OData.Service.Sample.TrippinInMemory.Models.Person", anyOfRef.Reference.Id);
                var anyOfNull = mediaType.Schema.AnyOf.Skip(1).FirstOrDefault();
                Assert.NotNull(anyOfNull.Type);
                Assert.Equal(JsonSchemaType.Null, anyOfNull.Type);
            }
        }

        [Fact]
        public void CreateResponseForEdmFunctionOfStreamReturnTypeReturnsCorrectResponse()
        {
            // Arrange
            string operationName1 = "getMailboxUsageStorage";
            string operationName2 = "incidentReport";
            IEdmModel model = EdmModelHelper.GraphBetaModel;
            ODataContext context = new(model);

            // Act
            IEdmOperation operation1 = model.SchemaElements.OfType<IEdmOperation>().First(o => o.Name == operationName1);
            IEdmOperation operation2 = model.SchemaElements.OfType<IEdmOperation>().First(o => o.Name == operationName2);
            Assert.NotNull(operation1);
            Assert.NotNull(operation2);
            ODataPath path1 = new(new ODataOperationSegment(operation1));
            ODataPath path2 = new(new ODataOperationSegment(operation2));
            OpenApiResponses responses1 = context.CreateResponses(operation1, new());
            OpenApiResponses responses2 = context.CreateResponses(operation2, new());

            // Assert for operation1 --> getMailboxUsageStorage
            Assert.NotNull(responses1);
            Assert.NotEmpty(responses1);
            Assert.Equal(2, responses1.Count);
            Assert.Equal(new string[] { "200", "default" }, responses1.Select(r => r.Key));

            var response = responses1["200"];
            Assert.NotNull(response.Content);
            Assert.Equal("application/octet-stream", response.Content.First().Key);

            // Assert for operation2 --> incidentReport
            Assert.NotNull(responses2);
            Assert.NotEmpty(responses2);
            Assert.Equal(2, responses2.Count);
            Assert.Equal(new string[] { "200", "default" }, responses2.Select(r => r.Key));

            response = responses2["200"];
            Assert.NotNull(response.Content);
            Assert.Equal("text/html", response.Content.First().Key);
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
                responses = context.CreateResponses(operationImport, new());
            }
            else
            {
                IEdmOperation operation = model.SchemaElements.OfType<IEdmOperation>().First(o => o.Name == actionName);
                Assert.NotNull(operation); // guard
                ODataPath path = new ODataPath(new ODataOperationSegment(operation));
                responses = context.CreateResponses(operation, new());
            }

            // Assert
            Assert.NotNull(responses);
            Assert.NotEmpty(responses);
            Assert.Equal(2, responses.Count);
            Assert.Equal(new string[] { responseCode, "default" }, responses.Select(r => r.Key));
        }

        [Theory]
        [InlineData("assignLicense", false, "200")]
        [InlineData("activateService", false, "204")]
        [InlineData("verifySignature", true, "200")]
        public void CreateResponseForEdmActionWhenErrorResponsesAsDefaultIsSet(string actionName, bool errorAsDefault, string responseCode)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.GraphBetaModel;
            var settings = new OpenApiConvertSettings
            {
                ErrorResponsesAsDefault = errorAsDefault,
            };
            ODataContext context = new ODataContext(model, settings);

            // Act
            OpenApiResponses responses;
            IEdmOperation operation = model.SchemaElements.OfType<IEdmOperation>().First(o => o.Name == actionName);
            Assert.NotNull(operation); // guard
            ODataPath path = new(new ODataOperationSegment(operation));
            responses = context.CreateResponses(operation, new());

            // Assert
            Assert.NotNull(responses);
            Assert.NotEmpty(responses);
            if (errorAsDefault)
            {
                Assert.Equal(new string[] { responseCode, "default" }, responses.Select(r => r.Key));
            }
            else
            {
                Assert.Equal(new string[] { responseCode, "4XX", "5XX" }, responses.Select(r => r.Key));
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CreateResponseForDeltaEdmFunctionReturnCorrectResponses(bool enableOdataAnnotationRef)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.GraphBetaModel;
            OpenApiConvertSettings settings = new()
            {
                EnableODataAnnotationReferencesForResponses = enableOdataAnnotationRef
            };
            ODataContext context = new(model, settings);

            // Act
            IEdmFunction operation = model.SchemaElements.OfType<IEdmFunction>().First(o => o.Name == "delta" &&
                   o.Parameters.First().Type.FullName() == "Collection(microsoft.graph.application)");
            Assert.NotNull(operation); // guard
            OpenApiResponses responses = context.CreateResponses(operation, new());
            string json = await responses.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0);

            // Assert
            Assert.NotNull(responses);
            Assert.NotEmpty(responses);
            if (enableOdataAnnotationRef)
            {
                var expectedJson = @"{
  ""200"": {
    ""description"": ""Success"",
    ""content"": {
      ""application/json"": {
        ""schema"": {
          ""title"": ""Collection of application"",
          ""type"": ""object"",
          ""allOf"": [
            {
              ""$ref"": ""#/components/schemas/BaseDeltaFunctionResponse""
            },
            {
              ""type"": ""object"",
              ""properties"": {
                ""value"": {
                  ""type"": ""array"",
                  ""items"": {
                    ""$ref"": ""#/components/schemas/microsoft.graph.application""
                  }
                }
              }
            }
          ]
        }
      }
    }
  },
  ""default"": {
    ""$ref"": ""#/components/responses/error""
  }
}";
                var expectedJsonNode = JsonNode.Parse(expectedJson);
                var actualJsonNode = JsonNode.Parse(json);
                Assert.True(JsonNode.DeepEquals(expectedJsonNode, actualJsonNode));
            }
            else
            {
                var expectedJson = @"{
  ""200"": {
    ""description"": ""Success"",
    ""content"": {
      ""application/json"": {
        ""schema"": {
          ""title"": ""Collection of application"",
          ""type"": ""object"",
          ""properties"": {
            ""value"": {
              ""type"": ""array"",
              ""items"": {
                ""$ref"": ""#/components/schemas/microsoft.graph.application""
              }
            },
            ""@odata.nextLink"": {
              ""type"": ""string"",
              ""nullable"": true
            },
            ""@odata.deltaLink"": {
              ""type"": ""string"",
              ""nullable"": true
            }
          }
        }
      }
    }
  },
  ""default"": {
    ""$ref"": ""#/components/responses/error""
  }
}";
                var expectedJsonNode = JsonNode.Parse(expectedJson);
                var actualJsonNode = JsonNode.Parse(json);
                Assert.True(JsonNode.DeepEquals(expectedJsonNode, actualJsonNode));
            }
        }
    }
}
