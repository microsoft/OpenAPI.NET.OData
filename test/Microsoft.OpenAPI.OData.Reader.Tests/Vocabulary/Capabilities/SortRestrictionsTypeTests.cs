// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;
using Xunit;

namespace Microsoft.OpenApi.OData.Reader.Vocabulary.Capabilities.Tests
{
    public class SortRestrictionsTypeTests
    {
        [Fact]
        public void TermAttributeAttachedOnSortRestrictionsType()
        {
            // Arrange & Act
            string qualifiedName = Utils.GetTermQualifiedName<SortRestrictionsType>();

            // Assert
            Assert.Equal("Org.OData.Capabilities.V1.SortRestrictions", qualifiedName);
        }

        [Fact]
        public void UnknownAnnotatableTargetReturnsDefaultSortRestrictionsValues()
        {
            // Arrange & Act
            EdmEntityType entityType = new EdmEntityType("NS", "Entity");

            //  Act
            SortRestrictionsType sort = EdmCoreModel.Instance.GetRecord<SortRestrictionsType>(entityType);

            // Assert
            Assert.Null(sort);
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
            SortRestrictionsType sort = model.GetRecord<SortRestrictionsType>(calendar);

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
            SortRestrictionsType sort = model.GetRecord<SortRestrictionsType>(calendars);

            // Assert
            VerifySortRestrictions(sort);
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
                return CapabilitiesModelHelper.GetEdmModelTypeInline(countAnnotation);
            }
        }

        private static void VerifySortRestrictions(SortRestrictionsType sort)
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

            Assert.True(sort.IsAscendingOnlyProperty("abc"));
            Assert.False(sort.IsAscendingOnlyProperty("rst"));

            Assert.False(sort.IsDescendingOnlyProperty("abc"));
            Assert.True(sort.IsDescendingOnlyProperty("rst"));

            Assert.False(sort.IsNonSortableProperty("abc"));
            Assert.True(sort.IsNonSortableProperty("Emails"));
        }
    }
}
