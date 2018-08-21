// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.OpenApi.OData.Edm.Tests
{
    public class ODataNavigationSourceSegmentTests
    {
        private IEdmEntityType _entityType;
        private IEdmEntitySet _entitySet;
        private IEdmSingleton _singleton;

        public ODataNavigationSourceSegmentTests()
        {
            _entityType = new EdmEntityType("NS", "Entity");
            IEdmEntityContainer container = new EdmEntityContainer("NS", "Container");
            _entitySet = new EdmEntitySet(container, "Entities", _entityType);
            _singleton = new EdmSingleton(container, "Me", _entityType);
        }

        [Fact]
        public void CtorThrowArgumentNullNavigationSource()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>("navigationSource", () => new ODataNavigationSourceSegment(navigationSource: null));
        }

        [Fact]
        public void CtorSetNavigationSourceProperty()
        {
            // Arrange & Act
            var segment = new ODataNavigationSourceSegment(_entitySet);

            // Assert
            Assert.Same(_entitySet, segment.NavigationSource);
        }

        [Fact]
        public void GetEntityTypeReturnsCorrectEntityType()
        {
            // Arrange & Act
            var segment = new ODataNavigationSourceSegment(_singleton);

            // Assert
            Assert.Same(_entityType, segment.EntityType);
        }

        [Fact]
        public void KindPropertyReturnsNavigationSourceEnumMember()
        {
            // Arrange & Act
            var segment = new ODataNavigationSourceSegment(_singleton);

            // Assert
            Assert.Equal(ODataSegmentKind.NavigationSource, segment.Kind);
        }

        [Fact]
        public void GetPathItemNameReturnsCorrectNavigationSourceLiteralForEntitySet()
        {
            // Arrange & Act
            var segment = new ODataNavigationSourceSegment(_entitySet);

            // Assert
            Assert.Equal("Entities", segment.GetPathItemName(new OpenApiConvertSettings()));
        }

        [Fact]
        public void GetPathItemNameReturnsCorrectNavigationSourceLiteralForSingleton()
        {
            // Arrange & Act
            var segment = new ODataNavigationSourceSegment(_singleton);

            // Assert
            Assert.Equal("Me", segment.GetPathItemName(new OpenApiConvertSettings()));
        }
    }
}
