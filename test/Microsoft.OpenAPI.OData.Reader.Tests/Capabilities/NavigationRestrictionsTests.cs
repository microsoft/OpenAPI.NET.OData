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
    public class NavigationRestrictionsTests
    {
        [Fact]
        public void UnknownAnnotatableTargetReturnsDefaultPropertyValues()
        {
            // Arrange
            EdmEntityType entityType = new EdmEntityType("NS", "Entity");

            // Act
            NavigationRestrictions navigation = new NavigationRestrictions(EdmCoreModel.Instance, entityType);

            // Assert
            Assert.Equal(CapabilitiesConstants.NavigationRestrictions, navigation.QualifiedName);
            Assert.Null(navigation.Navigability);
            Assert.Null(navigation.RestrictedProperties);
        }

        [Theory]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.Inline)]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.OutOfLine)]
        public void TargetOnEntityTypeReturnsCorrectNavigationRestrictionsValue(EdmVocabularyAnnotationSerializationLocation location)
        {
            // Arrange
            const string outOfLineTemplate = @"
                <Annotations Target=""NS.Calendar"">
                  {0}
                </Annotations>";

            string navigationAnnotation = @"
                <Annotation Term=""Org.OData.Capabilities.V1.NavigationRestrictions"" >
                  <Record>
                    <PropertyValue Property=""Navigability"" >
                      <EnumMember>Org.OData.Capabilities.V1.NavigationType/Recursive</EnumMember>
                    </PropertyValue>
                    <PropertyValue Property=""RestrictedProperties"" >
                      <Collection>
                        <Record>
                          <PropertyValue Property=""Navigability"" >
                            <EnumMember>Org.OData.Capabilities.V1.NavigationType/Single</EnumMember>
                          </PropertyValue>
                          <PropertyValue Property=""NavigationProperty"" NavigationPropertyPath=""abc"" />
                         </Record>
                       </Collection>
                     </PropertyValue>
                   </Record>
                 </Annotation>";

            IEdmModel model;
            if (location == EdmVocabularyAnnotationSerializationLocation.OutOfLine)
            {
                navigationAnnotation = string.Format(outOfLineTemplate, navigationAnnotation);
                model = CapabilitiesModelHelper.GetEdmModelOutline(navigationAnnotation);
            }
            else
            {
                model = CapabilitiesModelHelper.GetEdmModelTypeInline(navigationAnnotation);
            }

            IEdmEntityType calendar = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Calendar");

            // Act
            NavigationRestrictions navigation = new NavigationRestrictions(model, calendar);

            // Assert
            Assert.NotNull(navigation.Navigability);
            Assert.Equal(NavigationType.Recursive, navigation.Navigability.Value);

            Assert.NotNull(navigation.RestrictedProperties);
            NavigationPropertyRestriction navRestriction = Assert.Single(navigation.RestrictedProperties);
            Assert.NotNull(navRestriction.Navigability);
            Assert.Equal(NavigationType.Single, navRestriction.Navigability.Value);
            Assert.Equal("abc", navRestriction.NavigationProperty);
        }

        [Theory]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.Inline)]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.OutOfLine)]
        public void TargetOnEntitySetReturnsCorrectNavigationRestrictionsValue(EdmVocabularyAnnotationSerializationLocation location)
        {
            // Arrange
            const string outOfLineTemplate = @"
                <Annotations Target=""NS.Default/Calendars"">
                  {0}
                </Annotations>";

            string navigationAnnotation = @"
                <Annotation Term=""Org.OData.Capabilities.V1.NavigationRestrictions"" >
                  <Record>
                    <PropertyValue Property=""Navigability"" >
                      <EnumMember>Org.OData.Capabilities.V1.NavigationType/Recursive</EnumMember>
                    </PropertyValue>
                    <PropertyValue Property=""RestrictedProperties"" >
                      <Collection>
                        <Record>
                          <PropertyValue Property=""Navigability"" >
                            <EnumMember>Org.OData.Capabilities.V1.NavigationType/Single</EnumMember>
                          </PropertyValue>
                          <PropertyValue Property=""NavigationProperty"" NavigationPropertyPath=""abc"" />
                         </Record>
                         <Record>
                          <PropertyValue Property=""Navigability"" >
                            <EnumMember>Org.OData.Capabilities.V1.NavigationType/None</EnumMember>
                          </PropertyValue>
                          <PropertyValue Property=""NavigationProperty"" NavigationPropertyPath=""xyz"" />
                         </Record>
                       </Collection>
                     </PropertyValue>
                   </Record>
                 </Annotation>";

            IEdmModel model;
            if (location == EdmVocabularyAnnotationSerializationLocation.OutOfLine)
            {
                navigationAnnotation = string.Format(outOfLineTemplate, navigationAnnotation);
                model = CapabilitiesModelHelper.GetEdmModelOutline(navigationAnnotation);
            }
            else
            {
                model = CapabilitiesModelHelper.GetEdmModelTypeInline(navigationAnnotation);
            }

            IEdmEntitySet calendars = model.EntityContainer.FindEntitySet("Calendars");

            // Act
            NavigationRestrictions navigation = new NavigationRestrictions(model, calendars);

            // Assert
            Assert.NotNull(navigation.Navigability);
            Assert.Equal(NavigationType.Recursive, navigation.Navigability.Value);

            Assert.NotNull(navigation.RestrictedProperties);
            Assert.Equal(2, navigation.RestrictedProperties.Count);

            NavigationPropertyRestriction navRestriction = navigation.RestrictedProperties.First();
            Assert.NotNull(navRestriction.Navigability);
            Assert.Equal(NavigationType.Single, navRestriction.Navigability.Value);
            Assert.Equal("abc", navRestriction.NavigationProperty);

            navRestriction = navigation.RestrictedProperties.Last();
            Assert.NotNull(navRestriction.Navigability);
            Assert.Equal(NavigationType.None, navRestriction.Navigability.Value);
            Assert.Equal("xyz", navRestriction.NavigationProperty);
        }

        [Theory]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.Inline)]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.OutOfLine)]
        public void TargetOnNavigationPropertyReturnsCorrectNavigationRestrictionsValue(EdmVocabularyAnnotationSerializationLocation location)
        {
            // Arrange
            const string outOfLineTemplate = @"
                <Annotations Target=""NS.Calendar/RelatedEvents"">
                  {0}
                </Annotations>";

            string navigationAnnotation = @"
                <Annotation Term=""Org.OData.Capabilities.V1.NavigationRestrictions"" >
                  <Record>
                    <PropertyValue Property=""Navigability"" >
                      <EnumMember>Org.OData.Capabilities.V1.NavigationType/Recursive</EnumMember>
                    </PropertyValue>
                   </Record>
                 </Annotation>";

            IEdmModel model;
            if (location == EdmVocabularyAnnotationSerializationLocation.OutOfLine)
            {
                navigationAnnotation = string.Format(outOfLineTemplate, navigationAnnotation);
                model = CapabilitiesModelHelper.GetEdmModelOutline(navigationAnnotation);
            }
            else
            {
                model = CapabilitiesModelHelper.GetEdmModelNavInline(navigationAnnotation);
            }

            IEdmEntityType calendar = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Calendar");
            IEdmNavigationProperty navigationProperty = calendar.DeclaredNavigationProperties().First(c => c.Name == "RelatedEvents");

            // Act
            NavigationRestrictions navigation = new NavigationRestrictions(model, navigationProperty);

            // Assert
            Assert.NotNull(navigation.Navigability);
            Assert.Equal(NavigationType.Recursive, navigation.Navigability.Value);
            Assert.Null(navigation.RestrictedProperties);
        }

        [Theory]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.Inline)]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.OutOfLine)]
        public void IsRestrictedPropertyReturnsCorrectForNavigationProperty(EdmVocabularyAnnotationSerializationLocation location)
        {
            // Arrange
            const string outOfLineTemplate = @"
                <Annotations Target=""NS.Calendar"">
                  {0}
                </Annotations>";

            string navigationAnnotation = @"
                <Annotation Term=""Org.OData.Capabilities.V1.NavigationRestrictions"" >
                  <Record>
                    <PropertyValue Property=""Navigability"" >
                      <EnumMember>Org.OData.Capabilities.V1.NavigationType/Recursive</EnumMember>
                    </PropertyValue>
                    <PropertyValue Property=""RestrictedProperties"" >
                      <Collection>
                        <Record>
                          <PropertyValue Property=""Navigability"" >
                            <EnumMember>Org.OData.Capabilities.V1.NavigationType/None</EnumMember>
                          </PropertyValue>
                          <PropertyValue Property=""NavigationProperty"" NavigationPropertyPath=""RelatedEvents"" />
                         </Record>
                       </Collection>
                     </PropertyValue>
                   </Record>
                 </Annotation>";

            IEdmModel model;
            if (location == EdmVocabularyAnnotationSerializationLocation.OutOfLine)
            {
                navigationAnnotation = string.Format(outOfLineTemplate, navigationAnnotation);
                model = CapabilitiesModelHelper.GetEdmModelOutline(navigationAnnotation);
            }
            else
            {
                model = CapabilitiesModelHelper.GetEdmModelTypeInline(navigationAnnotation);
            }

            IEdmEntityType calendar = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Calendar");
            IEdmNavigationProperty navigationProperty = calendar.DeclaredNavigationProperties().First(c => c.Name == "RelatedEvents");

            // Act
            NavigationRestrictions navigation = new NavigationRestrictions(model, calendar);
            Assert.NotNull(navigation.Navigability); // Guard

            bool result = navigation.IsRestrictedProperty(navigationProperty);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void TargetWithUnknownEnumMemberDoesnotReturnsNavigationRestrictionsValue()
        {
            // Arrange
            const string navigationAnnotation = @"
              <Annotations Target=""NS.Calendar"" >
                  <Annotation Term=""Org.OData.Capabilities.V1.NavigationRestrictions"">
                    <Record>
                      <PropertyValue Property=""Navigability"" >
                        <EnumMember>Org.OData.Capabilities.V1.NavigationType/Unknown</EnumMember>
                    </PropertyValue>
                    </Record>
                  </Annotation>
              </Annotations>";

            IEdmModel model = CapabilitiesModelHelper.GetEdmModelOutline(navigationAnnotation);
            IEdmEntityType calendar = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Calendar");

            // Act
            NavigationRestrictions navigation = new NavigationRestrictions(model, calendar);

            // Assert
            Assert.Null(navigation.Navigability);
            Assert.Null(navigation.RestrictedProperties);
        }
    }
}
