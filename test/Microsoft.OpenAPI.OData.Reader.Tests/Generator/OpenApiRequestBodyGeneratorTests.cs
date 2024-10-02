﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Extensions;
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
            edmAction.AddParameter("param2", EdmCoreModel.Instance.GetString(false));

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

            var parameters = schema.Properties;
            Assert.Equal(2, parameters.Count);

            var parameter1 = parameters.First(p => p.Key == "param");
            Assert.Equal("param", parameter1.Key);
            Assert.Equal("string", parameter1.Value.Type);

            var parameter2 = parameters.First(p => p.Key == "param2");
            Assert.Equal("param2", parameter2.Key);
            Assert.Equal("string", parameter2.Value.Type);
        }

        [Fact]
        public void CanSerializeAsJsonFromTheCreatedRequestBody()
        {
            // Arrange
            ODataContext context = new ODataContext(_model);

            // Act
            var requestBody = context.CreateRequestBody(_actionImport);

            // Assert
            string json = requestBody.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);

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
          },
          ""param2"": {
            ""type"": ""string""
          }
        }
      }
    }
  },
  ""required"": true
}".ChangeLineBreaks(), json);
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

            var parameters = schema.Properties;
            Assert.Equal(2, parameters.Count);

            var parameter1 = parameters.First(p => p.Key == "param");
            Assert.Equal("param", parameter1.Key);
            Assert.Equal("string", parameter1.Value.Type);

            var parameter2 = parameters.First(p => p.Key == "param2");
            Assert.Equal("param2", parameter2.Key);
            Assert.Equal("string", parameter2.Value.Type);
        }

        [Fact]
        public void CreateRefRequestBodies()
        {
            // Arrange
            ODataContext context = new ODataContext(_model);

            // Act
            var requestBodies = context.CreateRequestBodies();
            requestBodies.TryGetValue(Common.Constants.ReferencePostRequestBodyName, out Models.OpenApiRequestBody refPostBody);

            // Assert
            Assert.NotNull(refPostBody);
            Assert.Equal("New navigation property ref value", refPostBody.Description);
            Assert.Equal(Common.Constants.ReferenceCreateSchemaName, refPostBody.Content.First().Value.Schema.Reference.Id);
        }
    }
}
