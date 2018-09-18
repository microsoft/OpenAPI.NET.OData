// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Tests;
using Xunit;

namespace Microsoft.OpenApi.OData.Edm.Tests
{
    public class ODataPathProviderTests
    {
        [Fact]
        public void CtorThrowArgumentNullModel()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>("model", () => new ODataPathProvider(model: null));
        }

        [Fact]
        public void CreatePathsForEmptyEdmModelReturnsEmptyPaths()
        {
            // Arrange
            IEdmModel model = new EdmModel();
            ODataPathProvider provider = new ODataPathProvider(model);

            // Act
            var paths = provider.CreatePaths();

            // Assert
            Assert.NotNull(paths);
            Assert.Empty(paths);
        }

        [Fact]
        public void CreatePathsForGraphBetaModelReturnsAllPaths()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.GraphBetaModel;
            ODataPathProvider provider = new ODataPathProvider(model);

            // Act
            var paths = provider.CreatePaths();

            // Assert
            Assert.NotNull(paths);
            Assert.Equal(4399, paths.Count());
        }
    }
}
