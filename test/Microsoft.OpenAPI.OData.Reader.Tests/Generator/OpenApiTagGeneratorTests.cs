// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.OpenApi.OData.Tests;
using Xunit;

namespace Microsoft.OpenApi.OData.Generator.Tests
{
    public class OpenApiTagGeneratorTest
    {
        [Fact]
        public void CreateTagsThrowArgumentNullContext()
        {
            // Arrange
            ODataContext context = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context", () => context.CreateTags());
        }

        [Fact]
        public void CreateTagsReturnsEmptyTagsForEmptyModel()
        {
            // Arrange
            ODataContext context = new ODataContext(EdmModelHelper.EmptyModel);

            // Act
            var tags = context.CreateTags();

            // Assert
            Assert.NotNull(tags);
            Assert.Empty(tags);
        }

        [Fact]
        public void CreateTagsReturnsTagsForBasicModel()
        {
            // Arrange
            ODataContext context = new ODataContext(EdmModelHelper.BasicEdmModel);

            // Act
            var tags = context.CreateTags();

            // Assert
            Assert.NotNull(tags);
            Assert.Equal(4, tags.Count);
            Assert.Equal(new[] { "People", "City", "CountryOrRegion", "Me" }, tags.Select(t => t.Name));
        }

        [Fact]
        public void CreateTagsReturnsTagsForTripModel()
        {
            // Arrange
            ODataContext context = new ODataContext(EdmModelHelper.TripServiceModel);

            // Act
            var tags = context.CreateTags();

            // Assert
            Assert.NotNull(tags);
            Assert.Equal(6, tags.Count);
            Assert.Equal(new[] { "People", "Airlines", "Airports", "NewComePeople", "Me", "ResetDataSource" },
                tags.Select(t => t.Name));
        }
    }
}
