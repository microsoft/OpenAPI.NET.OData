// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Tests;
using Xunit;

namespace Microsoft.OpenApi.OData.Operation.Tests
{
    public class EntitySetGetOperationHandlerTests
    {
        private EntitySetGetOperationHandler _operationHandler = new EntitySetGetOperationHandler();

        [Fact]
        public void CreateEntitySetGetOperationReturnsCorrectOperation()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.BasicEdmModel;
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("People");
            ODataContext context = new ODataContext(model);
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(entitySet));

            // Act
            var get = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(get);
            Assert.Equal("Get entities from " + entitySet.Name, get.Summary);
            Assert.NotNull(get.Tags);
            var tag = Assert.Single(get.Tags);
            Assert.Equal("People.Person", tag.Name);

            Assert.NotNull(get.Parameters);
            Assert.Equal(8, get.Parameters.Count);

            Assert.NotNull(get.Responses);
            Assert.Equal(2, get.Responses.Count);
        }
    }
}
