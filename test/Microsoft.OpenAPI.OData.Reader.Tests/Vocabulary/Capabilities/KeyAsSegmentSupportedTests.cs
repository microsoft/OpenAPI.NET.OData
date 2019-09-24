// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;
using Xunit;

namespace Microsoft.OpenApi.OData.Reader.Vocabulary.Capabilities.Tests
{
    public class KeyAsSegmentSupportedTests
    {
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
            bool? result = model.GetBoolean(model.EntityContainer, CapabilitiesConstants.KeyAsSegmentSupported);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(support, result.Value);
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
