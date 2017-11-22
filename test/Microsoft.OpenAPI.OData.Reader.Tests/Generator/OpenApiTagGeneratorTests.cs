// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Tests;
using Xunit;

namespace Microsoft.OpenApi.OData.Generator.Tests
{
    public class OpenApiTagGeneratorTest
    {
        private OpenApiConvertSettings _settings = new OpenApiConvertSettings();

        [Fact]
        public void CreateTagsThrowArgumentNullModel()
        {
            // Arrange
            IEdmModel model = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("model", () => model.CreateTags(_settings));
        }

        [Fact]
        public void CreateTagsThrowArgumentNullSettings()
        {
            // Arrange
            IEdmModel model = EdmCoreModel.Instance;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("settings", () => model.CreateTags(settings: null));
        }

        [Fact]
        public void CreateTagsReturnsNullForEmptyModel()
        {
            // Arrange & Act
            var tags = EdmModelHelper.EmptyModel.CreateTags(_settings);

            // Assert
            Assert.Null(tags);
        }

        [Fact]
        public void CreateTagsReturnsTagsForBasicModel()
        {
            // Arrange & Act
            var tags = EdmModelHelper.BasicEdmModel.CreateTags(_settings);

            // Assert
            Assert.NotNull(tags);
            Assert.Equal(4, tags.Count);
            Assert.Equal(new[] { "People", "City", "CountryOrRegion", "Me" }, tags.Select(t => t.Name));
        }

        [Fact]
        public void CreateTagsReturnsTagsForTripModel()
        {
            // Arrange & Act
            var tags = EdmModelHelper.TripServiceModel.CreateTags(_settings);

            // Assert
            Assert.NotNull(tags);
            Assert.Equal(5, tags.Count);
            Assert.Equal(new[] { "People", "Airlines", "Airports", "NewComePeople", "Me" },
                tags.Select(t => t.Name));
        }
    }
}
