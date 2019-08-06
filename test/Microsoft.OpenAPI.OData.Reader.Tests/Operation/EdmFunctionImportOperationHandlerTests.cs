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
    public class EdmFunctionImportOperationHandlerTests
    {
        private EdmFunctionImportOperationHandler _operationHandler = new EdmFunctionImportOperationHandler();

        [Fact]
        public void CreateOperationForEdmFunctionImportReturnsCorrectOperation()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            ODataContext context = new ODataContext(model);
            var functionImport = model.EntityContainer.FindOperationImports("GetPersonWithMostFriends").FirstOrDefault();
            Assert.NotNull(functionImport);
            ODataPath path = new ODataPath(new ODataOperationImportSegment(functionImport));

            // Act
            var operation = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(operation);
            Assert.Equal("Invoke functionImport GetPersonWithMostFriends", operation.Summary);
            Assert.NotNull(operation.Tags);
            var tag = Assert.Single(operation.Tags);
            Assert.Equal("People", tag.Name);

            Assert.NotNull(operation.Parameters);
            Assert.Empty(operation.Parameters);

            Assert.Null(operation.RequestBody);

            Assert.Equal(2, operation.Responses.Count);
            Assert.Equal(new string[] { "200", "default" }, operation.Responses.Select(e => e.Key));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateOperationForEdmFunctionImportReturnsCorrectOperationId(bool enableOperationId)
        {
            // Arrange
            EdmModel model = new EdmModel();
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));
            model.AddElement(customer);
            EdmFunction function = new EdmFunction("NS", "MyFunction", EdmCoreModel.Instance.GetString(false), false, null, false);
            function.AddParameter("entity", new EdmEntityTypeReference(customer, false));
            function.AddParameter("param", EdmCoreModel.Instance.GetString(false));
            model.AddElement(function);
            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
            EdmFunctionImport functionImport = new EdmFunctionImport(container, "MyFunction", function);
            model.AddElement(container);

            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableOperationId = enableOperationId
            };
            ODataContext context = new ODataContext(model, settings);

            ODataPath path = new ODataPath(new ODataOperationImportSegment(functionImport));

            // Act
            var operation = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(operation);

            if (enableOperationId)
            {
                Assert.Equal("OperationImport.MyFunction.790300f48b60d73292a9c056", operation.OperationId);
            }
            else
            {
                Assert.Null(operation.OperationId);
            }
        }
    }
}
