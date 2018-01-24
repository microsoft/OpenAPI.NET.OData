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
    public class TopSupportedTests
    {
        [Fact]
        public void UnknownAnnotatableTargetReturnsDefaultTopSupportedValues()
        {
            // Arrange
            EdmEntityType entityType = new EdmEntityType("NS", "Entity");

            // Act
            TopSupported top = new TopSupported(EdmCoreModel.Instance, entityType);

            // Assert
            Assert.Equal(CapabilitiesConstants.TopSupported, top.QualifiedName);
            Assert.Null(top.Supported);
        }

        [Theory]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.Inline)]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.OutOfLine)]
        public void TargetOnEntityTypeReturnsCorrectTopSupportedValue(EdmVocabularyAnnotationSerializationLocation location)
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
            TopSupported top = new TopSupported(model, calendar);

            // Assert
            Assert.NotNull(top.Supported);
            Assert.False(top.Supported.Value);
        }

        [Theory]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.Inline)]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.OutOfLine)]
        public void TargetOnEntitySetReturnsCorrectTopSupportedValue(EdmVocabularyAnnotationSerializationLocation location)
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
            TopSupported top = new TopSupported(model, calendars);

            // Assert
            Assert.NotNull(top.Supported);
            Assert.False(top.Supported.Value);
        }

        [Theory]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.Inline)]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.OutOfLine)]
        public void TargetOnNavigationPropertyReturnsCorrectTopSupportedValue(EdmVocabularyAnnotationSerializationLocation location)
        {
            // Arrange
            const string template = @"
                <Annotations Target=""NS.Calendar/RelatedEvents"">
                  {0}
                </Annotations>";

            IEdmModel model = GetEdmModel(template, location, true);
            Assert.NotNull(model); // guard

            IEdmEntityType calendar = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Calendar");
            Assert.NotNull(calendar); // guard

            IEdmNavigationProperty navigationProperty = calendar.DeclaredNavigationProperties().First(c => c.Name == "RelatedEvents");
            Assert.NotNull(navigationProperty); // guard

            // Act
            TopSupported top = new TopSupported(model, navigationProperty);

            // Assert
            Assert.NotNull(top.Supported);
            Assert.False(top.Supported.Value);
        }

        private static IEdmModel GetEdmModel(string template, EdmVocabularyAnnotationSerializationLocation location, bool navInLine = false)
        {
            string countAnnotation = @"<Annotation Term=""Org.OData.Capabilities.V1.TopSupported"" Bool=""false"" />";

            if (location == EdmVocabularyAnnotationSerializationLocation.OutOfLine)
            {
                countAnnotation = string.Format(template, countAnnotation);
                return CapabilitiesModelHelper.GetEdmModelOutline(countAnnotation);
            }
            else
            {
                if (navInLine)
                {
                    return CapabilitiesModelHelper.GetEdmModelNavInline(countAnnotation);
                }
                else
                {
                    return CapabilitiesModelHelper.GetEdmModelTypeInline(countAnnotation);
                }
            }
        }
    }
}
