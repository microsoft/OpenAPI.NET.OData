// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OpenApi.OData.Capabilities;
using Xunit;

namespace Microsoft.OpenApi.OData.Reader.Capabilities.Tests
{
    public class InsertRestrictionsTests
    {
        [Fact]
        public void KindPropertyReturnsInsertRestrictionsEnumMember()
        {
            // Arrange & Act
            InsertRestrictions insert = new InsertRestrictions();

            // Assert
            Assert.Equal(CapabilitesTermKind.InsertRestrictions, insert.Kind);
        }

        [Fact]
        public void UnknownAnnotatableTargetReturnsDefaultInsertRestrictionsValues()
        {
            // Arrange
            InsertRestrictions insert = new InsertRestrictions();
            EdmEntityType entityType = new EdmEntityType("NS", "Entity");

            //  Act
            bool result = insert.Load(EdmCoreModel.Instance, entityType);

            // Assert
            Assert.False(result);
            Assert.True(insert.IsInsertable);
            Assert.Null(insert.Insertable);
            Assert.Null(insert.NonInsertableNavigationProperties);
        }

        [Theory]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.Inline)]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.OutOfLine)]
        public void TargetOnEntityTypeReturnsCorrectInsertRestrictionsValue(EdmVocabularyAnnotationSerializationLocation location)
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
            InsertRestrictions insert = new InsertRestrictions();
            bool result = insert.Load(model, calendars);

            // Assert
            Assert.True(result);
            VerifyInsertRestrictions(insert);
        }

        [Theory]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.Inline)]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.OutOfLine)]
        public void TargetOnEntitySetReturnsCorrectInsertRestrictionsValue(EdmVocabularyAnnotationSerializationLocation location)
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
            InsertRestrictions insert = new InsertRestrictions();
            bool result = insert.Load(model, calendars);

            // Assert
            Assert.True(result);
            VerifyInsertRestrictions(insert);
        }

        private static IEdmModel GetEdmModel(string template, EdmVocabularyAnnotationSerializationLocation location)
        {
            string countAnnotation = @"
                <Annotation Term=""Org.OData.Capabilities.V1.InsertRestrictions"" >
                  <Record>
                    <PropertyValue Property=""Insertable"" Bool=""false"" />
                    <PropertyValue Property=""NonInsertableNavigationProperties"" >
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
                return CapabilitiesModelHelper.GetEdmModelTypeInline(countAnnotation);
            }
        }

        private static void VerifyInsertRestrictions(InsertRestrictions insert)
        {
            Assert.NotNull(insert);

            Assert.NotNull(insert.Insertable);
            Assert.False(insert.Insertable.Value);

            Assert.NotNull(insert.NonInsertableNavigationProperties);
            Assert.Equal(2, insert.NonInsertableNavigationProperties.Count);
            Assert.Equal("abc|RelatedEvents", String.Join("|", insert.NonInsertableNavigationProperties));

            Assert.True(insert.IsNonInsertableNavigationProperty("RelatedEvents"));
            Assert.False(insert.IsNonInsertableNavigationProperty("MyUnknownNavigationProperty"));
        }
    }
}
