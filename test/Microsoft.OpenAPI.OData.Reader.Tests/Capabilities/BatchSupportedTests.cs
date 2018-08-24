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
        public void KindPropertyReturnsBatchSupportedEnumMember()
        {
            // Arrange & Act
            BatchSupported batch = new BatchSupported();

            // Assert
            Assert.Equal(CapabilitesTermKind.BatchSupported, batch.Kind);
        }

        [Fact]
        public void UnknownAnnotatableTargetReturnsDefaultBatchSupportedValues()
        {
            // Arrange
            BatchSupported batch = new BatchSupported();
            EdmEntityType entityType = new EdmEntityType("NS", "Entity");

            //  Act
            bool result = batch.Load(EdmCoreModel.Instance, entityType);

            // Assert
            Assert.False(result);
            Assert.True(batch.IsSupported);
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
            BatchSupported batch = new BatchSupported();
            bool result = batch.Load(model, model.EntityContainer);

            // Assert
            Assert.True(result);
            Assert.NotNull(batch.Supported);
            Assert.Equal(support, batch.Supported.Value);
            Assert.Equal(support, batch.IsSupported);
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
