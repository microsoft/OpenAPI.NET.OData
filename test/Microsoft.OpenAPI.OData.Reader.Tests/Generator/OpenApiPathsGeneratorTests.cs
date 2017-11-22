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
    public class OpenApiPathsGeneratorTest
    {
        private OpenApiConvertSettings _settings = new OpenApiConvertSettings();

        [Fact]
        public void CreatePathsThrowArgumentNullContext()
        {
            // Arrange
            ODataContext context = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context", () => context.CreatePaths());
        }

        [Fact]
        public void CreatePathsReturnsForEmptyModel()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.EmptyModel;
            ODataContext context = new ODataContext(model);

            // Act
            var paths = context.CreatePaths();

            // Assert
            Assert.NotNull(paths);
            Assert.Empty(paths);
        }

        [Fact]
        public void CreatePathsReturnsForBasicModel()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.BasicEdmModel;
            ODataContext context = new ODataContext(model);

            // Act
            var paths = context.CreatePaths();

            // Assert
            Assert.NotNull(paths);
            Assert.Equal(7, paths.Count);

            Assert.Contains("/People", paths.Keys);
            Assert.Contains("/People('{UserName}')", paths.Keys);
            Assert.Contains("/City", paths.Keys);
            Assert.Contains("/City('{Name}')", paths.Keys);
            Assert.Contains("/CountryOrRegion", paths.Keys);
            Assert.Contains("/CountryOrRegion('{Name}')", paths.Keys);
            Assert.Contains("/Me", paths.Keys);
        }
    }
}
