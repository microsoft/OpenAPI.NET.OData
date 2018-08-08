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
    public class EntitySetPostOperationHandlerTests
    {
        private EntitySetPostOperationHandler _operationHandler = new EntitySetPostOperationHandler();

        [Fact]
        public void CreateEntitySetPostOperationReturnsCorrectOperation()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.BasicEdmModel;
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("People");
            ODataContext context = new ODataContext(model);
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(entitySet));

            // Act
            var post = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(post);
            Assert.Equal("Add new entity to " + entitySet.Name, post.Summary);
            Assert.NotNull(post.Tags);
            var tag = Assert.Single(post.Tags);
            Assert.Equal("People.Person", tag.Name);

            Assert.Empty(post.Parameters);
            Assert.NotNull(post.RequestBody);

            Assert.NotNull(post.Responses);
            Assert.Equal(2, post.Responses.Count);
        }
    }
}
