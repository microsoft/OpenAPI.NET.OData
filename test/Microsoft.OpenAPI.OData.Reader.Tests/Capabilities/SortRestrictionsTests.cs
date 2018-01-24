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
    public class SortRestrictionsTests
    {
        [Fact]
        public void UnknownAnnotatableTargetReturnsDefaultSortRestrictionsValues()
        {
            // Arrange
            EdmEntityType entityType = new EdmEntityType("NS", "Entity");

            // Act
            SortRestrictions sort = new SortRestrictions(EdmCoreModel.Instance, entityType);

            // Assert
            Assert.Equal(CapabilitiesConstants.SortRestrictions, sort.QualifiedName);
            Assert.Null(sort.Sortable);
            Assert.Null(sort.AscendingOnlyProperties);
            Assert.Null(sort.DescendingOnlyProperties);
            Assert.Null(sort.NonSortableProperties);
        }

        [Theory]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.Inline)]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.OutOfLine)]
        public void TargetOnEntityTypeReturnsCorrectSortRestrictionsValue(EdmVocabularyAnnotationSerializationLocation location)
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
            SortRestrictions sort = new SortRestrictions(model, calendar);

            // Assert
            VerifySortRestrictions(sort);
        }

        [Theory]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.Inline)]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.OutOfLine)]
        public void TargetOnEntitySetReturnsCorrectSortRestrictionsValue(EdmVocabularyAnnotationSerializationLocation location)
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
            SortRestrictions sort = new SortRestrictions(model, calendars);

            // Assert
            VerifySortRestrictions(sort);
        }

        [Theory]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.Inline)]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.OutOfLine)]
        public void TargetOnNavigationPropertyReturnsCorrectSortRestrictionsValue(EdmVocabularyAnnotationSerializationLocation location)
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
            SortRestrictions sort = new SortRestrictions(model, navigationProperty);

            // Assert
            VerifySortRestrictions(sort);
        }

        [Theory]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.Inline)]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.OutOfLine)]
        public void IsNonSortablePropertiesReturnsCorrectForProperty(EdmVocabularyAnnotationSerializationLocation location)
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

            IEdmProperty emails = calendar.DeclaredStructuralProperties().First(c => c.Name == "Emails");
            Assert.NotNull(emails); // Guard

            // Act
            SortRestrictions sort = new SortRestrictions(model, calendar);

            // Assert
            Assert.NotNull(sort.Sortable);
            Assert.False(sort.Sortable.Value);
            Assert.True(sort.IsNonSortableProperty(emails));
        }

        private static IEdmModel GetEdmModel(string template, EdmVocabularyAnnotationSerializationLocation location, bool navInLine = false)
        {
            string countAnnotation = @"
                <Annotation Term=""Org.OData.Capabilities.V1.SortRestrictions"" >
                  <Record>
                    <PropertyValue Property=""Sortable"" Bool=""false"" />
                    <PropertyValue Property=""AscendingOnlyProperties"" >
                      <Collection>
                        <PropertyPath>abc</PropertyPath>
                      </Collection>
                    </PropertyValue>
                    <PropertyValue Property=""DescendingOnlyProperties"" >
                      <Collection>
                        <PropertyPath>rst</PropertyPath>
                      </Collection>
                    </PropertyValue>
                    <PropertyValue Property=""NonSortableProperties"" >
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

        private static void VerifySortRestrictions(SortRestrictions sort)
        {
            Assert.NotNull(sort);

            Assert.NotNull(sort.Sortable);
            Assert.False(sort.Sortable.Value);

            Assert.NotNull(sort.AscendingOnlyProperties);
            Assert.Single(sort.AscendingOnlyProperties);
            Assert.Equal("abc", sort.AscendingOnlyProperties.First());

            Assert.NotNull(sort.DescendingOnlyProperties);
            Assert.Single(sort.DescendingOnlyProperties);
            Assert.Equal("rst", sort.DescendingOnlyProperties.First());

            Assert.NotNull(sort.NonSortableProperties);
            Assert.Single(sort.NonSortableProperties);
            Assert.Equal("Emails", sort.NonSortableProperties.First());
        }
    }
}
