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
    public class CountRestrictionsTests
    {
        [Fact]
        public void UnknownAnnotatableTargetReturnsDefaultPropertyValues()
        {
            // Arrange
            EdmEntityType entityType = new EdmEntityType("NS", "Entity");

            // Act
            CountRestrictions count = new CountRestrictions(EdmCoreModel.Instance, entityType);

            // Assert
            Assert.Equal(CapabilitiesConstants.CountRestrictions, count.QualifiedName);
            Assert.Null(count.Countable);
            Assert.Null(count.NonCountableProperties);
            Assert.Null(count.NonCountableNavigationProperties);
        }

        [Theory]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.Inline)]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.OutOfLine)]
        public void TargetOnEntityTypeReturnsCorrectCountRestrictionsValue(EdmVocabularyAnnotationSerializationLocation location)
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
            CountRestrictions count = new CountRestrictions(model, calendar);

            // Assert
            VerifyCountRestrictions(count);
        }

        [Theory]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.Inline)]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.OutOfLine)]
        public void TargetOnEntitySetReturnsCorrectCountRestrictionsValue(EdmVocabularyAnnotationSerializationLocation location)
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
            CountRestrictions count = new CountRestrictions(model, calendars);

            // Assert
            VerifyCountRestrictions(count);
        }

        [Theory]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.Inline)]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.OutOfLine)]
        public void TargetOnNavigationPropertyReturnsCorrectCountRestrictionsValue(EdmVocabularyAnnotationSerializationLocation location)
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
            CountRestrictions count = new CountRestrictions(model, navigationProperty);

            // Assert
            VerifyCountRestrictions(count);
        }

        [Theory]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.Inline)]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.OutOfLine)]
        public void IsNonCountablePropertyReturnsCorrectForProperty(EdmVocabularyAnnotationSerializationLocation location)
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

            IEdmProperty property = calendar.DeclaredStructuralProperties().First(c => c.Name == "Emails");
            Assert.NotNull(property); // Guard

            IEdmNavigationProperty navigationProperty = calendar.DeclaredNavigationProperties().First(c => c.Name == "RelatedEvents");
            Assert.NotNull(navigationProperty); // Guard

            // Act
            CountRestrictions count = new CountRestrictions(model, calendar);

            // Assert
            Assert.NotNull(count.Countable);
            Assert.False(count.Countable.Value);
            Assert.True(count.IsNonCountableProperty(property));
            Assert.True(count.IsNonCountableNavigationProperty(navigationProperty));
        }

        private static IEdmModel GetEdmModel(string template, EdmVocabularyAnnotationSerializationLocation location, bool navInLine = false)
        {
            string countAnnotation = @"
                <Annotation Term=""Org.OData.Capabilities.V1.CountRestrictions"" >
                  <Record>
                    <PropertyValue Property=""Countable"" Bool=""false"" />
                    <PropertyValue Property=""NonCountableProperties"">
                      <Collection>
                        <PropertyPath>Emails</PropertyPath>
                        <PropertyPath>mij</PropertyPath>
                      </Collection>
                    </PropertyValue>
                    <PropertyValue Property=""NonCountableNavigationProperties"" >
                      <Collection>
                        <NavigationPropertyPath>RelatedEvents</NavigationPropertyPath>
                        <NavigationPropertyPath>abc</NavigationPropertyPath>
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

        private static void VerifyCountRestrictions(CountRestrictions count)
        {
            Assert.NotNull(count);

            Assert.NotNull(count.Countable);
            Assert.False(count.Countable.Value);

            Assert.NotNull(count.NonCountableProperties);
            Assert.Equal(2, count.NonCountableProperties.Count);
            Assert.Equal("Emails|mij", String.Join("|", count.NonCountableProperties));

            Assert.NotNull(count.NonCountableNavigationProperties);
            Assert.Equal(2, count.NonCountableNavigationProperties.Count);
            Assert.Equal("RelatedEvents,abc", String.Join(",", count.NonCountableNavigationProperties));
        }
    }
}
