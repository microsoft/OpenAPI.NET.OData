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
    public class OpenApiPathItemGeneratorTest
    {
        private OpenApiConvertSettings _settings = new OpenApiConvertSettings();

        [Fact]
        public void CreatePathItemsThrowArgumentNullModel()
        {
            // Arrange
            IEdmModel model = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("model", () => model.CreatePathItems(_settings));
        }

        [Fact]
        public void CreatePathItemsThrowArgumentNullSettings()
        {
            // Arrange
            IEdmModel model = EdmCoreModel.Instance;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("settings", () => model.CreatePathItems(settings: null));
        }

        [Fact]
        public void CreatePathItemsReturnsForEmptyModel()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.EmptyModel;

            // Act
            var pathItems = model.CreatePathItems(_settings);

            // Assert
            Assert.NotNull(pathItems);
            Assert.Empty(pathItems);
        }

        [Fact]
        public void CreatePathItemsReturnsForBasicModel()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.BasicEdmModel;

            // Act
            var pathItems = model.CreatePathItems(_settings);

            // Assert
            Assert.NotNull(pathItems);
            Assert.Equal(7, pathItems.Count);

            Assert.Contains("/People", pathItems.Keys);
            Assert.Contains("/People('{UserName}')", pathItems.Keys);
            Assert.Contains("/City", pathItems.Keys);
            Assert.Contains("/City('{Name}')", pathItems.Keys);
            Assert.Contains("/CountryOrRegion", pathItems.Keys);
            Assert.Contains("/CountryOrRegion('{Name}')", pathItems.Keys);
            Assert.Contains("/Me", pathItems.Keys);
        }
    }
}
