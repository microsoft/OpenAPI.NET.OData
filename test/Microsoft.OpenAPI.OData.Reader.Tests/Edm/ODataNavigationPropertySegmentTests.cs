// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.OpenApi.OData.Edm.Tests
{
    public class ODataNavigationPropertySegmentTests
    {
        private IEdmEntityType _entityType;
        private IEdmNavigationProperty _property;
        public ODataNavigationPropertySegmentTests()
        {
            _entityType = new EdmEntityType("NS", "Entity");
            EdmNavigationPropertyInfo propertyInfo = new EdmNavigationPropertyInfo
            {
                Name = "Nav",
                TargetMultiplicity = EdmMultiplicity.One,
                Target = _entityType
            };
            _property = EdmNavigationProperty.CreateNavigationProperty(_entityType, propertyInfo);
        }

        [Fact]
        public void CtorThrowArgumentNullNavigationProperty()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>("navigationProperty", () => new ODataNavigationPropertySegment(navigationProperty: null));
        }

        [Fact]
        public void CtorSetNavigationPropertyProperty()
        {
            // Arrange & Act
            var segment = new ODataNavigationPropertySegment(_property);

            // Assert
            Assert.Same(_property, segment.NavigationProperty);
        }

        [Fact]
        public void GetEntityTypeReturnsCorrectEntityType()
        {
            // Arrange & Act
            var segment = new ODataNavigationPropertySegment(_property);

            // Assert
            Assert.Same(_entityType, segment.EntityType);
        }

        [Fact]
        public void GetNameReturnsCorrectNameString()
        {
            // Arrange & Act
            var segment = new ODataNavigationPropertySegment(_property);

            // Assert
            Assert.Equal("Nav", segment.Name);
        }

        [Fact]
        public void ToStringReturnsCorrectNameString()
        {
            // Arrange & Act
            var segment = new ODataNavigationPropertySegment(_property);

            // Assert
            Assert.Equal("Nav", segment.ToString());
        }
    }
}
