// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Exceptions;
using Microsoft.OpenApi.OData.Tests;
using Xunit;

namespace Microsoft.OpenApi.OData.Generator.Tests
{
    public class OpenApiOperationGeneratorTest
    {
        #region EntitySet Operation
        [Fact]
        public void CreateEntitySetGetOperationThrowArgumentNullContext()
        {
            // Arrange
            ODataContext context = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context", () => context.CreateEntitySetGetOperation(entitySet: null));
        }

        [Fact]
        public void CreateEntitySetGetOperationThrowArgumentNullEntitySet()
        {
            // Arrange
            ODataContext context = new ODataContext(EdmModelHelper.EmptyModel);

            // Act & Assert
            Assert.Throws<ArgumentNullException>("entitySet", () => context.CreateEntitySetGetOperation(entitySet: null));
        }

        [Fact]
        public void CreateEntitySetGetOperationReturnsCorrectOperation()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.BasicEdmModel;
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("People");
            ODataContext context = new ODataContext(model);

            // Act
            var get = context.CreateEntitySetGetOperation(entitySet);

            // Assert
            Assert.NotNull(get);
            Assert.Equal("Get entities from " + entitySet.Name, get.Summary);
            Assert.NotNull(get.Tags);
            var tag = Assert.Single(get.Tags);
            Assert.Equal(entitySet.Name, tag.Name);

            Assert.NotNull(get.Parameters);
            Assert.Equal(8, get.Parameters.Count);

            Assert.NotNull(get.Responses);
            Assert.Equal(2, get.Responses.Count);
        }

        [Fact]
        public void CreateEntitySetPostOperationThrowArgumentNullContext()
        {
            // Arrange
            ODataContext context = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context", () => context.CreateEntitySetPostOperation(entitySet: null));
        }

        [Fact]
        public void CreateEntitySetPostOperationThrowArgumentNullEntitySet()
        {
            // Arrange
            ODataContext context = new ODataContext(EdmModelHelper.EmptyModel);

            // Act & Assert
            Assert.Throws<ArgumentNullException>("entitySet", () => context.CreateEntitySetPostOperation(entitySet: null));
        }

        [Fact]
        public void CreateEntitySetPostOperationReturnsCorrectOperation()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.BasicEdmModel;
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("People");
            ODataContext context = new ODataContext(model);

            // Act
            var post = context.CreateEntitySetPostOperation(entitySet);

            // Assert
            Assert.NotNull(post);
            Assert.Equal("Add new entity to " + entitySet.Name, post.Summary);
            Assert.NotNull(post.Tags);
            var tag = Assert.Single(post.Tags);
            Assert.Equal(entitySet.Name, tag.Name);

            Assert.Empty(post.Parameters);
            Assert.NotNull(post.RequestBody);

