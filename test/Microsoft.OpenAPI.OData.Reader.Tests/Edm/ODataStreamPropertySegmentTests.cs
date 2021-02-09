// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.OpenApi.OData.Edm.Tests
{
    public class ODataStreamPropertySegmentTests
    {
        private readonly EdmEntityType _todo;

        public ODataStreamPropertySegmentTests()
        {
            _todo = new EdmEntityType("microsoft.graph", "Todo");
            _todo.AddKeys(_todo.AddStructuralProperty("Id", EdmPrimitiveTypeKind.String));
            _todo.AddKeys(_todo.AddStructuralProperty("Logo", EdmPrimitiveTypeKind.Stream));
            _todo.AddKeys(_todo.AddStructuralProperty("Description", EdmPrimitiveTypeKind.String));
        }

        [Fact]
        public void StreamPropertySegmentConstructorThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>("streamPropertyName", () => new ODataStreamPropertySegment(null));
        }

        [Fact]
        public void StreamPropertySegmentIdentifierPropertyReturnsStreamPropertyNameOfEntity()
        {
            // Arrange
            var streamPropName = _todo.DeclaredStructuralProperties().First(c => c.Name == "Logo").Name;

            // Act
            ODataStreamPropertySegment segment = new ODataStreamPropertySegment(streamPropName);

            // Assert
            Assert.Same(streamPropName, segment.Identifier);
        }

        [Fact]
        public void KindPropertyReturnsStreamPropertyEnumMember()
        {
            // Arrange
            var streamPropName = _todo.DeclaredStructuralProperties().First(c => c.Name == "Logo").Name;

            // Act
            ODataStreamPropertySegment segment = new ODataStreamPropertySegment(streamPropName);

            // Assert
            Assert.Equal(ODataSegmentKind.StreamProperty, segment.Kind);
        }

        [Fact]
        public void GetPathItemNameReturnsCorrectStreamPropertyNameOfEntity()
        {
            // Arrange
            var streamPropName = _todo.DeclaredStructuralProperties().First(c => c.Name == "Logo").Name;

            // Act
            ODataStreamPropertySegment segment = new ODataStreamPropertySegment(streamPropName);

            // Assert
            Assert.Equal(streamPropName, segment.GetPathItemName(new OpenApiConvertSettings()));
        }
    }
}
