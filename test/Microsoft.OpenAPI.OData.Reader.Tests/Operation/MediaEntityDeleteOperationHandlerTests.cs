// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Vocabulary.Core;
using System.Linq;
using Xunit;

namespace Microsoft.OpenApi.OData.Operation.Tests
{
    public class MediaEntityDeleteOperationHandlerTests
    {
        private readonly MediaEntityDeleteOperationHandler _operationalHandler = new MediaEntityDeleteOperationHandler();

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, false)]
        [InlineData(true, true)]
        [InlineData(false, true)]
        public void CreateMediaEntityDeleteOperationReturnsCorrectOperation(bool enableOperationId, bool useSuccessStatusCodeRange)
        {
            // Arrange
            IEdmModel model = OData.Tests.EdmModelHelper.TripServiceModel;
            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableOperationId = enableOperationId,
                UseSuccessStatusCodeRange = useSuccessStatusCodeRange
            };
            ODataContext context = new(model, settings);
            IEdmEntitySet people = model.EntityContainer.FindEntitySet("People");
            Assert.NotNull(people);

            IEdmEntityType person = people.EntityType();
            IEdmStructuralProperty property = person.StructuralProperties().First(c => c.Name == "Photo");
            ODataPath path = new(new ODataNavigationSourceSegment(people),
                new ODataKeySegment(person),
                new ODataStreamPropertySegment(property.Name));

            // Act
            var operation = _operationalHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(operation);
            Assert.Equal("Delete photo", operation.Summary);
            Assert.Equal("Delete photo of a specific user", operation.Description);

            Assert.NotNull(operation.ExternalDocs);
            Assert.Equal("Find more info here", operation.ExternalDocs.Description);
            Assert.Equal("https://learn.microsoft.com/graph/api/person-delete-photo?view=graph-rest-1.0", operation.ExternalDocs.Url.ToString());

            Assert.NotNull(operation.Tags);
            var tag = Assert.Single(operation.Tags);
            Assert.Equal("People.Person", tag.Name);

            Assert.NotNull(operation.Parameters);
            Assert.NotEmpty(operation.Parameters);

            Assert.Null(operation.RequestBody);

            Assert.Equal(2, operation.Responses.Count);
            Assert.Equal(["204", "default"], operation.Responses.Select(e => e.Key));

            if (enableOperationId)
            {
                Assert.Equal("People.Person.DeletePhoto", operation.OperationId);
            }
            else
            {
                Assert.Null(operation.OperationId);
            }
        }
    }
}
