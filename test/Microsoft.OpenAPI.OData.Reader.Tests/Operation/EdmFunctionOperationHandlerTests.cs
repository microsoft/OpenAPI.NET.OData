// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Validation;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Tests;
using Xunit;

namespace Microsoft.OpenApi.OData.Operation.Tests
{
    public class EdmFunctionOperationHandlerTests
    {
        private EdmFunctionOperationHandler _operationHandler = new EdmFunctionOperationHandler();

        [Fact]
        public void CreateOperationForEdmFunctionReturnsCorrectOperation()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            ODataContext context = new ODataContext(model);
            IEdmEntitySet people = model.EntityContainer.FindEntitySet("People");
            Assert.NotNull(people);

            IEdmFunction function = model.SchemaElements.OfType<IEdmFunction>().First(f => f.Name == "GetFavoriteAirline");
            Assert.NotNull(function);

            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(people), new ODataKeySegment(people.EntityType()), new ODataOperationSegment(function));

            // Act
            var operation = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(operation);
            Assert.Equal("Invoke function GetFavoriteAirline", operation.Summary);
            Assert.NotNull(operation.Tags);
            var tag = Assert.Single(operation.Tags);
            Assert.Equal("People.Functions", tag.Name);

            Assert.NotNull(operation.Parameters);
            Assert.Equal(1, operation.Parameters.Count);
            Assert.Equal(new string[] { "UserName" }, operation.Parameters.Select(p => p.Name));

            Assert.Null(operation.RequestBody);

            Assert.Equal(2, operation.Responses.Count);
            Assert.Equal(new string[] { "200", "default" }, operation.Responses.Select(e => e.Key));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateOperationForEdmFunctionReturnsCorrectOperationId(bool enableOperationId)
        {
            // Arrange
            EdmModel model = new EdmModel();
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));
            model.AddElement(customer);
            EdmFunction function = new EdmFunction("NS", "MyFunction", EdmCoreModel.Instance.GetString(false), true, null, false);
            function.AddParameter("entity", new EdmEntityTypeReference(customer, false));
            function.AddParameter("param", EdmCoreModel.Instance.GetString(false));
            model.AddElement(function);
            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
            EdmEntitySet customers = new EdmEntitySet(container, "Customers", customer);
            model.AddElement(container);

            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableOperationId = enableOperationId
            };
            ODataContext context = new ODataContext(model, settings);

            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(customers),
                new ODataKeySegment(customer),
                new ODataOperationSegment(function));

            // Act
            var operation = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(operation);

            if (enableOperationId)
            {
                Assert.Equal("Customers.MyFunction.373f", operation.OperationId);
            }
            else
            {
                Assert.Null(operation.OperationId);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateOperationForEdmFunctionWithTypeCastReturnsCorrectOperationId(bool enableOperationId)
        {
            // Arrange
            EdmModel model = new EdmModel();
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));
            model.AddElement(customer);
            EdmEntityType vipCustomer = new EdmEntityType("NS", "VipCustomer", customer);
            model.AddElement(vipCustomer);
            EdmFunction function = new EdmFunction("NS", "MyFunction", EdmCoreModel.Instance.GetString(false), true, null, false);
            function.AddParameter("entity", new EdmEntityTypeReference(vipCustomer, false));
            function.AddParameter("param", EdmCoreModel.Instance.GetString(false));
            model.AddElement(function);
            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
            EdmEntitySet customers = new EdmEntitySet(container, "Customers", customer);
            model.AddElement(container);

            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableOperationId = enableOperationId
            };
            ODataContext context = new ODataContext(model, settings);

            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(customers),
                new ODataKeySegment(customer),
                new ODataTypeCastSegment(vipCustomer),
                new ODataOperationSegment(function));

            // Act
            var operation = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(operation);

            if (enableOperationId)
            {
                Assert.Equal("Customers.NS.VipCustomer.MyFunction.373f", operation.OperationId);
            }
            else
            {
                Assert.Null(operation.OperationId);
            }
        }
    }
}
