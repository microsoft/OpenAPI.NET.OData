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
    public class UpdateRestrictionsTests
    {
        [Fact]
        public void KindPropertyReturnsUpdateRestrictionsEnumMember()
        {
            // Arrange & Act
            UpdateRestrictions update = new UpdateRestrictions();

            // Assert
            Assert.Equal(CapabilitesTermKind.UpdateRestrictions, update.Kind);
        }

        [Fact]
        public void UnknownAnnotatableTargetReturnsDefaultUpdateRestrictionsValues()
        {
            // Arrange
            UpdateRestrictions update = new UpdateRestrictions();
            EdmEntityType entityType = new EdmEntityType("NS", "Entity");

            //  Act
            bool result = update.Load(EdmCoreModel.Instance, entityType);

            // Assert
            Assert.False(result);
            Assert.True(update.IsUpdatable);
            Assert.Null(update.Updatable);
            Assert.Null(update.NonUpdatableNavigationProperties);
        }

        [Theory]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.Inline)]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.OutOfLine)]
        public void TargetOnEntityTypeReturnsCorrectUpdateRestrictionsValue(EdmVocabularyAnnotationSerializationLocation location)
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
            UpdateRestrictions update = new UpdateRestrictions();
            bool result = update.Load(model, calendar);

            // Assert
            Assert.True(result);
            VerifyUpdateRestrictions(update);
        }

        [Theory]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.Inline)]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.OutOfLine)]
        public void TargetOnEntitySetReturnsCorrectUpdateRestrictionsValue(EdmVocabularyAnnotationSerializationLocation location)
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
            UpdateRestrictions update = new UpdateRestrictions();
            bool result = update.Load(model, calendars);

            // Assert
            Assert.True(result);
            VerifyUpdateRestrictions(update);
        }

        private static IEdmModel GetEdmModel(string template, EdmVocabularyAnnotationSerializationLocation location)
        {
            string countAnnotation = @"
                <Annotation Term=""Org.OData.Capabilities.V1.UpdateRestrictions"" >
                  <Record>
                    <PropertyValue Property=""Updatable"" Bool=""false"" />
                    <PropertyValue Property=""NonUpdatableNavigationProperties"" >
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

        private static void VerifyUpdateRestrictions(UpdateRestrictions update)
        {
            Assert.NotNull(update);

            Assert.NotNull(update.Updatable);
            Assert.False(update.Updatable.Value);

            Assert.NotNull(update.NonUpdatableNavigationProperties);
            Assert.Equal(2, update.NonUpdatableNavigationProperties.Count);
            Assert.Equal("abc|RelatedEvents", String.Join("|", update.NonUpdatableNavigationProperties));

            Assert.True(update.IsNonUpdatableNavigationProperty("abc"));
            Assert.True(update.IsNonUpdatableNavigationProperty("RelatedEvents"));
            Assert.False(update.IsNonUpdatableNavigationProperty("Others"));
        }
    }
}
