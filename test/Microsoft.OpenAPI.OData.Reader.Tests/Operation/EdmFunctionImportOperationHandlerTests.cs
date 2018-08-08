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
            Assert.Equal("Invoke function GetPersonWithMostFriends", operation.Summary);
            Assert.NotNull(operation.Tags);
            var tag = Assert.Single(operation.Tags);
            Assert.Equal("People", tag.Name);

            Assert.NotNull(operation.Parameters);
            Assert.Empty(operation.Parameters);

            Assert.Null(operation.RequestBody);

            Assert.Equal(2, operation.Responses.Count);
            Assert.Equal(new string[] { "200", "default" }, operation.Responses.Select(e => e.Key));
        }
    }
}
