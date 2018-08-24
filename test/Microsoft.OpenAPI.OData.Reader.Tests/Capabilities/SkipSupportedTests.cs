// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OpenApi.OData.Capabilities;
using Xunit;

namespace Microsoft.OpenApi.OData.Reader.Capabilities.Tests
{
    public class SkipSupportedTests
    {
        [Fact]
        public void KindPropertyReturnsSkipSupportedEnumMember()
        {
            // Arrange & Act
            SkipSupported skip = new SkipSupported();

            // Assert
            Assert.Equal(CapabilitesTermKind.SkipSupported, skip.Kind);
        }

        [Fact]
        public void UnknownAnnotatableTargetReturnsDefaultSkipSupportedValues()
        {
            // Arrange
            SkipSupported skip = new SkipSupported();
            EdmEntityType entityType = new EdmEntityType("NS", "Entity");

            //  Act
            bool result = skip.Load(EdmCoreModel.Instance, entityType);

            // Assert
            Assert.False(result);
            Assert.True(skip.IsSupported);
            Assert.Null(skip.Supported);
        }

        [Theory]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.Inline)]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.OutOfLine)]
        public void TargetOnEntityTypeReturnsCorrectSkipSupportedValue(EdmVocabularyAnnotationSerializationLocation location)
        {
            // Arrange
            const string template = @"
                <Annotations Target=""NS.Calendar"">
                  {0}
                </Annotations>";

            IEdmModel model = GetEdmModel(template, location);
            Assert.NotNull(model); // guard

            IEdmEntityType calendar = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Calendar");
            Assert.NotNull(calendar); // guard

            // Act
            SkipSupported skip = new SkipSupported();
            bool result = skip.Load(model, calendar);

            // Assert
            Assert.True(result);
            Assert.False(skip.IsSupported);
            Assert.NotNull(skip.Supported);
            Assert.False(skip.Supported.Value);
        }

        [Theory]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.Inline)]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.OutOfLine)]
        public void TargetOnEntitySetReturnsCorrectSkipSupportedValue(EdmVocabularyAnnotationSerializationLocation location)
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
            SkipSupported skip = new SkipSupported();
            bool result = skip.Load(model, calendars);

            // Assert
            Assert.True(result);
            Assert.NotNull(skip.Supported);
            Assert.False(skip.Supported.Value);
        }

        private static IEdmModel GetEdmModel(string template, EdmVocabularyAnnotationSerializationLocation location)
        {
            string countAnnotation = @"<Annotation Term=""Org.OData.Capabilities.V1.SkipSupported"" Bool=""false"" />";

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
