// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OpenApi.OData.Capabilities;
using Xunit;

namespace Microsoft.OpenApi.OData.Reader.Capabilities.Tests
{
    public class ExpandRestrictionsTests
    {
        [Fact]
        public void UnknownAnnotatableTargetReturnsDefaultExpandRestrictionsValues()
        {
            // Arrange
            EdmEntityType entityType = new EdmEntityType("NS", "Entity");

            // Act
            ExpandRestrictions expand = new ExpandRestrictions(EdmCoreModel.Instance, entityType);

            // Assert
            Assert.Equal(CapabilitiesConstants.ExpandRestrictions, expand.QualifiedName);
            Assert.Null(expand.Expandable);
            Assert.Null(expand.NonExpandableProperties);
        }

        [Theory]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.Inline)]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.OutOfLine)]
        public void TargetOnEntityTypeReturnsCorrectExpandRestrictionsValue(EdmVocabularyAnnotationSerializationLocation location)
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
            ExpandRestrictions expand = new ExpandRestrictions(model, calendar);

            // Assert
            VerifyExpandRestrictions(expand);
        }

        [Theory]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.Inline)]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.OutOfLine)]
        public void TargetOnEntitySetReturnsCorrectExpandRestrictionsValue(EdmVocabularyAnnotationSerializationLocation location)
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
            ExpandRestrictions expand = new ExpandRestrictions(model, calendars);

            // Assert
            VerifyExpandRestrictions(expand);
        }

        [Theory]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.Inline)]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.OutOfLine)]
        public void TargetOnNavigationPropertyReturnsCorrectExpandRestrictionsValue(EdmVocabularyAnnotationSerializationLocation location)
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
            ExpandRestrictions expand = new ExpandRestrictions(model, navigationProperty);

            // Assert
            VerifyExpandRestrictions(expand);
        }

        [Theory]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.Inline)]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.OutOfLine)]
        public void IsNonExpandablePropertyReturnsCorrectForProperty(EdmVocabularyAnnotationSerializationLocation location)
        {
            // Arrange
            const string template = @"
                <Annotations Target=""NS.Calendar"">
                  {0}
                </Annotations>";

            IEdmModel model = GetEdmModel(template, location);
            Assert.NotNull(model); // guard

            IEdmEntityType calendar = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Calendar");
            Assert.NotNull(calendar); // Guard

            IEdmNavigationProperty navigationProperty = calendar.DeclaredNavigationProperties().First(c => c.Name == "RelatedEvents");
            Assert.NotNull(navigationProperty); // Guard

            // Act
            ExpandRestrictions expand = new ExpandRestrictions(model, calendar);

            // Assert
            Assert.NotNull(expand.Expandable);
            Assert.False(expand.Expandable.Value);
            Assert.True(expand.IsNonExpandableProperty(navigationProperty));
        }

        private static IEdmModel GetEdmModel(string template, EdmVocabularyAnnotationSerializationLocation location, bool navInLine = false)
        {
            string countAnnotation = @"
                <Annotation Term=""Org.OData.Capabilities.V1.ExpandRestrictions"" >
                  <Record>
                    <PropertyValue Property=""Expandable"" Bool=""false"" />
                    <PropertyValue Property=""NonExpandableProperties"" >
                      <Collection>
                        <NavigationPropertyPath>abc</NavigationPropertyPath>
                        <NavigationPropertyPath>RelatedEvents</NavigationPropertyPath>
                      </Collection>
                    </PropertyValue>
                  </Record>
                </Annotation>";

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

        private static void VerifyExpandRestrictions(ExpandRestrictions expand)
        {
            Assert.NotNull(expand);

            Assert.NotNull(expand.Expandable);
            Assert.False(expand.Expandable.Value);

            Assert.NotNull(expand.NonExpandableProperties);
            Assert.Equal(2, expand.NonExpandableProperties.Count);
            Assert.Equal("abc|RelatedEvents", String.Join("|", expand.NonExpandableProperties));
        }
    }
}
