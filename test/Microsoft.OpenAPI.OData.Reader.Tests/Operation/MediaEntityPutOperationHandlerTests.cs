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
            Assert.NotNull(todos);

            IEdmEntityType todo = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Todo");
            IEdmStructuralProperty sp = todo.DeclaredStructuralProperties().First(c => c.Name == "Logo");
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(todos),
                new ODataKeySegment(todos.EntityType()),
                new ODataStreamPropertySegment(sp.Name));

            // Act
            var getOperation = _operationalHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(getOperation);
            Assert.Equal("Update media content for Todo in Todos", getOperation.Summary);
            Assert.NotNull(getOperation.Tags);
            var tag = Assert.Single(getOperation.Tags);
            Assert.Equal("Todos.Todo", tag.Name);

            Assert.NotNull(getOperation.Responses);
            Assert.Equal(2, getOperation.Responses.Count);
            Assert.Equal(new[] { "204", "default" }, getOperation.Responses.Select(r => r.Key));

            if (enableOperationId)
            {
                Assert.Equal("Todos.Todo.UpdateLogo", getOperation.OperationId);
            }
            else
            {
                Assert.Null(getOperation.OperationId);
            }
        }
    }
}
