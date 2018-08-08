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
    public class NavigationPropertyPathItemGeneratorTest
    {
        private NavigationPropertyPathItemHandler _pathItemHandler = new NavigationPropertyPathItemHandler();

        [Theory]
        [InlineData(true, true, new OperationType[] { OperationType.Get, OperationType.Patch })]
        [InlineData(true, false, new OperationType[] { OperationType.Get, OperationType.Post })]
        [InlineData(false, true, new OperationType[] { OperationType.Get })]
        [InlineData(false, false, new OperationType[] { OperationType.Get})]
        public void CreateCollectionNavigationPropertyPathItemReturnsCorrectPathItem(bool containment, bool keySegment, OperationType[] expected)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.GraphBetaModel;
            ODataContext context = new ODataContext(model);
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("users");
            Assert.NotNull(entitySet); // guard
            IEdmEntityType entityType = entitySet.EntityType();

            IEdmNavigationProperty property = entityType.DeclaredNavigationProperties()
                .FirstOrDefault(c => c.ContainsTarget == containment && c.TargetMultiplicity() == EdmMultiplicity.Many);
            Assert.NotNull(property);

            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(entitySet),
                new ODataKeySegment(entityType),
                new ODataNavigationPropertySegment(property));

            if (keySegment)
            {
                path.Push(new ODataKeySegment(property.ToEntityType()));
            }

            // Act
            var pathItem = _pathItemHandler.CreatePathItem(context, path);

            // Assert
            Assert.NotNull(pathItem);

            Assert.NotNull(pathItem.Operations);
            Assert.NotEmpty(pathItem.Operations);
            Assert.Equal(expected, pathItem.Operations.Select(o => o.Key));
        }

        [Theory]
        [InlineData(true, new OperationType[] { OperationType.Get, OperationType.Patch })]
        [InlineData(false, new OperationType[] { OperationType.Get })]
        public void CreateSingleNavigationPropertyPathItemReturnsCorrectPathItem(bool containment, OperationType[] expected)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.GraphBetaModel;
            ODataContext context = new ODataContext(model);
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("users");
            Assert.NotNull(entitySet); // guard
            IEdmEntityType entityType = entitySet.EntityType();

            IEdmNavigationProperty property = entityType.DeclaredNavigationProperties()
                .FirstOrDefault(c => c.ContainsTarget == containment && c.TargetMultiplicity() != EdmMultiplicity.Many);
            Assert.NotNull(property);

            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(entitySet),
                new ODataKeySegment(entityType),
                new ODataNavigationPropertySegment(property));

            // Act
            var pathItem = _pathItemHandler.CreatePathItem(context, path);

            // Assert
            Assert.NotNull(pathItem);

            Assert.NotNull(pathItem.Operations);
            Assert.NotEmpty(pathItem.Operations);
            Assert.Equal(expected, pathItem.Operations.Select(o => o.Key));
        }
    }
}
