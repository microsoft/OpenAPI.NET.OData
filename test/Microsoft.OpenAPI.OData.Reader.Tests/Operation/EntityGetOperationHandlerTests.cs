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
    public class EntityGetOperationHandlerTests
    {
        private EntityGetOperationHandler _operationHandler = new EntityGetOperationHandler();

        [Fact]
        public void CreateEntityGetOperationReturnsCorrectOperation()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.BasicEdmModel;
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("People");
            ODataContext context = new ODataContext(model);
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(entitySet), new ODataKeySegment(entitySet.EntityType()));

            // Act
            var get = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(get);
            Assert.Equal("Get entity from People by key", get.Summary);
            Assert.NotNull(get.Tags);
            var tag = Assert.Single(get.Tags);
            Assert.Equal("People.Person", tag.Name);

            Assert.NotNull(get.Parameters);
            Assert.Equal(3, get.Parameters.Count);
            Assert.Null(get.RequestBody);

            Assert.NotNull(get.Responses);
            Assert.Equal(2, get.Responses.Count);
            Assert.Equal(new[] { "200", "default" }, get.Responses.Select(r => r.Key));
        }
    }
}
