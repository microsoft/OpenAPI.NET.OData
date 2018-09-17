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
    public class SingletonPatchOperationHandlerTests
    {
        private SingletonPatchOperationHandler _operationHandler = new SingletonPatchOperationHandler();

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateSingletonPatchOperationReturnsCorrectOperation(bool enableOperationId)
        {
            // Arrange
            IEdmModel model = SingletonGetOperationHandlerTests.GetEdmModel("");
            IEdmSingleton singleton = model.EntityContainer.FindSingleton("Me");
            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableOperationId = enableOperationId
            };
            ODataContext context = new ODataContext(model, settings);
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(singleton));

            // Act
            var patch = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(patch);
            Assert.Equal("Update Me", patch.Summary);
            Assert.NotNull(patch.Tags);
            var tag = Assert.Single(patch.Tags);
            Assert.Equal("Me.Customer", tag.Name);

            Assert.Empty(patch.Parameters);
            Assert.NotNull(patch.RequestBody);

            Assert.NotNull(patch.Responses);
            Assert.Equal(2, patch.Responses.Count);
            Assert.Equal(new[] { "204", "default" }, patch.Responses.Select(r => r.Key));

            if (enableOperationId)
            {
                Assert.Equal("Me.Customer.UpdateCustomer", patch.OperationId);
            }
            else
            {
                Assert.Null(patch.OperationId);
            }
        }
    }
}
