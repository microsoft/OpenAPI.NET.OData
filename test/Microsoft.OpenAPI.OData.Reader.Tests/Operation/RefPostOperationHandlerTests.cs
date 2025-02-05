// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.References;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Generator;
using Microsoft.OpenApi.OData.Tests;
using System.Linq;
using Xunit;

namespace Microsoft.OpenApi.OData.Operation.Tests
{
    public class RefPostOperationHandlerTests
    {
        public RefPostOperationHandlerTests()
        {
          _operationHandler = new ();
        }
        private readonly OpenApiDocument _openApiDocument = new();
        private readonly RefPostOperationHandler _operationHandler;

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(false, false)]
        public void CreateNavigationRefPostOperationReturnsCorrectOperation(bool enableOperationId, bool useHTTPStatusCodeClass2XX)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableOperationId = enableOperationId,
                UseSuccessStatusCodeRange = useHTTPStatusCodeClass2XX
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
            Assert.Equal("Create a trip.", operation.Summary);
            Assert.Equal("Create a new trip.", operation.Description);
            Assert.NotNull(operation.Tags);
            var tag = Assert.Single(operation.Tags);
            Assert.Equal("People.Trip", tag.Name);

            Assert.NotNull(operation.Parameters);
            Assert.NotEmpty(operation.Parameters);

            Assert.NotNull(operation.RequestBody);
            var requestBodyReference = Assert.IsType<OpenApiRequestBodyReference>(operation.RequestBody);
            Assert.Equal(Models.ReferenceType.RequestBody, requestBodyReference.Reference.Type);
            Assert.Equal(Common.Constants.ReferencePostRequestBodyName, requestBodyReference.Reference.Id);

            Assert.Equal(2, operation.Responses.Count);
            Assert.Equal(new string[] { "204", "default" }, operation.Responses.Select(e => e.Key));

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
