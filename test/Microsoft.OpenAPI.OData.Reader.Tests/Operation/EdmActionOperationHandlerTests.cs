// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Tests;
using Xunit;

namespace Microsoft.OpenApi.OData.Operation.Tests
{
    public class EdmActionOperationHandlerTests
    {
        private EdmActionOperationHandler _operationHandler = new EdmActionOperationHandler();

        [Fact]
        public void CreateOperationForEdmActionReturnsCorrectOperation()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            ODataContext context = new ODataContext(model);
            IEdmEntitySet people = model.EntityContainer.FindEntitySet("People");
            Assert.NotNull(people);

            IEdmAction action = model.SchemaElements.OfType<IEdmAction>().First(f => f.Name == "ShareTrip");
            Assert.NotNull(action);
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(people), new ODataKeySegment(people.EntityType()), new ODataOperationSegment(action));

            // Act
            var operation = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(operation);
            Assert.Equal("Invoke action ShareTrip", operation.Summary);
            Assert.NotNull(operation.Tags);
            var tag = Assert.Single(operation.Tags);
            Assert.Equal("People.Actions", tag.Name);

            Assert.NotNull(operation.Parameters);
            Assert.Equal(1, operation.Parameters.Count);
            Assert.Equal(new string[] { "UserName" }, operation.Parameters.Select(p => p.Name));

            Assert.NotNull(operation.RequestBody);
            Assert.Equal("Action parameters", operation.RequestBody.Description);

            Assert.Equal(2, operation.Responses.Count);
            Assert.Equal(new string[] { "204", "default" }, operation.Responses.Select(e => e.Key));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateOperationForEdmActionReturnsCorrectOperationId(bool enableOperationId)
        {
            // Arrange
            EdmModel model = new EdmModel();
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));
            model.AddElement(customer);
            EdmAction action = new EdmAction("NS", "MyAction", EdmCoreModel.Instance.GetString(false), true, null);
            action.AddParameter("entity", new EdmEntityTypeReference(customer, false));
            action.AddParameter("param", EdmCoreModel.Instance.GetString(false));
            model.AddElement(action);
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
                new ODataOperationSegment(action));

            // Act
            var operation = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(operation);

            if (enableOperationId)
            {
                Assert.Equal("Customers.Customer.MyAction", operation.OperationId);
            }
            else
            {
                Assert.Null(operation.OperationId);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateOperationForEdmActionWithTypeCastReturnsCorrectOperationId(bool enableOperationId)
        {
            // Arrange
            EdmModel model = new EdmModel();
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));
            model.AddElement(customer);
            EdmEntityType vipCustomer = new EdmEntityType("NS", "VipCustomer", customer);
            model.AddElement(vipCustomer);
            EdmAction action = new EdmAction("NS", "MyAction", EdmCoreModel.Instance.GetString(false), true, null);
            action.AddParameter("entity", new EdmEntityTypeReference(vipCustomer, false));
            action.AddParameter("param", EdmCoreModel.Instance.GetString(false));
            model.AddElement(action);
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
                new ODataOperationSegment(action));

            // Act
            var operation = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(operation);

            if (enableOperationId)
            {
                Assert.Equal("Customers.VipCustomer.MyAction", operation.OperationId);
            }
            else
            {
                Assert.Null(operation.OperationId);
            }
        }
    }
}
