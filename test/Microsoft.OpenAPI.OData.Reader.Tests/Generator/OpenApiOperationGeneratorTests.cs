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
    public class OpenApiOperationGeneratorTest
    {
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

            Assert.Null(post.Parameters);
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

            Assert.Null(patch.Parameters);
            Assert.NotNull(patch.RequestBody);

            Assert.NotNull(patch.Responses);
            Assert.Equal(2, patch.Responses.Count);
            Assert.Equal(new[] { "204", "default" }, patch.Responses.Select(r => r.Key));
        }
    }
}
