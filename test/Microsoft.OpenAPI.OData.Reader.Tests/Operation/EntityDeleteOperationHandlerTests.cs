// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Edm;
using Xunit;

namespace Microsoft.OpenApi.OData.Operation.Tests
{
    public class EntityDeleteOperationHandlerTests
    {
        private EntityDeleteOperationHandler _operationHandler = new EntityDeleteOperationHandler();

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateEntityDeleteOperationReturnsCorrectOperation(bool enableOperationId)
        {
            // Arrange
            IEdmModel model = EntitySetGetOperationHandlerTests.GetEdmModel("");
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Customers");
            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableOperationId = enableOperationId
            };
            ODataContext context = new ODataContext(model, settings);
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(entitySet), new ODataKeySegment(entitySet.EntityType()));

            // Act
            var delete = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(delete);
            Assert.Equal("Delete entity from Customers", delete.Summary);
            Assert.NotNull(delete.Tags);
            var tag = Assert.Single(delete.Tags);
            Assert.Equal("Customers.Customer", tag.Name);

            Assert.NotNull(delete.Parameters);
            Assert.Equal(2, delete.Parameters.Count);

            Assert.Null(delete.RequestBody);

            Assert.NotNull(delete.Responses);
            Assert.Equal(2, delete.Responses.Count);
            Assert.Equal(new[] { "204", "default" }, delete.Responses.Select(r => r.Key));

            if (enableOperationId)
            {
                Assert.Equal("Customers.Customer.DeleteCustomer", delete.OperationId);
            }
            else
            {
                Assert.Null(delete.OperationId);
            }
        }
    }
}
