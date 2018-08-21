// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.OpenApi.OData.Edm.Tests
{
    public class ODataKeySegmentTests
    {
        private IEdmEntityType _person { get; }
        private IEdmEntityType _customer { get; }

        public ODataKeySegmentTests()
        {
            // entity type with simple key
            var person = new EdmEntityType("NS", "Person");
            person.AddKeys(person.AddStructuralProperty("Id", EdmCoreModel.Instance.GetString(false)));
            _person = person;

            // entity type with composite keys
            var customer = new EdmEntityType("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("firstName", EdmCoreModel.Instance.GetString(false)));
            customer.AddKeys(customer.AddStructuralProperty("lastName", EdmCoreModel.Instance.GetString(false)));
            _customer = customer;
        }

        [Fact]
        public void KeySegmentConstructorThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>("entityType", () => new ODataKeySegment(null));
        }

        [Fact]
        public void KeySegmentEntityTypePropertyReturnsSameEntityType()
        {
            // Arrange & Act
            ODataKeySegment segment = new ODataKeySegment(_person);

            // Assert
            Assert.Same(_person, segment.EntityType);
        }

        [Fact]
        public void KindPropertyReturnsKeyEnumMember()
        {
            // Arrange & Act
            ODataKeySegment segment = new ODataKeySegment(_person);

            // Assert
            Assert.Equal(ODataSegmentKind.Key, segment.Kind);
        }

        [Theory]
        [InlineData(true, "{Person-Id}")]
        [InlineData(false, "{Id}")]
        public void GetPathItemNameReturnsCorrectKeyLiteralForSimpleKey(bool prefix, string expected)
        {
            // Arrange & Act
            ODataKeySegment segment = new ODataKeySegment(_person);
            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                PrefixEntityTypeNameBeforeKey = prefix
            };

            // Assert
            Assert.Equal(expected, segment.GetPathItemName(settings));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetPathItemNameReturnsCorrectKeyLiteralForCompositeKey(bool prefix)
        {
            // Arrange & Act
            ODataKeySegment segment = new ODataKeySegment(_customer);
            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                PrefixEntityTypeNameBeforeKey = prefix
            };

            // Assert
            Assert.Equal("firstName={firstName},lastName={lastName}", segment.GetPathItemName(settings));
        }
    }
}
