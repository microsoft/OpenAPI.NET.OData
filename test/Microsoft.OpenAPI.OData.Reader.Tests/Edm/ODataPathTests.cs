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
        public void ODataPathConstructorThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>("source", () => new ODataPath(null));
        }

        [Fact]
        public void ODataPathPopThrowsForEmptyPath()
        {
            // Arrange
            ODataPath path = new ODataPath();

            // Act
            Action text = () => path.Pop();

            // Assert
            var exception = Assert.Throws<InvalidOperationException>(text);
            Assert.Equal(SRResource.ODataPathPopInvalid, exception.Message);
        }

        [Fact]
        public void ODataPathPushWorks()
        {
            // Arrange
            ODataPath path = new ODataPath();
            Assert.Empty(path); // guard

            // Act
            path.Push(new ODataNavigationSourceSegment(_simpleKeyEntitySet));

            // Assert
            Assert.Single(path);
            Assert.Equal(1, path.Count);
        }

        [Fact]
        public void ODataPathFirstSegmentWorks()
        {
            // Arrange
            ODataNavigationSourceSegment nsSegment = new ODataNavigationSourceSegment(_simpleKeyEntitySet);
            ODataKeySegment keySegment = new ODataKeySegment(_simpleKeyEntityType);
            ODataPath path = new ODataPath(nsSegment, keySegment);

            // Act
            var segment = path.FirstSegment;

            // Assert
            Assert.Same(nsSegment, segment);
        }

        [Fact]
        public void ODataPathLastSegmentWorks()
        {
            // Arrange
            ODataNavigationSourceSegment nsSegment = new ODataNavigationSourceSegment(_simpleKeyEntitySet);
            ODataKeySegment keySegment = new ODataKeySegment(_simpleKeyEntityType);
            ODataPath path = new ODataPath(nsSegment, keySegment);

            // Act
            var segment = path.LastSegment;

            // Assert
            Assert.Same(keySegment, segment);
        }

        [Fact]
        public void KindPropertyReturnsUnknown()
        {
            // Arrange
            ODataKeySegment keySegment = new ODataKeySegment(_simpleKeyEntityType);
            ODataPath path = new ODataPath(keySegment);

            // Act & Assert
            Assert.Equal(ODataPathKind.Unknown, path.Kind);
        }

        [Fact]
        public void KindPropertyReturnsEntity()
        {
            // Arrange
            ODataNavigationSourceSegment nsSegment = new ODataNavigationSourceSegment(_simpleKeyEntitySet);
            ODataKeySegment keySegment = new ODataKeySegment(_simpleKeyEntityType);
            ODataPath path = new ODataPath(nsSegment, keySegment);

            // Act & Assert
            Assert.Equal(ODataPathKind.Entity, path.Kind);
        }

        [Fact]
        public void KindPropertyReturnsSingleton()
        {
            // Arrange
            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
            EdmSingleton me = new EdmSingleton(container, "Me", _simpleKeyEntityType);
            ODataNavigationSourceSegment nsSegment = new ODataNavigationSourceSegment(me);
            ODataPath path = new ODataPath(nsSegment);

            // Act & Assert
            Assert.Equal(ODataPathKind.Singleton, path.Kind);
        }

        [Fact]
        public void KindPropertyReturnsEntitySet()
        {
            // Arrange
            ODataNavigationSourceSegment nsSegment = new ODataNavigationSourceSegment(_compositeKeyEntitySet);
            ODataPath path = new ODataPath(nsSegment);

            // Act & Assert
            Assert.Equal(ODataPathKind.EntitySet, path.Kind);
        }

        [Fact]
        public void KindPropertyReturnsOperation()
        {
            // Arrange
            ODataNavigationSourceSegment nsSegment = new ODataNavigationSourceSegment(_simpleKeyEntitySet);
            EdmAction action = new EdmAction("NS", "MyAction", null, isBound: true, entitySetPathExpression: null);
            ODataOperationSegment opSegment = new ODataOperationSegment(action);
            ODataPath path = new ODataPath(nsSegment, opSegment);

            // Act & Assert
            Assert.Equal(ODataPathKind.Operation, path.Kind);
        }

        [Fact]
        public void KindPropertyReturnsNavigationProperty()
        {
            // Arrange
            EdmNavigationPropertyInfo propertyInfo = new EdmNavigationPropertyInfo
            {
                Name = "Nav",
                TargetMultiplicity = EdmMultiplicity.One,
                Target = _compositeKeyEntityType
            };
            var property = EdmNavigationProperty.CreateNavigationProperty(_simpleKeyEntityType, propertyInfo);
            ODataNavigationPropertySegment npSegment = new ODataNavigationPropertySegment(property);
            ODataNavigationSourceSegment nsSegment = new ODataNavigationSourceSegment(_simpleKeyEntitySet);
            ODataKeySegment keySegment = new ODataKeySegment(_simpleKeyEntityType);
            ODataPath path = new ODataPath(nsSegment, keySegment, npSegment);

            // Act & Assert
            Assert.Equal(ODataPathKind.NavigationProperty, path.Kind);
        }

        [Fact]
        public void KindPropertyReturnsOperationImport()
        {
            // Arrange
            IEdmEntityContainer container = new EdmEntityContainer("NS", "default");
            IEdmAction action = new EdmAction("NS", "MyAction", null);
            var operationImport = new EdmActionImport(container, "MyAction", action);
            ODataOperationImportSegment segment = new ODataOperationImportSegment(operationImport);
            ODataPath path = new ODataPath(segment);

            // Act & Assert
            Assert.Equal(ODataPathKind.OperationImport, path.Kind);
        }

        [Fact]
        public void KindPropertyReturnsStreamProperty()
        {
            // Arrange
            ODataNavigationSourceSegment nsSegment = new ODataNavigationSourceSegment(_simpleKeyEntitySet);
            ODataKeySegment keySegment = new ODataKeySegment(_simpleKeyEntityType);
            ODataStreamPropertySegment streamPropSegment = new ODataStreamPropertySegment("Logo");
            ODataPath path = new ODataPath(nsSegment, keySegment, streamPropSegment);

            // Act & Assert
            Assert.Equal(ODataPathKind.MediaEntity, path.Kind);
        }

        [Fact]
        public void KindPropertyReturnsStreamContent()
        {
            // Arrange
            ODataNavigationSourceSegment nsSegment = new ODataNavigationSourceSegment(_simpleKeyEntitySet);
            ODataKeySegment keySegment = new ODataKeySegment(_simpleKeyEntityType);
            ODataStreamContentSegment streamContSegment = new ODataStreamContentSegment();
            ODataPath path = new ODataPath(nsSegment, keySegment, streamContSegment);

            // Act & Assert
            Assert.Equal(ODataPathKind.MediaEntity, path.Kind);
        }

        [Theory]
        [InlineData(true, true, "/Orders/{Order-Id}")]
        [InlineData(true, false, "/Orders/{Id}")]
        [InlineData(false, true, "/Orders({Order-Id})")]
        [InlineData(false, false, "/Orders({Id})")]
        public void GetPathItemNameReturnsCorrectWithSingleKeySegment(bool keyAsSegment, bool prefix, string expected)
        {
            // Arrange
            ODataNavigationSourceSegment nsSegment = new ODataNavigationSourceSegment(_simpleKeyEntitySet);
            ODataKeySegment keySegment = new ODataKeySegment(_simpleKeyEntityType);
            ODataPath path = new ODataPath(nsSegment, keySegment);
            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableKeyAsSegment = keyAsSegment,
                PrefixEntityTypeNameBeforeKey = prefix
            };

            // Act
            string pathItemName = path.GetPathItemName(settings);

            // Assert
            Assert.Equal(expected, pathItemName);
        }

        [Theory]
        [InlineData(true, true, "/Customers/FirstName='{FirstName}',LastName='{LastName}'")]
        [InlineData(true, false, "/Customers/FirstName='{FirstName}',LastName='{LastName}'")]
        [InlineData(false, true, "/Customers(FirstName='{FirstName}',LastName='{LastName}')")]
        [InlineData(false, false, "/Customers(FirstName='{FirstName}',LastName='{LastName}')")]
        public void GetPathItemNameReturnsCorrectStringWithMultipleKeySegment(bool keyAsSegment, bool prefix, string expected)
        {
            // Arrange
            ODataNavigationSourceSegment nsSegment = new ODataNavigationSourceSegment(_compositeKeyEntitySet);
            ODataKeySegment keySegment = new ODataKeySegment(_compositeKeyEntityType);
            ODataPath path = new ODataPath(nsSegment, keySegment);
            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableKeyAsSegment = keyAsSegment,
                PrefixEntityTypeNameBeforeKey = prefix
            };

            // Act
            string pathItemName = path.GetPathItemName(settings);

            // Assert
            Assert.Equal(expected, pathItemName);
        }
    }
}
