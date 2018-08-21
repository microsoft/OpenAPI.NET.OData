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
        private IEdmEntityType _person { get; }

        public ODataTypeCastSegmentTests()
        {
            var person = new EdmEntityType("NS", "Person");
            person.AddKeys(person.AddStructuralProperty("Id", EdmCoreModel.Instance.GetString(false)));
            _person = person;
        }

        [Fact]
        public void TypeCastSegmentConstructorThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>("entityType", () => new ODataTypeCastSegment(null));
        }

        [Fact]
        public void TypeCastSegmentEntityTypePropertyReturnsSameEntityType()
        {
            // Arrange & Act
            var segment = new ODataTypeCastSegment(_person);

            // Assert
            Assert.Same(_person, segment.EntityType);
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
