// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OpenApi.OData.Capabilities;
using Microsoft.OData.Edm.Vocabularies;
using Xunit;

namespace Microsoft.OpenApi.OData.Reader.Capabilities.Tests
{
    public class KeyAsSegmentSupportedTests
    {
        [Fact]
        public void KindPropertyReturnsKeyAsSegmentSupportedEnumMember()
        {
            // Arrange & Act
            KeyAsSegmentSupported keyAsSegment = new KeyAsSegmentSupported();

            // Assert
            Assert.Equal(CapabilitesTermKind.KeyAsSegmentSupported, keyAsSegment.Kind);
        }

        [Fact]
        public void UnknownAnnotatableTargetReturnsDefaultTopSupportedValues()
        {
            // Arrange
            KeyAsSegmentSupported keyAsSegment = new KeyAsSegmentSupported();
            EdmEntityType entityType = new EdmEntityType("NS", "Entity");

            //  Act
            bool result = keyAsSegment.Load(EdmCoreModel.Instance, entityType);

            // Assert
            Assert.False(result);
            Assert.True(keyAsSegment.IsSupported);
            Assert.Null(keyAsSegment.Supported);
        }

        [Theory]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.Inline, true)]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.Inline, false)]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.OutOfLine, true)]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.OutOfLine, false)]
        public void EntitySetContainerReturnsCorrectKeyAsSegmentSupportedValue(EdmVocabularyAnnotationSerializationLocation location, bool support)
        {
            // Arrange
            IEdmModel model = GetEdmModel(location, support);
            Assert.NotNull(model); // guard

            // Act
            KeyAsSegmentSupported KeyAsSegmentSupported = new KeyAsSegmentSupported();
            bool result = KeyAsSegmentSupported.Load(model, model.EntityContainer);

            // Assert
            Assert.True(result);
            Assert.NotNull(KeyAsSegmentSupported.Supported);
            Assert.Equal(support, KeyAsSegmentSupported.Supported.Value);
        }

        private static IEdmModel GetEdmModel(EdmVocabularyAnnotationSerializationLocation location, bool supported)
        {
            EdmModel model = new EdmModel();
            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
            model.AddElement(container);
            IEdmTerm term = model.FindTerm(CapabilitiesConstants.KeyAsSegmentSupported);
            Assert.NotNull(term);

            IEdmBooleanConstantExpression boolean = new EdmBooleanConstant(supported);
            EdmVocabularyAnnotation annotation = new EdmVocabularyAnnotation(container, term, boolean);
            annotation.SetSerializationLocation(model, location);
            model.SetVocabularyAnnotation(annotation);
            return model;
        }
    }
}