            Assert.NotNull(post.Responses);
            Assert.Equal(2, post.Responses.Count);
        }

        [Fact]
        public void CreateEntityGetOperationThrowArgumentNullContext()
        {
            // Arrange
            ODataContext context = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context", () => context.CreateEntityGetOperation(entitySet: null));
        }

        [Fact]
        public void CreateEntityGetOperationThrowArgumentNullEntitySet()
        {
            // Arrange
            ODataContext context = new ODataContext(EdmModelHelper.EmptyModel);

            // Act & Assert
            Assert.Throws<ArgumentNullException>("entitySet", () => context.CreateEntityGetOperation(entitySet: null));
        }

        [Fact]
        public void CreateEntityGetOperationReturnsCorrectOperation()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.BasicEdmModel;
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("People");
            ODataContext context = new ODataContext(model);

            // Act
            var get = context.CreateEntityGetOperation(entitySet);

            // Assert
            Assert.NotNull(get);
            Assert.Equal("Get entity from People by key", get.Summary);
            Assert.NotNull(get.Tags);
            var tag = Assert.Single(get.Tags);
            Assert.Equal("People", tag.Name);

            Assert.NotNull(get.Parameters);
            Assert.Equal(3, get.Parameters.Count);
            Assert.Null(get.RequestBody);

            Assert.NotNull(get.Responses);
            Assert.Equal(2, get.Responses.Count);
            Assert.Equal(new[] { "200", "default" }, get.Responses.Select(r => r.Key));
        }
        #endregion

        #region Entity Operation

        [Fact]
        public void CreateEntityPatchOperationThrowArgumentNullContext()
        {
            // Arrange
            ODataContext context = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context", () => context.CreateEntityPatchOperation(entitySet: null));
        }

        [Fact]
        public void CreateEntityPatchOperationThrowArgumentNullEntitySet()
        {
            // Arrange
            ODataContext context = new ODataContext(EdmModelHelper.EmptyModel);

            // Act & Assert
            Assert.Throws<ArgumentNullException>("entitySet", () => context.CreateEntityPatchOperation(entitySet: null));
        }

        [Fact]
        public void CreateEntityPatchOperationReturnsCorrectOperation()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.BasicEdmModel;
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("People");
            ODataContext context = new ODataContext(model);

            // Act
            var patch = context.CreateEntityPatchOperation(entitySet);

            // Assert
            Assert.NotNull(patch);
            Assert.Equal("Update entity in People", patch.Summary);
            Assert.NotNull(patch.Tags);
            var tag = Assert.Single(patch.Tags);
            Assert.Equal("People", tag.Name);

            Assert.NotNull(patch.Parameters);
            Assert.Equal(1, patch.Parameters.Count);

            Assert.NotNull(patch.RequestBody);

            Assert.NotNull(patch.Responses);
            Assert.Equal(2, patch.Responses.Count);
            Assert.Equal(new[] { "204", "default" }, patch.Responses.Select(r => r.Key));
        }

        [Fact]
        public void CreateEntityDeleteOperationThrowArgumentNullContext()
        {
            // Arrange
            ODataContext context = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context", () => context.CreateEntityDeleteOperation(entitySet: null));
        }

        [Fact]
        public void CreateEntityDeleteOperationThrowArgumentNullEntitySet()
        {
            // Arrange
            ODataContext context = new ODataContext(EdmModelHelper.EmptyModel);

            // Act & Assert
            Assert.Throws<ArgumentNullException>("entitySet", () => context.CreateEntityDeleteOperation(entitySet: null));
        }

        [Fact]
        public void CreateEntityDeleteOperationReturnsCorrectOperation()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.BasicEdmModel;
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("People");
            ODataContext context = new ODataContext(model);

            // Act
            var delete = context.CreateEntityDeleteOperation(entitySet);

            // Assert
            Assert.NotNull(delete);
            Assert.Equal("Delete entity from People", delete.Summary);
            Assert.NotNull(delete.Tags);
            var tag = Assert.Single(delete.Tags);
            Assert.Equal("People", tag.Name);

            Assert.NotNull(delete.Parameters);
            Assert.Equal(2, delete.Parameters.Count);

            Assert.Null(delete.RequestBody);

            Assert.NotNull(delete.Responses);
            Assert.Equal(2, delete.Responses.Count);
            Assert.Equal(new[] { "204", "default" }, delete.Responses.Select(r => r.Key));
        }
        #endregion

        #region Singleton
        [Fact]
        public void CreateSingletonGetOperationThrowArgumentNullContext()
        {
            // Arrange
            ODataContext context = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context", () => context.CreateSingletonGetOperation(singleton: null));
        }

        [Fact]
        public void CreateSingletonGetOperationThrowArgumentNullSingleton()
        {
            // Arrange
            ODataContext context = new ODataContext(EdmModelHelper.EmptyModel);

            // Act & Assert
            Assert.Throws<ArgumentNullException>("singleton", () => context.CreateSingletonGetOperation(singleton: null));
        }

        [Fact]
        public void CreateSingletonGetOperationReturnsCorrectOperation()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.BasicEdmModel;
            IEdmSingleton singleton = model.EntityContainer.FindSingleton("Me");
            ODataContext context = new ODataContext(model);

            // Act
            var get = context.CreateSingletonGetOperation(singleton);

            // Assert
            Assert.NotNull(get);
            Assert.Equal("Get Me", get.Summary);
            Assert.NotNull(get.Tags);
            var tag = Assert.Single(get.Tags);
            Assert.Equal("Me", tag.Name);

            Assert.NotNull(get.Parameters);
            Assert.Equal(2, get.Parameters.Count);

            Assert.Null(get.RequestBody);

            Assert.NotNull(get.Responses);
            Assert.Equal(2, get.Responses.Count);
            Assert.Equal(new[] { "200", "default" }, get.Responses.Select(r => r.Key));
        }

        [Fact]
        public void CreateSingletonPatchOperationThrowArgumentNullContext()
        {
            // Arrange
            ODataContext context = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context", () => context.CreateSingletonPatchOperation(singleton: null));
        }

        [Fact]
        public void CreateSingletonPatchOperationThrowArgumentNullSingleton()
        {
            // Arrange
            ODataContext context = new ODataContext(EdmModelHelper.EmptyModel);

            // Act & Assert
            Assert.Throws<ArgumentNullException>("singleton", () => context.CreateSingletonPatchOperation(singleton: null));
        }

        [Fact]
        public void CreateSingletonPatchOperationReturnsCorrectOperation()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.BasicEdmModel;
            IEdmSingleton singleton = model.EntityContainer.FindSingleton("Me");
            ODataContext context = new ODataContext(model);

            // Act
            var patch = context.CreateSingletonPatchOperation(singleton);

            // Assert
            Assert.NotNull(patch);
            Assert.Equal("Update Me", patch.Summary);
            Assert.NotNull(patch.Tags);
            var tag = Assert.Single(patch.Tags);
            Assert.Equal("Me", tag.Name);

            Assert.Empty(patch.Parameters);
            Assert.NotNull(patch.RequestBody);

            Assert.NotNull(patch.Responses);
            Assert.Equal(2, patch.Responses.Count);
            Assert.Equal(new[] { "204", "default" }, patch.Responses.Select(r => r.Key));
        }
        #endregion

        #region EdmNavigationProperty
        [Fact]
        public void CreateNavigationGetOperationThrowArgumentNullContext()
        {
            // Arrange
            ODataContext context = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context",
                () => context.CreateNavigationGetOperation(navigationSource: null, property: null));
        }

        [Fact]
        public void CreateNavigationGetOperationThrowArgumentNullNavigationSource()
        {
            // Arrange
            ODataContext context = new ODataContext(EdmModelHelper.EmptyModel);

            // Act & Assert
            Assert.Throws<ArgumentNullException>("navigationSource",
                () => context.CreateNavigationGetOperation(navigationSource: null, property: null));
        }

        [Fact]
        public void CreateNavigationGetOperationThrowArgumentNullProperty()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.BasicEdmModel;
            ODataContext context = new ODataContext(model);
            IEdmEntitySet people = model.EntityContainer.FindEntitySet("People");
            Assert.NotNull(people);

            // Act & Assert
            Assert.Throws<ArgumentNullException>("property",
                () => context.CreateNavigationGetOperation(people, property: null));
        }

        [Fact]
        public void CreateNavigationGetOperationReturnsCorrectOperation()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            ODataContext context = new ODataContext(model);
            IEdmEntitySet people = model.EntityContainer.FindEntitySet("People");
            Assert.NotNull(people);

            IEdmEntityType person = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Person");
            IEdmNavigationProperty navProperty = person.DeclaredNavigationProperties().First(c => c.Name == "Trips");

            // Act
            var operation = context.CreateNavigationGetOperation(people, navProperty);

            // Assert
            Assert.NotNull(operation);
            Assert.Equal("Get Trips from People", operation.Summary);
            Assert.NotNull(operation.Tags);
            var tag = Assert.Single(operation.Tags);
            Assert.Equal("People", tag.Name);

            Assert.NotNull(operation.Parameters);
            Assert.Equal(9, operation.Parameters.Count);

            Assert.Null(operation.RequestBody);

            Assert.Equal(2, operation.Responses.Count);
            Assert.Equal(new string[] { "200", "default" }, operation.Responses.Select(e => e.Key));
        }

        [Fact]
        public void CreateNavigationPatchOperationThrowArgumentNullContext()
        {
            // Arrange
            ODataContext context = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context",
                () => context.CreateNavigationPatchOperation(navigationSource: null, property: null));
        }

        [Fact]
        public void CreateNavigationPatchOperationThrowArgumentNullNavigationSource()
        {
            // Arrange
            ODataContext context = new ODataContext(EdmModelHelper.EmptyModel);

            // Act & Assert
            Assert.Throws<ArgumentNullException>("navigationSource",
                () => context.CreateNavigationPatchOperation(navigationSource: null, property: null));
        }

        [Fact]
        public void CreateNavigationPatchOperationThrowArgumentNullProperty()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.BasicEdmModel;
            ODataContext context = new ODataContext(model);
            IEdmEntitySet people = model.EntityContainer.FindEntitySet("People");
            Assert.NotNull(people);

            // Act & Assert
            Assert.Throws<ArgumentNullException>("property",
                () => context.CreateNavigationPatchOperation(people, property: null));
        }

        [Fact]
        public void CreateNavigationPatchOperationThrowExceptionForCollectionNavigationProperty()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            ODataContext context = new ODataContext(model);
            IEdmEntitySet people = model.EntityContainer.FindEntitySet("People");
            Assert.NotNull(people);

            IEdmEntityType person = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Person");
            IEdmNavigationProperty navProperty = person.DeclaredNavigationProperties().First(c => c.Name == "Trips");

            // Act & Assert
            var exception = Assert.Throws<OpenApiException>(() => context.CreateNavigationPatchOperation(people, navProperty));
            Assert.Equal("It is not valid to update any collection valued navigation property 'Trips'.", exception.Message);
        }

        [Fact]
        public void CreateNavigationPatchOperationReturnsCorrectOperation()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            ODataContext context = new ODataContext(model);
            IEdmEntitySet people = model.EntityContainer.FindEntitySet("People");
            Assert.NotNull(people);

            IEdmEntityType person = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Person");
            IEdmNavigationProperty navProperty = person.DeclaredNavigationProperties().First(c => c.Name == "BestFriend");

            // Act
            var operation = context.CreateNavigationPatchOperation(people, navProperty);

            // Assert
            Assert.NotNull(operation);
            Assert.Equal("Update the navigation property BestFriend in People", operation.Summary);
            Assert.NotNull(operation.Tags);
            var tag = Assert.Single(operation.Tags);
            Assert.Equal("People", tag.Name);

            Assert.NotNull(operation.Parameters);
            Assert.Equal(1, operation.Parameters.Count);

            Assert.NotNull(operation.RequestBody);
            Assert.Equal("New navigation property values", operation.RequestBody.Description);

            Assert.Equal(2, operation.Responses.Count);
            Assert.Equal(new string[] { "204", "default" }, operation.Responses.Select(e => e.Key));
        }

        [Fact]
        public void CreateNavigationPostOperationThrowArgumentNullContext()
        {
            // Arrange
            ODataContext context = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context",
                () => context.CreateNavigationPostOperation(navigationSource: null, property: null));
        }

        [Fact]
        public void CreateNavigationPostOperationThrowArgumentNullNavigationSource()
        {
            // Arrange
            ODataContext context = new ODataContext(EdmModelHelper.EmptyModel);

            // Act & Assert
            Assert.Throws<ArgumentNullException>("navigationSource",
                () => context.CreateNavigationPostOperation(navigationSource: null, property: null));
        }

        [Fact]
        public void CreateNavigationPostOperationThrowArgumentNullProperty()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.BasicEdmModel;
            ODataContext context = new ODataContext(model);
            IEdmEntitySet people = model.EntityContainer.FindEntitySet("People");
            Assert.NotNull(people);

            // Act & Assert
            Assert.Throws<ArgumentNullException>("property",
                () => context.CreateNavigationPostOperation(people, property: null));
        }

        [Fact]
        public void CreateNavigationPostOperationThrowExceptionForNonCollectionNavigationProperty()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            ODataContext context = new ODataContext(model);
            IEdmEntitySet people = model.EntityContainer.FindEntitySet("People");
            Assert.NotNull(people);

            IEdmEntityType person = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Person");
            IEdmNavigationProperty navProperty = person.DeclaredNavigationProperties().First(c => c.Name == "BestFriend");

            // Act & Assert
            var exception = Assert.Throws<OpenApiException>(() => context.CreateNavigationPostOperation(people, navProperty));
            Assert.Equal("It is not valid to Post to any non-collection valued navigation property 'BestFriend'.", exception.Message);
        }

        [Fact]
        public void CreateNavigationPostOperationReturnsCorrectOperation()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            ODataContext context = new ODataContext(model);
            IEdmEntitySet people = model.EntityContainer.FindEntitySet("People");
            Assert.NotNull(people);

            IEdmEntityType person = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Person");
            IEdmNavigationProperty navProperty = person.DeclaredNavigationProperties().First(c => c.Name == "Trips");

            // Act
            var operation = context.CreateNavigationPostOperation(people, navProperty);

            // Assert
            Assert.NotNull(operation);
            Assert.Equal("Add new navigation property to Trips for People", operation.Summary);
            Assert.NotNull(operation.Tags);
            var tag = Assert.Single(operation.Tags);
            Assert.Equal("People", tag.Name);

            Assert.NotNull(operation.Parameters);
            Assert.Empty(operation.Parameters);

            Assert.NotNull(operation.RequestBody);
            Assert.Equal("New navigation property", operation.RequestBody.Description);

            Assert.Equal(2, operation.Responses.Count);
            Assert.Equal(new string[] { "201", "default" }, operation.Responses.Select(e => e.Key));
        }
        #endregion

        #region EdmOperation
        [Fact]
        public void CreateOperationForEdmOperationThrowArgumentNullContext()
        {
            // Arrange
            ODataContext context = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context",
                () => context.CreateOperation(navigationSource: null, edmOperation: null));
        }

        [Fact]
        public void CreateOperationForEdmOperationThrowArgumentNullNavigationSource()
        {
            // Arrange
            ODataContext context = new ODataContext(EdmModelHelper.EmptyModel);

            // Act & Assert
            Assert.Throws<ArgumentNullException>("navigationSource",
                () => context.CreateOperation(navigationSource: null, edmOperation: null));
        }

        [Fact]
        public void CreateOperationForEdmOperationThrowArgumentNullEdmOperation()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.BasicEdmModel;
            ODataContext context = new ODataContext(model);
            IEdmEntitySet people = model.EntityContainer.FindEntitySet("People");
            Assert.NotNull(people);

            // Act & Assert
            Assert.Throws<ArgumentNullException>("edmOperation",
                () => context.CreateOperation(people, edmOperation: null));
        }

        [Fact]
        public void CreateOperationForEdmFunctionReturnsCorrectOperation()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            ODataContext context = new ODataContext(model);
            IEdmEntitySet people = model.EntityContainer.FindEntitySet("People");
            Assert.NotNull(people);

            IEdmFunction function = model.SchemaElements.OfType<IEdmFunction>().First(f => f.Name == "GetFavoriteAirline");
            Assert.NotNull(function);

            // Act
            var operation = context.CreateOperation(people, function);

            // Assert
            Assert.NotNull(operation);
            Assert.Equal("Invoke function GetFavoriteAirline", operation.Summary);
            Assert.NotNull(operation.Tags);
            var tag = Assert.Single(operation.Tags);
            Assert.Equal("People", tag.Name);

            Assert.NotNull(operation.Parameters);
            Assert.Equal(1, operation.Parameters.Count);
            Assert.Equal(new string[] { "UserName" }, operation.Parameters.Select(p => p.Name));

            Assert.Null(operation.RequestBody);

            Assert.Equal(2, operation.Responses.Count);
            Assert.Equal(new string[] { "200", "default" }, operation.Responses.Select(e => e.Key));
        }

        [Fact]
        public void CreateOperationForEdmActionReturnsCorrectOperation()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            ODataContext context = new ODataContext(model);
            IEdmEntitySet people = model.EntityContainer.FindEntitySet("People");
            Assert.NotNull(people);

            IEdmAction action = model.SchemaElements.OfType<IEdmAction>().First(f => f.Name == "ShareTrip");
            Assert.NotNull(action);

            // Act
            var operation = context.CreateOperation(people, action);

            // Assert
            Assert.NotNull(operation);
            Assert.Equal("Invoke action ShareTrip", operation.Summary);
            Assert.NotNull(operation.Tags);
            var tag = Assert.Single(operation.Tags);
            Assert.Equal("People", tag.Name);

            Assert.NotNull(operation.Parameters);
            Assert.Equal(1, operation.Parameters.Count);
            Assert.Equal(new string[] { "UserName" }, operation.Parameters.Select(p => p.Name));

            Assert.NotNull(operation.RequestBody);
            Assert.Equal("Action parameters", operation.RequestBody.Description);

            Assert.Equal(2, operation.Responses.Count);
            Assert.Equal(new string[] { "204", "default" }, operation.Responses.Select(e => e.Key));
        }
        #endregion

        #region EdmOperationImport
        [Fact]
        public void CreateOperationForEdmOperationImportThrowArgumentNullContext()
        {
            // Arrange
            ODataContext context = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context",
                () => context.CreateOperation(operationImport: null));
        }

        [Fact]
        public void CreateOperationForEdmOperationImportThrowArgumentNullOperationImport()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.BasicEdmModel;
            ODataContext context = new ODataContext(model);

            // Act & Assert
            Assert.Throws<ArgumentNullException>("operationImport", () => context.CreateOperation(operationImport: null));
        }

        [Fact]
        public void CreateOperationForEdmFunctionImportReturnsCorrectOperation()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            ODataContext context = new ODataContext(model);
            var functionImport = model.EntityContainer.FindOperationImports("GetPersonWithMostFriends").FirstOrDefault();
            Assert.NotNull(functionImport);

            // Act
            var operation = context.CreateOperation(functionImport);

            // Assert
            Assert.NotNull(operation);
            Assert.Equal("Invoke function GetPersonWithMostFriends", operation.Summary);
            Assert.NotNull(operation.Tags);
            var tag = Assert.Single(operation.Tags);
            Assert.Equal("People", tag.Name);

            Assert.NotNull(operation.Parameters);
            Assert.Empty(operation.Parameters);

            Assert.Null(operation.RequestBody);

            Assert.Equal(2, operation.Responses.Count);
            Assert.Equal(new string[] { "200", "default" }, operation.Responses.Select(e => e.Key));
        }

        [Fact]
        public void CreateOperationForEdmActionImportReturnsCorrectOperation()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            ODataContext context = new ODataContext(model);

            var actionImport = model.EntityContainer.FindOperationImports("ResetDataSource").FirstOrDefault();
            Assert.NotNull(actionImport);

            // Act
            var operation = context.CreateOperation(actionImport);

            // Assert
            Assert.NotNull(operation);
            Assert.Equal("Invoke action ResetDataSource", operation.Summary);
            Assert.Null(operation.Tags);

            Assert.NotNull(operation.Parameters);
            Assert.Empty(operation.Parameters);

            Assert.Null(operation.RequestBody);

            Assert.Equal(2, operation.Responses.Count);
            Assert.Equal(new string[] { "204", "default" }, operation.Responses.Select(e => e.Key));
        }
        #endregion
    }
}
