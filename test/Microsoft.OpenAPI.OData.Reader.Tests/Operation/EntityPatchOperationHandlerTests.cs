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
    public class EntityPatchOperationHandlerTests
    {
        private EntityPatchOperationHandler _operationHandler = new EntityPatchOperationHandler();

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateEntityPatchOperationReturnsCorrectOperation(bool enableOperationId)
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
            var patch = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(patch);
            Assert.Equal("Update entity in Customers", patch.Summary);
            Assert.NotNull(patch.Tags);
            var tag = Assert.Single(patch.Tags);
            Assert.Equal("Customers.Customer", tag.Name);

            Assert.NotNull(patch.Parameters);
            Assert.Equal(1, patch.Parameters.Count);

            Assert.NotNull(patch.RequestBody);

            Assert.NotNull(patch.Responses);
            Assert.Equal(2, patch.Responses.Count);
            Assert.Equal(new[] { "204", "default" }, patch.Responses.Select(r => r.Key));

            if (enableOperationId)
            {
                Assert.Equal("Customers.Customer.UpdateCustomer", patch.OperationId);
            }
            else
            {
                Assert.Null(patch.OperationId);
            }
        }
    }
}
