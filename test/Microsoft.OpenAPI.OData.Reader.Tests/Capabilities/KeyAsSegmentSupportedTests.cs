// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OpenApi.OData.Capabilities;
using Xunit;
using Microsoft.OData.Edm.Vocabularies;

namespace Microsoft.OpenApi.OData.Reader.Capabilities.Tests
{
    public class KeyAsSegmentSupportedTests
    {
        [Fact]
        public void UnknownAnnotatableTargetReturnsDefaultTopSupportedValues()
        {
            // Arrange
            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");

            // Act
            KeyAsSegmentSupported keyAsSegment = new KeyAsSegmentSupported(EdmCoreModel.Instance, container);

            // Assert
            Assert.Equal(CapabilitiesConstants.KeyAsSegmentSupported, keyAsSegment.QualifiedName);
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
            KeyAsSegmentSupported KeyAsSegmentSupported = new KeyAsSegmentSupported(model, model.EntityContainer);

            // Assert
            Assert.NotNull(KeyAsSegmentSupported.Supported);
            Assert.Equal(support, KeyAsSegmentSupported.Supported.Value);
        }

        private static IEdmModel GetEdmModel(EdmVocabularyAnnotationSerializationLocation location, bool supported)
        {
            EdmModel model = new EdmModel();
            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
            model.AddElement(container);
            IEdmTerm term = model.FindTerm(CapabilitiesConstants.KeyAsSegmentSupported);
            if (term == null)
            {
                // NOTE: KeyAsSegmentSupported annotation term is not included in OData Spec 4.0.
                // Please remove the codes here once it's supported in the latest OData lib.
                term = new EdmTerm(CapabilitiesConstants.Namespace, "KeyAsSegmentSupported", EdmPrimitiveTypeKind.Boolean);
                model.AddElement(term);
            }

            IEdmBooleanConstantExpression boolean = new EdmBooleanConstant(supported);
            EdmVocabularyAnnotation annotation = new EdmVocabularyAnnotation(container, term, boolean);
            annotation.SetSerializationLocation(model, location);
            model.SetVocabularyAnnotation(annotation);
            return model;
        }
    }
}
