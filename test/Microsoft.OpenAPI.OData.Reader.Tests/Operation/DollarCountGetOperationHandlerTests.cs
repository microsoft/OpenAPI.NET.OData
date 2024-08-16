// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using System.Linq;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Tests;
using Xunit;
using Microsoft.OpenApi.Models;

namespace Microsoft.OpenApi.OData.Operation.Tests
{
    public class DollarCountGetOperationHandlerTests
    {
        private readonly DollarCountGetOperationHandler _operationHandler = new();

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(false, false)]
        public void CreateDollarCountGetOperationForNavigationPropertyReturnsCorrectOperation(bool enableOperationId, bool useHTTPStatusCodeClass2XX)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            OpenApiConvertSettings settings = new()
            {
                EnableOperationId = enableOperationId,
                UseSuccessStatusCodeRange = useHTTPStatusCodeClass2XX
            };
            ODataContext context = new(model, settings);
            IEdmEntitySet people = model.EntityContainer.FindEntitySet("People");
            Assert.NotNull(people);

            IEdmEntityType person = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Person");
            IEdmNavigationProperty navProperty = person.DeclaredNavigationProperties().First(c => c.Name == "Trips");
            ODataPath path = new(new ODataNavigationSourceSegment(people),
                new ODataKeySegment(people.EntityType),
                new ODataNavigationPropertySegment(navProperty),
                new ODataDollarCountSegment());

            // Act
            var operation = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(operation);
            Assert.Equal("Get the number of the resource", operation.Summary);

            Assert.NotNull(operation.Parameters);
            Assert.Equal(4, operation.Parameters.Count);
            Assert.Equal(new[] { "UserName", "ConsistencyLevel", "search", "filter"},
                operation.Parameters.Select(x => x.Name ?? x.Reference.Id).ToList());
            
            Assert.Null(operation.RequestBody);

            Assert.Equal(2, operation.Responses.Count);
            var statusCode = useHTTPStatusCodeClass2XX ? "2XX" : "200";
            Assert.Equal(new string[] { statusCode, "default" }, operation.Responses.Select(e => e.Key));

            if (enableOperationId)
            {
                Assert.Equal("People.Trips.GetCount-e877", operation.OperationId);
            }
            else
            {
                Assert.Null(operation.OperationId);
            }
        }

        [Fact]
        public void CreateDollarCountGetOperationForNavigationPropertyWithTargetPathAnnotationsReturnsCorrectOperation()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.GraphBetaModel;
            ODataContext context = new(model, new OpenApiConvertSettings());
            IEdmEntitySet users = model.EntityContainer.FindEntitySet("users");
            Assert.NotNull(users);

            IEdmEntityType user = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "user");
            IEdmNavigationProperty navProperty = user.DeclaredNavigationProperties().First(c => c.Name == "appRoleAssignments");
            ODataPath path = new(new ODataNavigationSourceSegment(users),
                new ODataKeySegment(users.EntityType),
                new ODataNavigationPropertySegment(navProperty),
                new ODataDollarCountSegment());

            // Act
            var operation = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(operation.Parameters);
            Assert.Equal(4, operation.Parameters.Count);
            Assert.Equal(new[] { "id", "ConsistencyLevel", "search", "filter" },
                operation.Parameters.Select(x => x.Name ?? x.Reference.Id).ToList());

            Assert.Equal("Get the number of the resource", operation.Summary);

            Assert.Null(operation.RequestBody);

            Assert.Equal(2, operation.Responses.Count);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(false, false)]
        public void CreateDollarCountGetOperationForNavigationSourceReturnsCorrectOperation(bool enableOperationId, bool useHTTPStatusCodeClass2XX)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            OpenApiConvertSettings settings = new()
            {
                EnableOperationId = enableOperationId,
                UseSuccessStatusCodeRange = useHTTPStatusCodeClass2XX
            };
            ODataContext context = new(model, settings);
            IEdmEntitySet people = model.EntityContainer.FindEntitySet("People");
            Assert.NotNull(people);

            ODataPath path = new(new ODataNavigationSourceSegment(people),
                new ODataDollarCountSegment());

            // Act
            var operation = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(operation);
            Assert.Equal("Get the number of the resource", operation.Summary);
            Assert.NotNull(operation.Parameters);
            Assert.Equal(3, operation.Parameters.Count);
            Assert.Equal(new[] { "ConsistencyLevel", "search", "filter" },
                operation.Parameters.Select(x => x.Name ?? x.Reference.Id).ToList());

            Assert.Null(operation.RequestBody);
            Assert.Equal(2, operation.Responses.Count);
            var statusCode = useHTTPStatusCodeClass2XX ? "2XX" : "200";
            Assert.Equal(new string[] { statusCode, "default" }, operation.Responses.Select(e => e.Key));

            if (enableOperationId)
            {
                Assert.Equal("People.GetCount-dd8d", operation.OperationId);
            }
            else
            {
                Assert.Null(operation.OperationId);
            }
        }

        [Fact]
        public void CreateDollarCountGetOperationForNavigationSourceWithTargetPathAnnotationsReturnsCorrectOperation()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.GraphBetaModel;
            ODataContext context = new(model, new OpenApiConvertSettings());
            IEdmEntitySet users = model.EntityContainer.FindEntitySet("users");
            Assert.NotNull(users);

            ODataPath path = new(new ODataNavigationSourceSegment(users),
                new ODataDollarCountSegment());

            // Act
            var operation = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(operation);
            Assert.Equal("Get the number of the resource", operation.Summary);
            Assert.NotNull(operation.Parameters);
            Assert.Equal(3, operation.Parameters.Count);
            Assert.Equal(new[] { "ConsistencyLevel", "search", "filter" },
                operation.Parameters.Select(x => x.Name ?? x.Reference.Id).ToList());

            Assert.Null(operation.RequestBody);
            Assert.Equal(2, operation.Responses.Count);
        }
    }
}
