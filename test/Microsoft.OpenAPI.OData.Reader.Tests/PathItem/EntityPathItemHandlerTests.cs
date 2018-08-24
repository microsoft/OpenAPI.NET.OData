// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Properties;
using Xunit;

namespace Microsoft.OpenApi.OData.PathItem.Tests
{
    public class EntityPathItemHandlerTests
    {
        private EntityPathItemHandler _pathItemHandler = new EntityPathItemHandler();

        [Fact]
        public void CreatePathItemThrowsForNullContext()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>("context",
                () => _pathItemHandler.CreatePathItem(context: null, path: new ODataPath()));
        }

        [Fact]
        public void CreatePathItemThrowsForNullPath()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>("path",
                () => _pathItemHandler.CreatePathItem(new ODataContext(EdmCoreModel.Instance), path: null));
        }

        [Fact]
        public void CreatePathItemThrowsForNonEntityPath()
        {
            // Arrange
            IEdmModel model = EntitySetPathItemHandlerTests.GetEdmModel(annotation: "");
            ODataContext context = new ODataContext(model);
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(entitySet); // guard
            var path = new ODataPath(new ODataNavigationSourceSegment(entitySet));
            Assert.Equal(ODataPathKind.EntitySet, path.Kind); // guard

            // Act
            Action test = () => _pathItemHandler.CreatePathItem(context, path);

            // Assert
            var exception = Assert.Throws<InvalidOperationException>(test);
            Assert.Equal(String.Format(SRResource.InvalidPathKindForPathItemHandler, "EntityPathItemHandler", path.Kind), exception.Message);
        }

        [Fact]
        public void CreateEntityPathItemReturnsCorrectPathItem()
        {
            // Arrange
            IEdmModel model = EntitySetPathItemHandlerTests.GetEdmModel(annotation: "");
            ODataContext context = new ODataContext(model);
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(entitySet); // guard
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(entitySet), new ODataKeySegment(entitySet.EntityType()));

            // Act
            var pathItem = _pathItemHandler.CreatePathItem(context, path);

            // Assert
            Assert.NotNull(pathItem);

            Assert.NotNull(pathItem.Operations);
            Assert.NotEmpty(pathItem.Operations);
            Assert.Equal(3, pathItem.Operations.Count);
            Assert.Equal(new OperationType[] { OperationType.Get, OperationType.Patch, OperationType.Delete },
                pathItem.Operations.Select(o => o.Key));
        }

        [Theory]
        [InlineData(true, new OperationType[] { OperationType.Get, OperationType.Patch, OperationType.Delete })]
        [InlineData(false, new OperationType[] { OperationType.Patch, OperationType.Delete })]
        public void CreateEntityPathItemWorksForIndexableByKeyRestrictionsCapablities(bool indexableByKey, OperationType[] expected)
        {
            // Arrange
            string annotation = String.Format(@"
<Annotation Term=""Org.OData.Capabilities.V1.IndexableByKey"" Bool=""{0}"" />", indexableByKey);

            IEdmModel model = EntitySetPathItemHandlerTests.GetEdmModel(annotation);
            ODataContext context = new ODataContext(model);
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(entitySet); // guard
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(entitySet), new ODataKeySegment(entitySet.EntityType()));

            // Act
            var pathItem = _pathItemHandler.CreatePathItem(context, path);

            // Assert
            Assert.NotNull(pathItem);

            Assert.NotNull(pathItem.Operations);
            Assert.NotEmpty(pathItem.Operations);
            Assert.Equal(expected, pathItem.Operations.Select(e => e.Key));
        }

        [Theory]
        [InlineData(true, new OperationType[] { OperationType.Get, OperationType.Patch, OperationType.Delete })]
        [InlineData(false, new OperationType[] { OperationType.Get, OperationType.Delete })]
        public void CreateEntityPathItemWorksForUpdateRestrictionsCapablities(bool updatable, OperationType[] expected)
        {
            // Arrange
            string annotation = String.Format(@"
<Annotation Term=""Org.OData.Capabilities.V1.UpdateRestrictions"">
  <Record>
    <PropertyValue Property=""Updatable"" Bool=""{0}"" />
  </Record>
</Annotation>", updatable);

            IEdmModel model = EntitySetPathItemHandlerTests.GetEdmModel(annotation);
            ODataContext context = new ODataContext(model);
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(entitySet); // guard
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(entitySet), new ODataKeySegment(entitySet.EntityType()));

            // Act
            var pathItem = _pathItemHandler.CreatePathItem(context, path);

            // Assert
            Assert.NotNull(pathItem);

            Assert.NotNull(pathItem.Operations);
            Assert.NotEmpty(pathItem.Operations);
            Assert.Equal(expected, pathItem.Operations.Select(e => e.Key));
        }

        [Theory]
        [InlineData(true, new OperationType[] { OperationType.Get, OperationType.Patch, OperationType.Delete })]
        [InlineData(false, new OperationType[] { OperationType.Get, OperationType.Patch })]
        public void CreateEntityPathItemWorksForDeleteRestrictionsCapablities(bool deletable, OperationType[] expected)
        {
            // Arrange
            string annotation = String.Format(@"
<Annotation Term=""Org.OData.Capabilities.V1.DeleteRestrictions"">
  <Record>
    <PropertyValue Property=""Deletable"" Bool=""{0}"" />
  </Record>
</Annotation>", deletable);

            IEdmModel model = EntitySetPathItemHandlerTests.GetEdmModel(annotation);
            ODataContext context = new ODataContext(model);
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(entitySet); // guard
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(entitySet), new ODataKeySegment(entitySet.EntityType()));

            // Act
            var pathItem = _pathItemHandler.CreatePathItem(context, path);

            // Assert
            Assert.NotNull(pathItem);

            Assert.NotNull(pathItem.Operations);
            Assert.NotEmpty(pathItem.Operations);
            Assert.Equal(expected, pathItem.Operations.Select(e => e.Key));
        }
    }
}
