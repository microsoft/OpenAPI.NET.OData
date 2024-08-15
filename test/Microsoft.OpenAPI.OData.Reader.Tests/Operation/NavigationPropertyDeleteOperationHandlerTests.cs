// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Tests;
using System.Linq;
using Xunit;

namespace Microsoft.OpenApi.OData.Operation.Tests
{
    public class NavigationPropertyDeleteOperationHandlerTests
    {
        private NavigationPropertyDeleteOperationHandler _operationHandler = new NavigationPropertyDeleteOperationHandler();

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, false)]
        [InlineData(true, true)]
        [InlineData(false, true)]
        public void CreateNavigationDeleteOperationReturnsCorrectOperation(bool enableOperationId, bool useSuccessStatusCodeRange)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableOperationId = enableOperationId,
                UseSuccessStatusCodeRange = useSuccessStatusCodeRange
            };
            ODataContext context = new ODataContext(model, settings);
            IEdmEntitySet people = model.EntityContainer.FindEntitySet("People");
            Assert.NotNull(people);

            IEdmEntityType person = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Person");
            IEdmNavigationProperty navProperty = person.DeclaredNavigationProperties().First(c => c.Name == "Trips");
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(people), new ODataKeySegment(people.EntityType), new ODataNavigationPropertySegment(navProperty));

            // Act
            var operation = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(operation);
            Assert.Equal("Delete a trip.", operation.Summary);
            Assert.Equal("Delete an instance of a trip.", operation.Description);
            Assert.NotNull(operation.Tags);
            var tag = Assert.Single(operation.Tags);
            Assert.Equal("People.Trip", tag.Name);

            Assert.NotNull(operation.Parameters);
            Assert.NotEmpty(operation.Parameters);

            Assert.Null(operation.RequestBody);

            Assert.Equal(2, operation.Responses.Count);
            Assert.Equal(new string[] { "204", "default" }, operation.Responses.Select(e => e.Key));

            if (enableOperationId)
            {
                Assert.Equal("People.DeleteTrips", operation.OperationId);
            }
            else
            {
                Assert.Null(operation.OperationId);
            }
        }

        [Fact]
        public void CreateNavigationDeleteOperationWithTargetPathAnnotationsReturnsCorrectOperation()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            ODataContext context = new(model, new OpenApiConvertSettings());
            IEdmEntitySet people = model.EntityContainer.FindEntitySet("People");
            Assert.NotNull(people);

            IEdmEntityType person = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Person");
            IEdmNavigationProperty navProperty = person.DeclaredNavigationProperties().First(c => c.Name == "Friends");
            ODataPath path = new(new ODataNavigationSourceSegment(people), new ODataKeySegment(people.EntityType), new ODataNavigationPropertySegment(navProperty));

            // Act
            var operation = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(operation);
            Assert.Equal("Delete a friend.", operation.Summary);
            Assert.Equal("Delete an instance of a friend relationship.", operation.Description);
            
            Assert.NotNull(operation.ExternalDocs);
            Assert.Equal("Find more info here", operation.ExternalDocs.Description);
            Assert.Equal("https://learn.microsoft.com/graph/api/person-delete-friend?view=graph-rest-1.0", operation.ExternalDocs.Url.ToString());
        }
    }
}
