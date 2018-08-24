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
    public class ExpandRestrictionsTests
    {
        [Fact]
        public void KindPropertyReturnsExpandRestrictionsEnumMember()
        {
            // Arrange & Act
            ExpandRestrictions expand = new ExpandRestrictions();

            // Assert
            Assert.Equal(CapabilitesTermKind.ExpandRestrictions, expand.Kind);
        }

        [Fact]
        public void UnknownAnnotatableTargetReturnsDefaultExpandRestrictionsValues()
        {
            // Arrange
            ExpandRestrictions expand = new ExpandRestrictions();
            EdmEntityType entityType = new EdmEntityType("NS", "Entity");

            //  Act
            bool result = expand.Load(EdmCoreModel.Instance, entityType);

            // Assert
            Assert.False(result);
            Assert.Equal(CapabilitesTermKind.ExpandRestrictions, expand.Kind);
            Assert.True(expand.IsExpandable);
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

            IEdmEntitySet calendars = model.EntityContainer.FindEntitySet("Calendars");
            Assert.NotNull(calendars); // guard

            // Act
            ExpandRestrictions expand = new ExpandRestrictions();
            bool result = expand.Load(model, calendars);

            // Assert
            Assert.True(result);
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
            // Act
            ExpandRestrictions expand = new ExpandRestrictions();
            bool result = expand.Load(model, calendars);

            // Assert
            Assert.True(result);
            VerifyExpandRestrictions(expand);
        }

        private static IEdmModel GetEdmModel(string template, EdmVocabularyAnnotationSerializationLocation location)
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
                return CapabilitiesModelHelper.GetEdmModelTypeInline(countAnnotation);
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

            Assert.True(expand.IsNonExpandableProperty("RelatedEvents"));
        }
    }
}
