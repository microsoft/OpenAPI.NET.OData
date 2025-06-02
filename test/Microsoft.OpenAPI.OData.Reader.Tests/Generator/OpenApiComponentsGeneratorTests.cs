// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Tests;
using Moq;
using Xunit;

namespace Microsoft.OpenApi.OData.Generator.Tests
{
    public class OpenApiComponentsGeneratorTest
    {
        [Fact]
        public void CreateComponentsThrowArgumentNullContext()
        {
            // Arrange
            ODataContext context = null;
            OpenApiDocument openApiDocument = new();
            var mockModel = new Mock<IEdmModel>().Object;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context", () => context.AddComponentsToDocument(openApiDocument));
            Assert.Throws<ArgumentNullException>("document", () => new ODataContext(mockModel).AddComponentsToDocument(null));
        }

        [Fact]
        public void CreateComponentsReturnsForEmptyModel()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.EmptyModel;
            ODataContext context = new ODataContext(model);
            OpenApiDocument openApiDocument = new();

            // Act
            context.AddComponentsToDocument(openApiDocument);
            var components = openApiDocument.Components;

            // Assert
            Assert.NotNull(components);
            Assert.NotNull(components.Schemas);
            Assert.NotNull(components.Parameters);
            Assert.NotNull(components.Responses);
            Assert.NotNull(components.RequestBodies);
        }
    }
}
