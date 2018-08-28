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
    public class EdmActionImportOperationHandlerTests
    {
        private EdmActionImportOperationHandler _operationHandler = new EdmActionImportOperationHandler();

        [Fact]
        public void CreateOperationForEdmActionImportReturnsCorrectOperation()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            ODataContext context = new ODataContext(model);

            var actionImport = model.EntityContainer.FindOperationImports("ResetDataSource").FirstOrDefault();
            Assert.NotNull(actionImport);
            ODataPath path = new ODataPath(new ODataOperationImportSegment(actionImport));

            // Act
            var operation = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(operation);
            Assert.Equal("Invoke actionImport ResetDataSource", operation.Summary);
            Assert.NotNull(operation.Tags);

            Assert.NotNull(operation.Parameters);
            Assert.Empty(operation.Parameters);

            Assert.Null(operation.RequestBody);

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
            EdmAction action = new EdmAction("NS", "MyAction", EdmCoreModel.Instance.GetString(false), false, null);
            action.AddParameter("param", EdmCoreModel.Instance.GetString(false));
            model.AddElement(action);
            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
            EdmEntitySet customers = new EdmEntitySet(container, "Customers", customer);
            EdmActionImport actionImport = new EdmActionImport(container, "MyAction", action);
            model.AddElement(container);

            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                OperationId = enableOperationId
            };
            ODataContext context = new ODataContext(model, settings);

            ODataPath path = new ODataPath(new ODataOperationImportSegment(actionImport));

            // Act
            var operation = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(operation);

            if (enableOperationId)
            {
                Assert.Equal("OperationImport.MyAction", operation.OperationId);
            }
            else
            {
                Assert.Null(operation.OperationId);
            }
        }
    }
}
