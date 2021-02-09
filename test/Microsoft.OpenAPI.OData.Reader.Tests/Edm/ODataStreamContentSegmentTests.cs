// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.OpenApi.OData.Edm.Tests
{
    public class ODataStreamContentSegmentTests
    {
        private readonly EdmEntityType _todo;

        public ODataStreamContentSegmentTests()
        {
            _todo = new EdmEntityType("microsoft.graph", "Todo",
                new EdmEntityType("microsoft.graph", "Task"),
                isAbstract: false,
                isOpen: false,
                hasStream: true);
            _todo.AddKeys(_todo.AddStructuralProperty("Id", EdmPrimitiveTypeKind.String));
            _todo.AddKeys(_todo.AddStructuralProperty("Logo", EdmPrimitiveTypeKind.Stream));
            _todo.AddKeys(_todo.AddStructuralProperty("Description", EdmPrimitiveTypeKind.String));
        }

        [Fact]
        public void StreamContentSegmentIdentifierPropertyReturnsCorrectDefaultValue()
        {
            // Arrange & Act
            ODataStreamContentSegment segment = new ODataStreamContentSegment();

            // Assert
            Assert.Same("$value", segment.Identifier);
        }

        [Fact]
        public void KindPropertyReturnsStreamContentEnumMember()
        {
            // Arrange & Act
            ODataStreamContentSegment segment = new ODataStreamContentSegment();

            // Assert
            Assert.Equal(ODataSegmentKind.StreamContent, segment.Kind);
        }

        [Fact]
        public void GetPathItemNameReturnsCorrectDefaultStreamContentValue()
        {
            // Arrange & Act
            ODataStreamContentSegment segment = new ODataStreamContentSegment();

            // Assert
            Assert.Equal("$value", segment.GetPathItemName(new OpenApiConvertSettings()));
        }
    }
}
