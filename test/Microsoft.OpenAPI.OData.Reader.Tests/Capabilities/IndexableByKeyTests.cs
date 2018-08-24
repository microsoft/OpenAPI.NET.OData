// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OpenApi.OData.Capabilities;
using Xunit;

namespace Microsoft.OpenApi.OData.Reader.Capabilities.Tests
{
    public class IndexableByKeyTests
    {
        [Fact]
        public void KindPropertyReturnsIndexableByKeyEnumMember()
        {
            // Arrange & Act
            IndexableByKey index = new IndexableByKey();

            // Assert
            Assert.Equal(CapabilitesTermKind.IndexableByKey, index.Kind);
        }

        [Fact]
        public void UnknownAnnotatableTargetReturnsDefaultIndexableByKeyValues()
        {
            // Arrange
            IndexableByKey index = new IndexableByKey();
            EdmEntityType entityType = new EdmEntityType("NS", "Entity");

            //  Act
            bool result = index.Load(EdmCoreModel.Instance, entityType);

            // Assert
            Assert.False(result);
            Assert.True(index.IsSupported);
            Assert.Null(index.Supported);
        }

        [Theory]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.Inline)]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.OutOfLine)]
        public void TargetOnEntityTypeReturnsCorrectIndexableByKeyValue(EdmVocabularyAnnotationSerializationLocation location)
        {
            // Arrange
            const string template = @"
                <Annotations Target=""NS.Calendar"">
                  {0}
                </Annotations>";

            IEdmModel model = GetEdmModel(template, location);
            Assert.NotNull(model); // guard

            IEdmEntitySet calendars = model.EntityContainer.FindEntitySet("Calendars");
            Assert.NotNull(calendars); // guard

            // Act
            IndexableByKey index = new IndexableByKey();
            bool result = index.Load(model, calendars);

            // Assert
            Assert.True(result);
            Assert.NotNull(index.Supported);
            Assert.False(index.Supported.Value);
        }

        [Theory]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.Inline)]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.OutOfLine)]
        public void TargetOnEntitySetReturnsCorrectIndexableByKeyValue(EdmVocabularyAnnotationSerializationLocation location)
        {
            // Arrange
            const string template = @"
                <Annotations Target=""NS.Default/Calendars"">
                  {0}
                </Annotations>";

            IEdmModel model = GetEdmModel(template, location);
            Assert.NotNull(model); // guard

            IEdmEntitySet calendars = model.EntityContainer.FindEntitySet("Calendars");
            Assert.NotNull(calendars); // guard

            // Act
            IndexableByKey index = new IndexableByKey();
            bool result = index.Load(model, calendars);

            // Assert
            Assert.True(result);
            Assert.NotNull(index.Supported);
            Assert.False(index.Supported.Value);
        }

        private static IEdmModel GetEdmModel(string template, EdmVocabularyAnnotationSerializationLocation location)
        {
            string countAnnotation = @"<Annotation Term=""Org.OData.Capabilities.V1.IndexableByKey"" Bool=""false"" />";

            if (location == EdmVocabularyAnnotationSerializationLocation.OutOfLine)
            {
                countAnnotation = string.Format(template, countAnnotation);
                return CapabilitiesModelHelper.GetEdmModelOutline(countAnnotation);
            }
            else
            {
                return CapabilitiesModelHelper.GetEdmModelTypeInline(countAnnotation);
            }
        }
    }
}
