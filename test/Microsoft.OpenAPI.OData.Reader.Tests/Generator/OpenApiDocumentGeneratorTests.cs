// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Tests;
using Xunit;

namespace Microsoft.OpenApi.OData.Generator.Tests
{
    public class OpenApiDocumentGeneratorTest
    {
        [Fact]
        public void CreateDocumentThrowArgumentNullContext()
        {
            // Arrange
            ODataContext context = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context", () => context.CreateDocument());
        }

        [Fact]
        public void CreateDocumentReturnsForEmptyModel()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.EmptyModel;
            ODataContext context = new ODataContext(model);

            // Act
            var document = context.CreateDocument();

            // Assert
            Assert.NotNull(document);
            Assert.Equal("3.0.0", document.SpecVersion.ToString());
            Assert.NotNull(document.Info);
            Assert.NotNull(document.Tags);
            Assert.NotNull(document.Servers);
            Assert.NotNull(document.Paths);
            Assert.NotNull(document.Components);

            Assert.Null(document.ExternalDocs);
            Assert.Null(document.SecurityRequirements);
        }
    }
}
