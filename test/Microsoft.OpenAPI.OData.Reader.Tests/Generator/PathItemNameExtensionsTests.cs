// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Tests;
using Xunit;

namespace Microsoft.OpenApi.OData.Generator.Tests
{
    public class PathItemNameExtensionsTest
    {
        #region Entity PathItem Name
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
        #endregion

        #region OperationImport PathItem Name
        [Fact]
        public void CreatePathItemNameForOperationImportThrowArgumentNullContext()
        {
            // Arrange
            ODataContext context = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context", () => context.CreatePathItemName(operationImport: null));
        }

        [Fact]
        public void CreatePathItemNameForOperationImportThrowArgumentNullOperationImport()
        {
            // Arrange
            ODataContext context = new ODataContext(EdmModelHelper.EmptyModel);

            // Act & Assert
            Assert.Throws<ArgumentNullException>("operationImport", () => context.CreatePathItemName(operationImport: null));
        }

        [Fact]
        public void CreatePathItemNameForActionImportReturnCorrectName()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            ODataContext context = new ODataContext(model);
            IEdmActionImport actionImport = model.EntityContainer
                .FindOperationImports("ResetDataSource").FirstOrDefault() as IEdmActionImport;
            Assert.NotNull(actionImport); // guard

            // Act
            string name = context.CreatePathItemName(actionImport);

            // Assert
            Assert.Equal("/ResetDataSource", name);
        }

        [Theory]
        [InlineData(EdmContainerElementKind.ActionImport, "ResetDataSource", "/ResetDataSource")]
        [InlineData(EdmContainerElementKind.FunctionImport, "GetPersonWithMostFriends", "/GetPersonWithMostFriends()")]
        [InlineData(EdmContainerElementKind.FunctionImport, "GetNearestAirport", "/GetNearestAirport(lat={lat},lon={lon})")]
        public void CreatePathItemNameForOperationImportReturnCorrectName(
            EdmContainerElementKind elementKind, string operationName, string expect)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            ODataContext context = new ODataContext(model);
            IEdmOperationImport operationImport = model.EntityContainer
                .FindOperationImports(operationName).FirstOrDefault();
            Assert.NotNull(operationImport); // guard
            Assert.Equal(elementKind, operationImport.ContainerElementKind);

            // Act
            string name = context.CreatePathItemName(operationImport);

            // Assert
            Assert.Equal(expect, name);
        }
        #endregion

        #region Operation PathItem Name
        [Fact]
        public void CreatePathItemNameForOperationThrowArgumentNullContext()
        {
            // Arrange
            ODataContext context = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context", () => context.CreatePathItemName(operation: null));
        }

        [Fact]
        public void CreatePathItemNameForOperationThrowArgumentNullOperation()
        {
            // Arrange
            ODataContext context = new ODataContext(EdmModelHelper.EmptyModel);

            // Act & Assert
            Assert.Throws<ArgumentNullException>("operation", () => context.CreatePathItemName(operation: null));
        }

        [Theory]
        [InlineData(true, EdmSchemaElementKind.Action, "ShareTrip", "/ShareTrip")]
        [InlineData(false, EdmSchemaElementKind.Action, "ShareTrip",
            "/Microsoft.OData.Service.Sample.TrippinInMemory.Models.ShareTrip")]
        [InlineData(true, EdmSchemaElementKind.Function, "GetFriendsTrips", "/GetFriendsTrips(userName={userName})")]
        [InlineData(false, EdmSchemaElementKind.Function, "GetFriendsTrips",
            "/Microsoft.OData.Service.Sample.TrippinInMemory.Models.GetFriendsTrips(userName={userName})")]
        [InlineData(true, EdmSchemaElementKind.Function, "GetNearestAirport", "/GetNearestAirport(lat={lat},lon={lon})")]
        [InlineData(false, EdmSchemaElementKind.Function, "GetNearestAirport",
            "/Microsoft.OData.Service.Sample.TrippinInMemory.Models.GetNearestAirport(lat={lat},lon={lon})")]
        public void CreatePathItemNameForOperationReturnCorrectName(bool unqualifiedCall,
            EdmSchemaElementKind elementKind, string operationName, string expect)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                UnqualifiedCall = unqualifiedCall
            };

            ODataContext context = new ODataContext(model, settings);
            IEdmOperation operation = model.SchemaElements.OfType<IEdmOperation>()
                .First(s => s.Name == operationName);
            Assert.NotNull(operation); // guard
            Assert.Equal(elementKind, operation.SchemaElementKind);

            // Act
            string name = context.CreatePathItemName(operation);

            // Assert
            Assert.Equal(expect, name);
        }
        #endregion
    }
}
