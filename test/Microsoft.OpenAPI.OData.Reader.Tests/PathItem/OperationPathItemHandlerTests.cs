// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Tests;
using Xunit;

namespace Microsoft.OpenApi.OData.PathItem.Tests
{
    public class OperationPathItemHandlerTest
    {
        private OperationPathItemHandler _pathItemHandler = new OperationPathItemHandler();

        [Fact]
        public void CreatePathItemThrowsForNullContext()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>("context",
                () => _pathItemHandler.CreatePathItem(context: null, path: new ODataPath()));
        }

        [Fact]
        public void CreatePathItemThrowsForNullPath()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>("path",
                () => _pathItemHandler.CreatePathItem(new ODataContext(EdmCoreModel.Instance), path: null));
        }

        [Theory]
        [InlineData("GetFriendsTrips", "People", OperationType.Get)]
        [InlineData("ShareTrip", "People", OperationType.Post)]
        public void CreatePathItemForOperationReturnsCorrectPathItem(string operationName, string entitySet,
            OperationType operationType)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            ODataContext context = new ODataContext(model);
            IEdmNavigationSource navigationSource = model.EntityContainer.FindEntitySet(entitySet);
            Assert.NotNull(navigationSource); // guard
            IEdmOperation edmOperation = model.SchemaElements.OfType<IEdmOperation>()
                .FirstOrDefault(o => o.Name == operationName);
            Assert.NotNull(edmOperation); // guard
            string expectSummary = "Invoke " +
                (edmOperation.IsAction() ? "action " : "function ") + operationName;
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(navigationSource), new ODataOperationSegment(edmOperation));

            // Act
            OpenApiPathItem pathItem = _pathItemHandler.CreatePathItem(context, path);

            // Assert
            Assert.NotNull(pathItem);
            Assert.NotNull(pathItem.Operations);
            var operationKeyValue = Assert.Single(pathItem.Operations);
            Assert.Equal(operationType, operationKeyValue.Key);
            Assert.NotNull(operationKeyValue.Value);

            Assert.Equal(expectSummary, operationKeyValue.Value.Summary);
        }
    }
}
