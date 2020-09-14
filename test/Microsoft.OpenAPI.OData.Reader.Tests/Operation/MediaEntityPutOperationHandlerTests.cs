// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Edm;
using System.Linq;
using Xunit;

namespace Microsoft.OpenApi.OData.Operation.Tests
{
    public class MediaEntityPutOperationHandlerTests
    {
        private readonly MediaEntityPutOperationHandler _operationalHandler = new MediaEntityPutOperationHandler();

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateEntityPutOperationReturnsCorrectOperation(bool enableOperationId)
        {
            // Arrange
            IEdmModel model = MediaEntityGetOperationHandlerTests.GetEdmModel();
            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableOperationId = enableOperationId
            };

            ODataContext context = new ODataContext(model, settings);
            IEdmEntitySet todos = model.EntityContainer.FindEntitySet("Todos");
            IEdmSingleton me = model.EntityContainer.FindSingleton("me");
            Assert.NotNull(todos);

            IEdmEntityType todo = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Todo");
            IEdmStructuralProperty sp = todo.DeclaredStructuralProperties().First(c => c.Name == "Logo");
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(todos),
                new ODataKeySegment(todos.EntityType()),
                new ODataStreamPropertySegment(sp.Name));

            IEdmEntityType user = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "user");
            IEdmNavigationProperty navProperty = user.DeclaredNavigationProperties().First(c => c.Name == "photo");
            ODataPath path2 = new ODataPath(new ODataNavigationSourceSegment(me),
                new ODataNavigationPropertySegment(navProperty),
                new ODataStreamContentSegment());

            // Act
            var getOperation = _operationalHandler.CreateOperation(context, path);
            var getOperation2 = _operationalHandler.CreateOperation(context, path2);

            // Assert
            Assert.NotNull(getOperation);
            Assert.NotNull(getOperation2);
            Assert.Equal("Update media content for Todo in Todos", getOperation.Summary);
            Assert.Equal("Update media content for the navigation property photo in me", getOperation2.Summary);
            Assert.NotNull(getOperation.Tags);
            Assert.NotNull(getOperation2.Tags);

            var tag = Assert.Single(getOperation.Tags);
            var tag2 = Assert.Single(getOperation2.Tags);
            Assert.Equal("Todos.Todo", tag.Name);
            Assert.Equal("me.profilePhoto", tag2.Name);

            Assert.NotNull(getOperation.Responses);
            Assert.NotNull(getOperation2.Responses);
            Assert.Equal(2, getOperation.Responses.Count);
            Assert.Equal(2, getOperation2.Responses.Count);
            Assert.Equal(new[] { "204", "default" }, getOperation.Responses.Select(r => r.Key));
            Assert.Equal(new[] { "204", "default" }, getOperation2.Responses.Select(r => r.Key));

            if (enableOperationId)
            {
                Assert.Equal("Todos.Todo.UpdateLogo", getOperation.OperationId);
                Assert.Equal("me.photo.UpdateContent", getOperation2.OperationId);
            }
            else
            {
                Assert.Null(getOperation.OperationId);
                Assert.Null(getOperation2.OperationId);
            }
        }
    }
}
