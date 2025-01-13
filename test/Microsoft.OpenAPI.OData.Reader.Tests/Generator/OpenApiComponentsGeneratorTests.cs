// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Tests;
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

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context", () => context.CreateComponents(openApiDocument));
        }

        [Fact]
        public void CreateComponentsReturnsForEmptyModel()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.EmptyModel;
            ODataContext context = new ODataContext(model);
            OpenApiDocument openApiDocument = new();

            // Act
            var components = context.CreateComponents(openApiDocument);

            // Assert
            Assert.NotNull(components);
            Assert.NotNull(components.Schemas);
            Assert.NotNull(components.Parameters);
            Assert.NotNull(components.Responses);
            Assert.NotNull(components.RequestBodies);
        }
    }
}
