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
    public class RefPostOperationHandlerTests
    {
        private RefPostOperationHandler _operationHandler = new RefPostOperationHandler();

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateNavigationRefPostOperationReturnsCorrectOperation(bool enableOperationId)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableOperationId = enableOperationId
            };
            ODataContext context = new ODataContext(model, settings);
            IEdmEntitySet people = model.EntityContainer.FindEntitySet("People");
            Assert.NotNull(people);

            IEdmEntityType person = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Person");
            IEdmNavigationProperty navProperty = person.DeclaredNavigationProperties().First(c => c.Name == "Trips");
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(people),
                new ODataKeySegment(people.EntityType()),
                new ODataNavigationPropertySegment(navProperty),
                ODataRefSegment.Instance);

            // Act
            var operation = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(operation);
            Assert.Equal("Create new navigation property ref to Trips for People", operation.Summary);
            Assert.NotNull(operation.Tags);
            var tag = Assert.Single(operation.Tags);
            Assert.Equal("People.Trip", tag.Name);

            Assert.NotNull(operation.Parameters);
            Assert.NotEmpty(operation.Parameters);

            Assert.NotNull(operation.RequestBody);
            Assert.Equal("New navigation property ref value", operation.RequestBody.Description);

            Assert.Equal(2, operation.Responses.Count);
            Assert.Equal(new string[] { "201", "default" }, operation.Responses.Select(e => e.Key));

            if (enableOperationId)
            {
                Assert.Equal("People.CreateRefTrips", operation.OperationId);
            }
            else
            {
                Assert.Null(operation.OperationId);
            }
        }
    }
}
