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
        public IEdmEntityType Person { get; }

        public IEdmEntityType Customer { get; }

        public ODataKeySegmentTests()
        {
            var person = new EdmEntityType("NS", "Person");
            person.AddKeys(person.AddStructuralProperty("Id", EdmCoreModel.Instance.GetString(false)));
            Person = person;

            var customer = new EdmEntityType("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("firstName", EdmCoreModel.Instance.GetString(false)));
            customer.AddKeys(customer.AddStructuralProperty("lastName", EdmCoreModel.Instance.GetString(false)));
            Customer = customer;
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
            ODataKeySegment segment = new ODataKeySegment(Person);

            // Assert
            Assert.Same(Person, segment.EntityType);
        }

        [Fact]
        public void KeySegmentToStringReturnsCorrectKeyString()
        {
            // Arrange & Act
            ODataKeySegment segment = new ODataKeySegment(Person);

            // Assert
            Assert.Equal("{Id}", segment.ToString());
        }

        [Fact]
        public void KeySegmentToStringReturnsCorrectKeyStringForCompositeKeys()
        {
            // Arrange & Act
            ODataKeySegment segment = new ODataKeySegment(Customer);

            // Assert
            Assert.Equal("{firstName={firstName},lastName={lastName}}", segment.ToString());
        }
    }
}
