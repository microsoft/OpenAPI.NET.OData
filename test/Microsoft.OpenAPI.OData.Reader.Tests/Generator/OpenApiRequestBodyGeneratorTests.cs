// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Extensions;
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
            Assert.Throws<ArgumentNullException>("context", () => context.CreateRequestBody(actionImport: null));
        }

        [Fact]
        public void CreateRequestBodyForActionImportThrowArgumentNullActionImport()
        {
            // Arrange
            ODataContext context = new ODataContext(EdmModelHelper.BasicEdmModel);

            // Act & Assert
            Assert.Throws<ArgumentNullException>("actionImport", () => context.CreateRequestBody(actionImport: null));
        }

        [Fact]
        public void CreateRequestBodyForActionImportReturnCorrectRequestBody()
        {
            // Arrange
            ODataContext context = new ODataContext(_model);

            // Act
            var requestBody = context.CreateRequestBody(_actionImport);

            // Assert
            Assert.NotNull(requestBody);
            Assert.Equal("Action parameters", requestBody.Description);
            Assert.NotNull(requestBody.Content);
            var content = Assert.Single(requestBody.Content);
            Assert.Equal("application/json", content.Key);
            Assert.NotNull(content.Value);

            Assert.NotNull(content.Value.Schema);
            var schema = content.Value.Schema;
            Assert.Equal("object", schema.Type);
            Assert.NotNull(schema.Properties);
            var parameter = Assert.Single(schema.Properties);
            Assert.Equal("param", parameter.Key);
            Assert.Equal("string", parameter.Value.Type);
        }

        [Fact]
        public void CanSerializeAsJsonFromTheCreatedRequestBody()
        {
            // Arrange
            ODataContext context = new ODataContext(_model);

            // Act
            var requestBody = context.CreateRequestBody(_actionImport);

            // Assert
            string json = requestBody.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0_0);

            Assert.Equal(@"{
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
  },
  ""required"": true
}".Replace(), json);
        }

        [Fact]
        public void CreateRequestBodyForActionReturnCorrectRequestBody()
        {
            // Arrange
            ODataContext context = new ODataContext(_model);

            // Act
            var requestBody = context.CreateRequestBody(_action);

            // Assert
            Assert.NotNull(requestBody);
            Assert.Equal("Action parameters", requestBody.Description);
            Assert.NotNull(requestBody.Content);
            var content = Assert.Single(requestBody.Content);
            Assert.Equal("application/json", content.Key);
            Assert.NotNull(content.Value);

            Assert.NotNull(content.Value.Schema);
            var schema = content.Value.Schema;
            Assert.Equal("object", schema.Type);
            Assert.NotNull(schema.Properties);
            var parameter = Assert.Single(schema.Properties);
            Assert.Equal("param", parameter.Key);
            Assert.Equal("string", parameter.Value.Type);
        }
    }
}
