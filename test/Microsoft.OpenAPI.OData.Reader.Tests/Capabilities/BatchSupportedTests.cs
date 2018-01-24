// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Capabilities;
using Xunit;

namespace Microsoft.OpenApi.OData.Reader.Capabilities.Tests
{
    public class BatchSupportedTests
    {
        [Fact]
        public void UnknownAnnotatableTargetReturnsDefaultBatchSupportedValues()
        {
            // Arrange
            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");

            // Act
            BatchSupported batch = new BatchSupported(EdmCoreModel.Instance, container);

            // Assert
            Assert.Equal(CapabilitiesConstants.BatchSupported, batch.QualifiedName);
            Assert.Null(batch.Supported);
        }

        [Theory]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.Inline, true)]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.Inline, false)]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.OutOfLine, true)]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.OutOfLine, false)]
        public void EntitySetContainerReturnsCorrectBatchSupportedValue(EdmVocabularyAnnotationSerializationLocation location, bool support)
        {
            // Arrange
            IEdmModel model = GetEdmModel(location, support);
            Assert.NotNull(model); // guard

            // Act
            BatchSupported batch = new BatchSupported(model, model.EntityContainer);

            // Assert
            Assert.NotNull(batch.Supported);
            Assert.Equal(support, batch.Supported.Value);
        }

        private static IEdmModel GetEdmModel(EdmVocabularyAnnotationSerializationLocation location, bool supported)
        {
            EdmModel model = new EdmModel();
            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
            model.AddElement(container);
            IEdmTerm term = model.FindTerm(CapabilitiesConstants.BatchSupported);
            Assert.NotNull(term);

            IEdmBooleanConstantExpression boolean = new EdmBooleanConstant(supported);
            EdmVocabularyAnnotation annotation = new EdmVocabularyAnnotation(container, term, boolean);
            annotation.SetSerializationLocation(model, location);
            model.SetVocabularyAnnotation(annotation);
            return model;
        }
    }
}
