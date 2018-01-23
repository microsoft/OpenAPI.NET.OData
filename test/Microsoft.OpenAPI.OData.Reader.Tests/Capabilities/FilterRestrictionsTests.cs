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
    public class FilterRestrictionsTests
    {
        [Fact]
        public void UnknownAnnotatableTargetReturnsDefaultFilterRestrictionsValues()
        {
            // Arrange
            EdmEntityType entityType = new EdmEntityType("NS", "Entity");

            // Act
            FilterRestrictions filter = new FilterRestrictions(EdmCoreModel.Instance, entityType);

            // Assert
            Assert.Equal(CapabilitiesConstants.FilterRestrictions, filter.QualifiedName);
            Assert.Null(filter.Filterable);
            Assert.Null(filter.RequiresFilter);
            Assert.Null(filter.RequiredProperties);
            Assert.Null(filter.NonFilterableProperties);
        }

        [Theory]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.Inline)]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.OutOfLine)]
        public void TargetOnEntityTypeReturnsCorrectfilterRestrictionsValue(EdmVocabularyAnnotationSerializationLocation location)
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
            FilterRestrictions filter = new FilterRestrictions(model, calendar);

            // Assert
            VerifyFilterRestrictions(filter);
        }

        [Theory]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.Inline)]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.OutOfLine)]
        public void TargetOnEntitySetReturnsCorrectFilterRestrictionsValue(EdmVocabularyAnnotationSerializationLocation location)
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
            FilterRestrictions filter = new FilterRestrictions(model, calendars);

            // Assert
            VerifyFilterRestrictions(filter);
        }

        [Theory]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.Inline)]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.OutOfLine)]
        public void TargetOnNavigationPropertyReturnsCorrectFilterRestrictionsValue(EdmVocabularyAnnotationSerializationLocation location)
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
            FilterRestrictions filter = new FilterRestrictions(model, navigationProperty);

            // Assert
            VerifyFilterRestrictions(filter);
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

            IEdmProperty id = calendar.DeclaredStructuralProperties().First(c => c.Name == "Id");
            Assert.NotNull(id); // Guard

            IEdmProperty emails = calendar.DeclaredStructuralProperties().First(c => c.Name == "Emails");
            Assert.NotNull(emails); // Guard

            // Act
            FilterRestrictions filter = new FilterRestrictions(model, calendar);

            // Assert
            Assert.NotNull(filter.Filterable);
            Assert.False(filter.Filterable.Value);
            Assert.NotNull(filter.RequiresFilter);
            Assert.False(filter.RequiresFilter.Value);

            Assert.True(filter.IsRequiredProperty(id));
            Assert.True(filter.IsNonFilterableProperty(emails));
        }

        private static IEdmModel GetEdmModel(string template, EdmVocabularyAnnotationSerializationLocation location, bool navInLine = false)
        {
            string countAnnotation = @"
                <Annotation Term=""Org.OData.Capabilities.V1.FilterRestrictions"" >
                  <Record>
                    <PropertyValue Property=""Filterable"" Bool=""false"" />
                    <PropertyValue Property=""RequiresFilter"" Bool=""false"" />
                    <PropertyValue Property=""RequiredProperties"" >
                      <Collection>
                        <PropertyPath>Id</PropertyPath>
                      </Collection>
                    </PropertyValue>
                    <PropertyValue Property=""NonFilterableProperties"" >
                      <Collection>
                        <PropertyPath>Emails</PropertyPath>
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

        private static void VerifyFilterRestrictions(FilterRestrictions filter)
        {
            Assert.NotNull(filter);

            Assert.NotNull(filter.Filterable);
            Assert.False(filter.Filterable.Value);

            Assert.NotNull(filter.RequiresFilter);
            Assert.False(filter.RequiresFilter.Value);

            Assert.NotNull(filter.RequiredProperties);
            Assert.Single(filter.RequiredProperties);
            Assert.Equal("Id", filter.RequiredProperties.First());

            Assert.NotNull(filter.NonFilterableProperties);
            Assert.Single(filter.NonFilterableProperties);
            Assert.Equal("Emails", filter.NonFilterableProperties.First());
        }
    }
}
