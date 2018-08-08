// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.OpenApi.OData.Edm.Tests
{
    public class ODataOperationISegmentTests
    {
        private IEdmOperation _operation = new EdmAction("NS", "MyAction", null);

        [Fact]
        public void CtorThrowArgumentNullOperation()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>("operation", () => new ODataOperationSegment(operation: null));
        }

        [Fact]
        public void CtorSetOperationProperty()
        {
            // Arrange & Act
            var segment = new ODataOperationSegment(_operation);

            // Assert
            Assert.Same(_operation, segment.Operation);
        }

        [Fact]
        public void GetEntityTypeThrowsNotImplementedException()
        {
            // Arrange & Act
            var segment = new ODataOperationSegment(_operation);

            // Assert
            Assert.Throws<NotImplementedException>(() => segment.EntityType);
        }

        [Fact]
        public void GetNameReturnsCorrectNameString()
        {
            // Arrange & Act
            var segment = new ODataOperationSegment(_operation);

            // Assert
            Assert.Equal("MyAction", segment.Name);
        }

        [Fact]
        public void ToStringReturnsCorrectNameString()
        {
            // Arrange & Act
            var segment = new ODataOperationSegment(_operation);

            // Assert
            Assert.Equal("MyAction", segment.ToString());
        }
    }
}
