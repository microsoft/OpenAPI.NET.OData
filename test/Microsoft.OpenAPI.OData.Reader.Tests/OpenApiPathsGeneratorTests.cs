// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.OpenApi.OData.Tests
{
    public class OpenApiPathsGeneratorTest
    {
        [Fact]
        public void CreatePathsThrowArgumentNull()
        {
            // Arrange
            IEdmModel model = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("model", () => model.CreatePaths());
        }

        [Fact]
        public void CreatePathsReturnsForEmptyModel()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.EmptyModel;

            // Act
            var paths = model.CreatePaths();

            // Assert
            Assert.NotNull(paths);
            Assert.Empty(paths);
        }

        [Fact]
        public void CreatePathsReturnsForBasicModel()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.BasicEdmModel;

            // Act
            var paths = model.CreatePaths();

            // Assert
            Assert.NotNull(paths);
            Assert.Equal(6, paths.Count);

            Assert.Contains("/People", paths.Keys);
            Assert.Contains("/People('{UserName}')", paths.Keys);
            Assert.Contains("/City", paths.Keys);
            Assert.Contains("/City('{Name}')", paths.Keys);
            Assert.Contains("/CountryOrRegion", paths.Keys);
            Assert.Contains("/CountryOrRegion('{Name}')", paths.Keys);
        }
    }
}
