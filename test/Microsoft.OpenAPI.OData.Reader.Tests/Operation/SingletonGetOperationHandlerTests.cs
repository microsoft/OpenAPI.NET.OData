// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Tests;
using Xunit;

namespace Microsoft.OpenApi.OData.Operation.Tests
{
    public class SingletonGetOperationHandlerTests
    {
        private SingletonGetOperationHandler _operationHandler = new SingletonGetOperationHandler();

        [Fact]
        public void CreateSingletonGetOperationReturnsCorrectOperation()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.BasicEdmModel;
            IEdmSingleton singleton = model.EntityContainer.FindSingleton("Me");
            ODataContext context = new ODataContext(model);
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(singleton));

            // Act
            var get = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(get);
            Assert.Equal("Get Me", get.Summary);
            Assert.NotNull(get.Tags);
            var tag = Assert.Single(get.Tags);
            Assert.Equal("Me.Person", tag.Name);

            Assert.NotNull(get.Parameters);
            Assert.Equal(2, get.Parameters.Count);

            Assert.Null(get.RequestBody);

            Assert.NotNull(get.Responses);
            Assert.Equal(2, get.Responses.Count);
            Assert.Equal(new[] { "200", "default" }, get.Responses.Select(r => r.Key));
        }
    }
}
