// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Tests;
using Xunit;

namespace Microsoft.OpenApi.OData.Generator.Tests
{
    public class OpenApiRequestBodyGeneratorTest
    {
        private IEdmModel _model;
        private IEdmAction _action;
        private IEdmEntityType _entityType;

        private IEdmActionImport _actionImport;

        public OpenApiRequestBodyGeneratorTest()
        {
            EdmModel model = new EdmModel();
            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
            model.AddElement(container);

            EdmEntityType customer = new EdmEntityType("NS", "Customer", null, false, false, true);
            customer.AddKeys(customer.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));

            var boolType = EdmCoreModel.Instance.GetBoolean(false);

            var actionEntitySetPath = new EdmPathExpression("Param1/Nav");
            var edmAction = new EdmAction("NS", "Checkout", boolType, true, actionEntitySetPath);
            edmAction.AddParameter(new EdmOperationParameter(edmAction, "bindingParameter", new EdmEntityTypeReference(customer, true)));
            edmAction.AddParameter("param", EdmCoreModel.Instance.GetString(true));
            model.AddElement(edmAction);

            var actionImportEntitySetPath = new EdmPathExpression("Param1/Nav2");
            var edmActionImport = new EdmActionImport(container, "CheckoutImport", edmAction, actionImportEntitySetPath);
            container.AddElement(edmActionImport);

            _model = model;
            _entityType = customer;
            _action = edmAction;
            _actionImport = edmActionImport;
        }

        [Fact]
        public void CreateRequestBodyForActionImportThrowArgumentNullContext()
        {
            // Arrange
            ODataContext context = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context", () => context.CreateRequestBody(actionImport: null, new()));
        }

        [Fact]
        public void CreateRequestBodyForActionImportThrowArgumentNullActionImport()
        {
            // Arrange
            ODataContext context = new ODataContext(EdmModelHelper.BasicEdmModel);

            // Act & Assert
            Assert.Throws<ArgumentNullException>("actionImport", () => context.CreateRequestBody(actionImport: null, new()));
        }

        [Fact]
        public void CreateRequestBodyForActionImportReturnCorrectRequestBody()
        {
            // Arrange
            ODataContext context = new ODataContext(_model);

            // Act
            var requestBody = context.CreateRequestBody(_actionImport, new());

            // Assert
            Assert.NotNull(requestBody);
            Assert.Equal("Action parameters", requestBody.Description);
            Assert.NotNull(requestBody.Content);
            var content = Assert.Single(requestBody.Content);
            Assert.Equal("application/json", content.Key);
            Assert.NotNull(content.Value);

            Assert.NotNull(content.Value.Schema);
            var schema = content.Value.Schema;
            Assert.Equal(JsonSchemaType.Object, schema.Type);
            Assert.NotNull(schema.Properties);
            var parameter = Assert.Single(schema.Properties);
            Assert.Equal("param", parameter.Key);
            Assert.Equal(JsonSchemaType.String | JsonSchemaType.Null, parameter.Value.Type);
        }

        [Fact]
        public async Task CanSerializeAsJsonFromTheCreatedRequestBody()
        {
            // Arrange
            ODataContext context = new ODataContext(_model);

            // Act
            var requestBody = context.CreateRequestBody(_actionImport, new());

            // Assert
            string json = await requestBody.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0);
            var expectedJson = @"{
  ""description"": ""Action parameters"",
  ""content"": {
    ""application/json"": {
      ""schema"": {
        ""type"": ""object"",
        ""properties"": {
          ""param"": {
            ""type"": ""string"",
            ""nullable"": true
          }
        }
      }
    }
  }
}";

            var actualJsonNode = JsonNode.Parse(json);
            var expectedJsonNode = JsonNode.Parse(expectedJson);

            Assert.True(JsonNode.DeepEquals(actualJsonNode, expectedJsonNode));
        }

        [Fact]
        public void CreateRequestBodyForActionReturnCorrectRequestBody()
        {
            // Arrange
            ODataContext context = new ODataContext(_model);

            // Act
            var requestBody = context.CreateRequestBody(_action, new());

            // Assert
            Assert.NotNull(requestBody);
            Assert.Equal("Action parameters", requestBody.Description);
            Assert.NotNull(requestBody.Content);
            var content = Assert.Single(requestBody.Content);
            Assert.Equal("application/json", content.Key);
            Assert.NotNull(content.Value);

            Assert.NotNull(content.Value.Schema);
            var schema = content.Value.Schema;
            Assert.Equal(JsonSchemaType.Object, schema.Type);
            Assert.NotNull(schema.Properties);
            var parameter = Assert.Single(schema.Properties);
            Assert.Equal("param", parameter.Key);
            Assert.Equal(JsonSchemaType.String | JsonSchemaType.Null, parameter.Value.Type);
        }

        [Fact]
        public void CreateRefRequestBodies()
        {
            // Arrange
            ODataContext context = new ODataContext(_model);
            OpenApiDocument openApiDocument = new OpenApiDocument();

            // Act
            context.AddRequestBodiesToDocument(openApiDocument);
            var requestBodies = openApiDocument.Components.RequestBodies;
            requestBodies.TryGetValue(Common.Constants.ReferencePostRequestBodyName, out var refPostBody);

            // Assert
            Assert.NotNull(refPostBody);
            Assert.Equal("New navigation property ref value", refPostBody.Description);
            var schemaReference = Assert.IsType<OpenApiSchemaReference>(refPostBody.Content.First().Value.Schema);
            Assert.Equal(Common.Constants.ReferenceCreateSchemaName, schemaReference.Reference.Id);
        }
    }
}
