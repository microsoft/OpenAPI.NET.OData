// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Edm;
using Xunit;

namespace Microsoft.OpenApi.OData.Operation.Tests
{
    public class EntitySetPostOperationHandlerTests
    {
        private EntitySetPostOperationHandler _operationHandler = new EntitySetPostOperationHandler();

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateEntitySetPostOperationReturnsCorrectOperation(bool enableOperationId)
        {
            // Arrange
            IEdmModel model = EntitySetGetOperationHandlerTests.GetEdmModel("");
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Customers");
            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableOperationId = enableOperationId
            };
            ODataContext context = new ODataContext(model, settings);
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(entitySet));

            // Act
            var post = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(post);
            Assert.Equal("Add new entity to " + entitySet.Name, post.Summary);
            Assert.NotNull(post.Tags);
            var tag = Assert.Single(post.Tags);
            Assert.Equal("Customers.Customer", tag.Name);

            Assert.Empty(post.Parameters);
            Assert.NotNull(post.RequestBody);

            Assert.NotNull(post.Responses);
            Assert.Equal(2, post.Responses.Count);

            if (enableOperationId)
            {
                Assert.Equal("Customers.Customer.CreateCustomer", post.OperationId);
            }
            else
            {
                Assert.Null(post.OperationId);
            }
        }
    }
}
