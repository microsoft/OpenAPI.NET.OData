// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Properties;
using Xunit;

namespace Microsoft.OpenApi.OData.Edm.Tests
{
    public class ODataPathTests
    {
        private IEdmEntityType _simpleKeyEntityType;
        private IEdmEntitySet _simpleKeyEntitySet;

        private IEdmEntityType _compositeKeyEntityType;
        private IEdmEntitySet _compositeKeyEntitySet;

        public ODataPathTests()
        {
            // Single key entity type
            EdmEntityType entityType = new EdmEntityType("NS", "Order");
            entityType.AddKeys(entityType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));
            _simpleKeyEntityType = entityType;

            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
            EdmEntitySet entitySet = new EdmEntitySet(container, "Orders", entityType);
            _simpleKeyEntitySet = entitySet;

            // Multiple keys entity type
            entityType = new EdmEntityType("NS", "Customer");
            entityType.AddKeys(entityType.AddStructuralProperty("FirstName", EdmPrimitiveTypeKind.String),
                entityType.AddStructuralProperty("LastName", EdmPrimitiveTypeKind.String));
            _compositeKeyEntityType = entityType;

            entitySet = new EdmEntitySet(container, "Customers", entityType);
            _compositeKeyEntitySet = entitySet;
        }

        [Fact]
        public void PopThrowsForEmptyPath()
        {
            // Arrange
            ODataPath path = new ODataPath();

            // Act
            Action text = () => path.Pop();

            // Assert
            var exception = Assert.Throws<InvalidOperationException>(text);
            Assert.Equal(SRResource.ODataPathPopInvalid, exception.Message);
        }

        [Theory]
        [InlineData(true, "/Orders/{Order-Id}")]
        [InlineData(false, "/Orders({Order-Id})")]
        public void GetPathItemNameReturnsCorrectStringWithSingleKeySegment(bool keyAsSegment, string expected)
        {
            // Arrange
            ODataNavigationSourceSegment nsSegment = new ODataNavigationSourceSegment(_simpleKeyEntitySet);
            ODataKeySegment keySegment = new ODataKeySegment(_simpleKeyEntityType);
            ODataPath path = new ODataPath(nsSegment, keySegment);

            // Act
            string pathItemName = path.GetPathItemName(keyAsSegment);

            // Assert
            Assert.Equal(expected, pathItemName);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetPathItemNameReturnsCorrectStringWithMultipleKeySegment(bool keyAsSegment)
        {
            // Arrange
            ODataNavigationSourceSegment nsSegment = new ODataNavigationSourceSegment(_compositeKeyEntitySet);
            ODataKeySegment keySegment = new ODataKeySegment(_compositeKeyEntityType);
            ODataPath path = new ODataPath(nsSegment, keySegment);

            // Act
            string pathItemName = path.GetPathItemName(keyAsSegment);

            // Assert
            Assert.Equal("/Customers(FirstName={FirstName},LastName={LastName})", pathItemName);
        }
    }
}
