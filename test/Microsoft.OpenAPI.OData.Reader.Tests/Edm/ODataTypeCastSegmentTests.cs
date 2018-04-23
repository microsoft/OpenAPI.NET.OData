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
        public IEdmEntityType Person { get; }

        public ODataTypeCastSegmentTests()
        {
            var person = new EdmEntityType("NS", "Person");
            person.AddKeys(person.AddStructuralProperty("Id", EdmCoreModel.Instance.GetString(false)));
            Person = person;
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
            ODataTypeCastSegment segment = new ODataTypeCastSegment(Person);

            // Assert
            Assert.Same(Person, segment.EntityType);
        }

        [Fact]
        public void TypeCastSegmentToStringReturnsCorrectKeyString()
        {
            // Arrange & Act
            ODataTypeCastSegment segment = new ODataTypeCastSegment(Person);

            // Assert
            Assert.Equal("NS.Person", segment.ToString());
        }
    }
}
