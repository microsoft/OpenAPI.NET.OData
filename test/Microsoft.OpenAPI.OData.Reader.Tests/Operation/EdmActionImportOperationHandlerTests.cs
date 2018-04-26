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
            Assert.Equal("Invoke action ResetDataSource", operation.Summary);
            Assert.NotNull(operation.Tags);

            Assert.NotNull(operation.Parameters);
            Assert.Empty(operation.Parameters);

            Assert.Null(operation.RequestBody);

            Assert.Equal(2, operation.Responses.Count);
            Assert.Equal(new string[] { "204", "default" }, operation.Responses.Select(e => e.Key));
        }
    }
}
