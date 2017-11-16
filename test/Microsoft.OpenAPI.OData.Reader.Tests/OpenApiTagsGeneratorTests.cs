// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.OpenApi.OData.Tests
{
    public class OpenApiTagsGeneratorTest
    {
        [Fact]
        public void CreateTagsThrowArgumentNull()
        {
            // Arrange
            IEdmModel model = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("model", () => model.CreateTags());
        }

        [Fact]
        public void CreateTagsReturnsNullForEmptyModel()
        {
            // Arrange & Act
            var tags = EdmModelHelper.EmptyModel.CreateTags();

            // Assert
            Assert.Null(tags);
        }

        [Fact]
        public void CreateTagsReturnsTagsForBasicModel()
        {
            // Arrange & Act
            var tags = EdmModelHelper.BasicEdmModel.CreateTags();

            // Assert
            Assert.NotNull(tags);
            Assert.Equal(4, tags.Count);
            Assert.Equal(new[] { "People", "City", "CountryOrRegion", "Me" }, tags.Select(t => t.Name));
        }

        [Fact]
        public void CreateTagsReturnsTagsForTripModel()
        {
            // Arrange & Act
            var tags = EdmModelHelper.TripServiceModel.CreateTags();

            // Assert
            Assert.NotNull(tags);
            Assert.Equal(5, tags.Count);
            Assert.Equal(new[] { "People", "Airlines", "Airports", "NewComePeople", "Me" },
                tags.Select(t => t.Name));
        }
    }
}
