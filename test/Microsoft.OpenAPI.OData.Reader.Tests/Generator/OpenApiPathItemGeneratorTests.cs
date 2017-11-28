// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Tests;
using Xunit;

namespace Microsoft.OpenApi.OData.Generator.Tests
{
    public class OpenApiPathItemGeneratorTest
    {
        [Fact]
        public void CreatePathItemsThrowArgumentNullContext()
        {
            // Arrange
            ODataContext context = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context", () => context.CreatePathItems());
        }

        [Fact]
        public void CreatePathItemsReturnsForEmptyModel()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.EmptyModel;
            ODataContext context = new ODataContext(model);

            // Act
            var pathItems = context.CreatePathItems();

            // Assert
            Assert.NotNull(pathItems);
            Assert.Empty(pathItems);
        }

        [Fact]
        public void CreatePathItemsReturnsForBasicModel()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.BasicEdmModel;
            ODataContext context = new ODataContext(model);

            // Act
            var pathItems = context.CreatePathItems();

            // Assert
            Assert.NotNull(pathItems);
            Assert.Equal(7, pathItems.Count);

            Assert.Contains("/People", pathItems.Keys);
            Assert.Contains("/People('{UserName}')", pathItems.Keys);
            Assert.Contains("/City", pathItems.Keys);
            Assert.Contains("/City('{Name}')", pathItems.Keys);
            Assert.Contains("/CountryOrRegion", pathItems.Keys);
            Assert.Contains("/CountryOrRegion('{Name}')", pathItems.Keys);
            Assert.Contains("/Me", pathItems.Keys);
        }

        #region EntitySet PathItem
        [Fact]
        public void CreateEntitySetPathItemThrowArgumentNullContext()
        {
            // Arrange
            ODataContext context = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context", () => context.CreateEntitySetPathItem(entitySet: null));
        }

        [Fact]
        public void CreateEntitySetPathItemThrowArgumentNullEntitySet()
        {
            // Arrange
            ODataContext context = new ODataContext(EdmModelHelper.BasicEdmModel);

            // Act & Assert
            Assert.Throws<ArgumentNullException>("entitySet", () => context.CreateEntitySetPathItem(entitySet: null));
        }

        [Fact]
        public void CreateEntitySetPathItemReturnsCorrectPathItem()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.BasicEdmModel;
            ODataContext context = new ODataContext(model);
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("People");
            Assert.NotNull(entitySet); // guard

            // Act
            var pathItem = context.CreateEntitySetPathItem(entitySet);

            // Assert
            Assert.NotNull(pathItem);

            Assert.NotNull(pathItem.Operations);
            Assert.NotEmpty(pathItem.Operations);
            Assert.Equal(2, pathItem.Operations.Count);
            Assert.Equal(new OperationType[] { OperationType.Get, OperationType.Post },
                pathItem.Operations.Select(o => o.Key));
        }
        #endregion

        #region Entity PathItem
        [Fact]
        public void CreateEntityPathItemThrowArgumentNullContext()
        {
            // Arrange
            ODataContext context = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context", () => context.CreateEntityPathItem(entitySet: null));
        }

        [Fact]
        public void CreateEntityPathItemThrowArgumentNullEntitySet()
        {
            // Arrange
            ODataContext context = new ODataContext(EdmModelHelper.BasicEdmModel);

            // Act & Assert
            Assert.Throws<ArgumentNullException>("entitySet", () => context.CreateEntityPathItem(entitySet: null));
        }

        [Fact]
        public void CreateEntityPathItemReturnsCorrectPathItem()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.BasicEdmModel;
            ODataContext context = new ODataContext(model);
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("People");
            Assert.NotNull(entitySet); // guard

            // Act
            var pathItem = context.CreateEntityPathItem(entitySet);

            // Assert
            Assert.NotNull(pathItem);

            Assert.NotNull(pathItem.Operations);
            Assert.NotEmpty(pathItem.Operations);
            Assert.Equal(3, pathItem.Operations.Count);
            Assert.Equal(new OperationType[] { OperationType.Get, OperationType.Patch, OperationType.Delete },
                pathItem.Operations.Select(o => o.Key));
        }
        #endregion

        #region Singleton PathItem
        [Fact]
        public void CreateSingletonPathItemThrowArgumentNullContext()
        {
            // Arrange
            ODataContext context = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context", () => context.CreateSingletonPathItem(singleton: null));
        }

        [Fact]
        public void CreateSingletonPathItemThrowArgumentNullSingleton()
        {
            // Arrange
            ODataContext context = new ODataContext(EdmModelHelper.BasicEdmModel);

            // Act & Assert
            Assert.Throws<ArgumentNullException>("singleton", () => context.CreateSingletonPathItem(singleton: null));
        }

        [Fact]
        public void CreateSingletonPathItemReturnsCorrectPathItem()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.BasicEdmModel;
            ODataContext context = new ODataContext(model);
            IEdmSingleton singleton = model.EntityContainer.FindSingleton("Me");
            Assert.NotNull(singleton); // guard

            // Act
            var pathItem = context.CreateSingletonPathItem(singleton);

            // Assert
            Assert.NotNull(pathItem);

            Assert.NotNull(pathItem.Operations);
            Assert.NotEmpty(pathItem.Operations);
            Assert.Equal(2, pathItem.Operations.Count);
            Assert.Equal(new OperationType[] { OperationType.Get, OperationType.Patch },
                pathItem.Operations.Select(o => o.Key));
        }

        #endregion

        [Fact]
        public void CreateEntityPathNameThrowArgumentNullContext()
        {
            // Arrange
            ODataContext context = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context", () => context.CreateEntityPathName(entitySet: null));
        }

        [Fact]
        public void CreateEntityPathNameThrowArgumentNullEntitySet()
        {
            // Arrange
            ODataContext context = new ODataContext(EdmModelHelper.EmptyModel);

            // Act & Assert
            Assert.Throws<ArgumentNullException>("entitySet", () => context.CreateEntityPathName(entitySet: null));
        }

        [Fact]
        public void CreateEntityPathNameReturnsCorrectPathItemName()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.BasicEdmModel;
            ODataContext context = new ODataContext(model);
            IEdmEntitySet people = model.EntityContainer.FindEntitySet("People");
            Assert.NotNull(people); // guard

            // Act
            string name = context.CreateEntityPathName(people);

            // Assert
            Assert.NotNull(name);
            Assert.Equal("/People('{UserName}')", name);
        }

        [Fact]
        public void CreateEntityPathNameReturnsCorrectPathItemNameWithKeyAsSegment()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.BasicEdmModel;
            ODataContext context = new ODataContext(model, new OpenApiConvertSettings
            {
                KeyAsSegment = true
            });
            IEdmEntitySet people = model.EntityContainer.FindEntitySet("People");
            Assert.NotNull(people); // guard

            // Act
            string name = context.CreateEntityPathName(people);

            // Assert
            Assert.NotNull(name);
            Assert.Equal("/People/{UserName}", name);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateEntityPathNameReturnsCorrectPathItemNameForCompositeKeys(bool keyAsSegment)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.CompositeKeyModel;
            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                KeyAsSegment = keyAsSegment
            };
            ODataContext context = new ODataContext(model, settings);
            IEdmEntitySet customers = model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(customers); // guard

            // Act
            string name = context.CreateEntityPathName(customers);

            // Assert
            Assert.NotNull(name);
            Assert.Equal("/Customers('Id={Id},Name={Name}')", name);
        }
    }
}
