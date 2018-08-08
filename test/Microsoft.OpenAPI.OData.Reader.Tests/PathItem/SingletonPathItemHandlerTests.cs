// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Tests;
using Xunit;

namespace Microsoft.OpenApi.OData.PathItem.Tests
{
    public class SingletonPathItemGeneratorTest
    {
        private SingletonPathItemHandler _pathItemHandler = new SingletonPathItemHandler();

        [Fact]
        public void CreateSingletonPathItemReturnsCorrectPathItem()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.BasicEdmModel;
            ODataContext context = new ODataContext(model);
            IEdmSingleton singleton = model.EntityContainer.FindSingleton("Me");
            Assert.NotNull(singleton); // guard
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(singleton));

            // Act
            var pathItem = _pathItemHandler.CreatePathItem(context, path);

            // Assert
            Assert.NotNull(pathItem);

            Assert.NotNull(pathItem.Operations);
            Assert.NotEmpty(pathItem.Operations);
            Assert.Equal(2, pathItem.Operations.Count);
            Assert.Equal(new OperationType[] { OperationType.Get, OperationType.Patch },
                pathItem.Operations.Select(o => o.Key));
        }
    }
}
