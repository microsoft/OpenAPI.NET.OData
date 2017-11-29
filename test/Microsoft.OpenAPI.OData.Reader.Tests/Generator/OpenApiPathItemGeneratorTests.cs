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

        #region Bound Edm Operation PathItem
        [Fact]
        public void CreatePathItemForOperationThrowArgumentNullContext()
        {
            // Arrange
            ODataContext context = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context",
                () => context.CreatePathItem(navigationSource: null, edmOperation: null));
        }

        [Fact]
        public void CreatePathItemForOperationThrowArgumentNullNavigationSource()
        {
            // Arrange
            ODataContext context = new ODataContext(EdmModelHelper.EmptyModel);

            // Act & Assert
            Assert.Throws<ArgumentNullException>("navigationSource",
                () => context.CreatePathItem(navigationSource: null, edmOperation: null));
        }

        [Fact]
        public void CreatePathItemForOperationThrowArgumentNulledmOperation()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.BasicEdmModel;
            ODataContext context = new ODataContext(model);
            IEdmEntitySet entitySet = model.EntityContainer.EntitySets().First();

            // Act & Assert
            Assert.Throws<ArgumentNullException>("edmOperation",
                () => context.CreatePathItem(navigationSource: entitySet, edmOperation: null));
        }

        [Theory]
        [InlineData("GetFriendsTrips", "People", OperationType.Get)]
        [InlineData("ShareTrip", "People", OperationType.Post)]
        public void CreatePathItemForOperationReturnsCorrectPathItem(string operationName, string entitySet,
            OperationType operationType)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            ODataContext context = new ODataContext(model);
            IEdmNavigationSource navigationSource = model.EntityContainer.FindEntitySet(entitySet);
            Assert.NotNull(navigationSource); // guard
            IEdmOperation edmOperation = model.SchemaElements.OfType<IEdmOperation>()
                .FirstOrDefault(o => o.Name == operationName);
            Assert.NotNull(edmOperation); // guard
            string expectSummary = "Invoke " +
                (edmOperation.IsAction() ? "action " : "function ") + operationName;

            // Act
            OpenApiPathItem pathItem = context.CreatePathItem(navigationSource, edmOperation);

            // Assert
            Assert.NotNull(pathItem);
            Assert.NotNull(pathItem.Operations);
            var operationKeyValue = Assert.Single(pathItem.Operations);
            Assert.Equal(operationType, operationKeyValue.Key);
            Assert.NotNull(operationKeyValue.Value);

            Assert.Equal(expectSummary, operationKeyValue.Value.Summary);
        }
        #endregion

        #region Edm OperationImport PathItem
        [Fact]
        public void CreatePathItemForOperationImportThrowArgumentNullContext()
        {
            // Arrange
            ODataContext context = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context", () => context.CreatePathItem(operationImport: null));
        }

        [Fact]
        public void CreatePathItemForOperationImportThrowArgumentNullOperationImport()
        {
            // Arrange
            ODataContext context = new ODataContext(EdmModelHelper.EmptyModel);

            // Act & Assert
            Assert.Throws<ArgumentNullException>("operationImport", () => context.CreatePathItem(operationImport: null));
        }

        [Theory]
        [InlineData("GetNearestAirport", OperationType.Get)]
        [InlineData("ResetDataSource", OperationType.Post)]
        public void CreatePathItemForOperationImportReturnsCorrectPathItem(string operationImport,
            OperationType operationType)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            ODataContext context = new ODataContext(model);
            IEdmOperationImport edmOperationImport = model.EntityContainer
                .OperationImports().FirstOrDefault(o => o.Name == operationImport);
            Assert.NotNull(edmOperationImport); // guard
            string expectSummary = "Invoke " +
                (edmOperationImport.IsActionImport() ? "action " : "function ") + operationImport;

            // Act
            OpenApiPathItem pathItem = context.CreatePathItem(edmOperationImport);

            // Assert
            Assert.NotNull(pathItem);
            Assert.NotNull(pathItem.Operations);
            var operationKeyValue = Assert.Single(pathItem.Operations);
            Assert.Equal(operationType, operationKeyValue.Key);
            Assert.NotNull(operationKeyValue.Value);

            Assert.Equal(expectSummary, operationKeyValue.Value.Summary);
        }
        #endregion
    }
}
