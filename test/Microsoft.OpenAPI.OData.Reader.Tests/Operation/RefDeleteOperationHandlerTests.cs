// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Generator;
using Microsoft.OpenApi.OData.Tests;
using Xunit;

namespace Microsoft.OpenApi.OData.Operation.Tests
{
    public class RefDeleteOperationHandlerTests
    {
        public RefDeleteOperationHandlerTests()
        {
          _operationHandler = new (_openApiDocument);
        }
        private readonly OpenApiDocument _openApiDocument = new();
        private readonly RefDeleteOperationHandler _operationHandler;

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateNavigationRefDeleteOperationReturnsCorrectOperation(bool enableOperationId)
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
                new ODataKeySegment(people.EntityType),
                new ODataNavigationPropertySegment(navProperty),
                ODataRefSegment.Instance);

            // Act
            var operation = _operationHandler.CreateOperation(context, path);
            _openApiDocument.Tags = context.CreateTags();

            // Assert
            Assert.NotNull(operation);
            Assert.Equal("Delete a trip.", operation.Summary);
            Assert.Equal("Delete an instance of a trip.", operation.Description);
            Assert.NotNull(operation.Tags);
            var tag = Assert.Single(operation.Tags);
            Assert.Equal("People.Trip", tag.Name);

            Assert.NotNull(operation.Parameters);
            Assert.NotEmpty(operation.Parameters);
            Assert.Equal(3, operation.Parameters.Count);

            Assert.Null(operation.RequestBody);

            Assert.Equal(2, operation.Responses.Count);
            Assert.Equal(new string[] { "204", "default" }, operation.Responses.Select(e => e.Key));

            if (enableOperationId)
            {
                Assert.Equal("People.DeleteRefTrips", operation.OperationId);
            }
            else
            {
                Assert.Null(operation.OperationId);
            }
        }
    }
}
