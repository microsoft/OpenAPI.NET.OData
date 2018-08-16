// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.OpenApi.OData.Edm.Tests
{
    public class ODataPathTests
    {
        private IEdmEntityType _singleKeyEntityType;
        private IEdmEntitySet _singleKeyEntitySet;

        private IEdmEntityType _multipleKeyEntityType;
        private IEdmEntitySet _multipleKeyEntitySet;

        public ODataPathTests()
        {
            // Single key entity type
            EdmEntityType entityType = new EdmEntityType("NS", "Order");
            entityType.AddKeys(entityType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));
            _singleKeyEntityType = entityType;

            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
            EdmEntitySet entitySet = new EdmEntitySet(container, "Orders", entityType);
            _singleKeyEntitySet = entitySet;

            // Multiple keys entity type
            entityType = new EdmEntityType("NS", "Customer");
            entityType.AddKeys(entityType.AddStructuralProperty("FirstName", EdmPrimitiveTypeKind.String),
                entityType.AddStructuralProperty("LastName", EdmPrimitiveTypeKind.String));
            _multipleKeyEntityType = entityType;

            entitySet = new EdmEntitySet(container, "Customers", entityType);
            _multipleKeyEntitySet = entitySet;
        }

        [Theory]
        [InlineData(true, "/Orders/{Order-Id}")]
        [InlineData(false, "/Orders({Order-Id})")]
        public void GetPathItemNameReturnsCorrectStringWithSingleKeySegment(bool keyAsSegment, string expected)
        {
            // Arrange
            ODataNavigationSourceSegment nsSegment = new ODataNavigationSourceSegment(_singleKeyEntitySet);
            ODataKeySegment keySegment = new ODataKeySegment(_singleKeyEntityType);
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
            ODataNavigationSourceSegment nsSegment = new ODataNavigationSourceSegment(_multipleKeyEntitySet);
            ODataKeySegment keySegment = new ODataKeySegment(_multipleKeyEntityType);
            ODataPath path = new ODataPath(nsSegment, keySegment);

            // Act
            string pathItemName = path.GetPathItemName(keyAsSegment);

            // Assert
            Assert.Equal("/Customers(FirstName={FirstName},LastName={LastName})", pathItemName);
        }
    }
}
