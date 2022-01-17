// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.OpenApi.OData.Edm.Tests
{
    public class ODataTypeCastSegmentTests
    {
        private readonly EdmEntityType _person;

        public ODataTypeCastSegmentTests()
        {
            _person = new EdmEntityType("NS", "Person");
            _person.AddKeys(_person.AddStructuralProperty("Id", EdmCoreModel.Instance.GetString(false)));
        }

        [Fact]
        public void TypeCastSegmentConstructorThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>("structuredType", () => new ODataTypeCastSegment(null));
        }

        [Fact]
        public void TypeCastSegmentEntityTypePropertyReturnsSameEntityType()
        {
            // Arrange & Act
            var segment = new ODataTypeCastSegment(_person);

            // Assert
            Assert.Null(segment.EntityType);
            Assert.Same(_person, segment.StructuredType);
        }

        [Fact]
        public void KindPropertyReturnsTypeCastEnumMember()
        {
            // Arrange & Act
            var segment = new ODataTypeCastSegment(_person);

            // Assert
            Assert.Equal(ODataSegmentKind.TypeCast, segment.Kind);
        }

        [Fact]
        public void GetPathItemNameReturnsCorrectTypeCastLiteral()
        {
            // Arrange & Act
            var segment = new ODataTypeCastSegment(_person);

            // Assert
            Assert.Equal("NS.Person", segment.GetPathItemName(new OpenApiConvertSettings()));
        }
    }
}
