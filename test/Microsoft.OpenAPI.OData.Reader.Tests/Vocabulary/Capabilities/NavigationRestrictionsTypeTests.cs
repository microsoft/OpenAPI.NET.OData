// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;
using Xunit;

namespace Microsoft.OpenApi.OData.Reader.Vocabulary.Capabilities.Tests
{
    public class NavigationRestrictionsTypeTests
    {
        [Fact]
        public void TermAttributeAttachedOnNavigationRestrictionsType()
        {
            // Arrange & Act
            string qualifiedName = Utils.GetTermQualifiedName<NavigationRestrictionsType>();

            // Assert
            Assert.Equal("Org.OData.Capabilities.V1.NavigationRestrictions", qualifiedName);
        }

        [Fact]
        public void InitializNavigationRestrictionsTypeWithRecordSuccess()
        {
            // Assert
            EdmModel model = new EdmModel();
            IEdmEnumType navigationType = model.FindType("Org.OData.Capabilities.V1.NavigationType") as IEdmEnumType;
            Assert.NotNull(navigationType);

            IEdmRecordExpression restriction1 = new EdmRecordExpression(
                new EdmPropertyConstructor("NavigationProperty", new EdmNavigationPropertyPathExpression("abc")),
                new EdmPropertyConstructor("Navigability", new EdmEnumMemberExpression(navigationType.Members.First(c => c.Name == "Single"))),
                new EdmPropertyConstructor("SkipSupported", new EdmBooleanConstant(false)));

            IEdmRecordExpression restriction2 = new EdmRecordExpression(
                new EdmPropertyConstructor("NavigationProperty", new EdmNavigationPropertyPathExpression("xyz")),
                new EdmPropertyConstructor("Navigability", new EdmEnumMemberExpression(navigationType.Members.First(c => c.Name == "None"))),
                new EdmPropertyConstructor("OptimisticConcurrencyControl", new EdmBooleanConstant(true)));

            IEdmRecordExpression record = new EdmRecordExpression(
                new EdmPropertyConstructor("Navigability", new EdmEnumMemberExpression(navigationType.Members.First(c => c.Name == "Recursive"))),
                new EdmPropertyConstructor("RestrictedProperties", new EdmCollectionExpression(restriction1, restriction2))
            );

            // Act
            NavigationRestrictionsType navigation = new NavigationRestrictionsType();
            navigation.Initialize(record);

            // Assert
            VerifyNavigationRestrictions(navigation);
        }

        [Theory]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.Inline)]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.OutOfLine)]
        public void TargetOnEntityTypeReturnsCorrectNavigationRestrictionsValue(EdmVocabularyAnnotationSerializationLocation location)
        {
            // Arrange
            const string template = @"
                <Annotations Target=""NS.Calendar"">
                  {0}
                </Annotations>";

            IEdmModel model = GetEdmModel(template, location);
            Assert.NotNull(model); // guard

            IEdmEntitySet calendars = model.EntityContainer.FindEntitySet("Calendars");

            // Act
            NavigationRestrictionsType navigation = model.GetRecord<NavigationRestrictionsType>(calendars);

            // Assert
            VerifyNavigationRestrictions(navigation);
        }

        [Theory]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.Inline)]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.OutOfLine)]
        public void TargetOnEntitySetReturnsCorrectNavigationRestrictionsValue(EdmVocabularyAnnotationSerializationLocation location)
        {
            // Arrange
            const string template = @"
                <Annotations Target=""NS.Default/Calendars"">
                  {0}
                </Annotations>";

            IEdmModel model = GetEdmModel(template, location);
            Assert.NotNull(model); // guard

            IEdmEntitySet calendars = model.EntityContainer.FindEntitySet("Calendars");

            // Act
            NavigationRestrictionsType navigation = model.GetRecord<NavigationRestrictionsType>(calendars);

            // Assert
            VerifyNavigationRestrictions(navigation);

            NavigationPropertyRestriction navRestriction = navigation.RestrictedProperties.Last();
            Assert.NotNull(navRestriction.Navigability);
            Assert.Equal(NavigationType.None, navRestriction.Navigability.Value);
            Assert.Equal("xyz", navRestriction.NavigationProperty);
            Assert.True(navigation.IsRestrictedProperty("xyz"));
        }

        private static IEdmModel GetEdmModel(string template, EdmVocabularyAnnotationSerializationLocation location)
        {
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
                          <PropertyValue Property=""SkipSupported"" Bool=""false"" />
                         </Record>
                         <Record>
                          <PropertyValue Property=""Navigability"" >
                            <EnumMember>Org.OData.Capabilities.V1.NavigationType/None</EnumMember>
                          </PropertyValue>
                          <PropertyValue Property=""NavigationProperty"" NavigationPropertyPath=""xyz"" />
                          <PropertyValue Property=""OptimisticConcurrencyControl"" Bool=""true"" />
                         </Record>
                       </Collection>
                     </PropertyValue>
                   </Record>
                 </Annotation>";

            if (location == EdmVocabularyAnnotationSerializationLocation.OutOfLine)
            {
                navigationAnnotation = string.Format(template, navigationAnnotation);
                return CapabilitiesModelHelper.GetEdmModelOutline(navigationAnnotation);
            }
            else
            {
                return CapabilitiesModelHelper.GetEdmModelTypeInline(navigationAnnotation);
            }
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
            NavigationRestrictionsType navigation = model.GetRecord<NavigationRestrictionsType>(calendar);

            // Assert
            Assert.Null(navigation.Navigability);
            Assert.Null(navigation.RestrictedProperties);
        }

        private static void VerifyNavigationRestrictions(NavigationRestrictionsType navigation)
        {
            Assert.NotNull(navigation);
            Assert.True(navigation.IsNavigable);

            Assert.NotNull(navigation.Navigability);
            Assert.Equal(NavigationType.Recursive, navigation.Navigability.Value);

            Assert.NotNull(navigation.RestrictedProperties);
            Assert.Equal(2, navigation.RestrictedProperties.Count);

            Assert.False(navigation.IsRestrictedProperty("abc"));
            Assert.True(navigation.IsRestrictedProperty("xyz"));

            // #1
            NavigationPropertyRestriction navRestriction = navigation.RestrictedProperties.First();
            Assert.NotNull(navRestriction.Navigability);
            Assert.Equal(NavigationType.Single, navRestriction.Navigability.Value);
            Assert.Equal("abc", navRestriction.NavigationProperty);

            Assert.NotNull(navRestriction.SkipSupported);
            Assert.False(navRestriction.SkipSupported.Value);

            Assert.Null(navRestriction.TopSupported);
            Assert.Null(navRestriction.OptimisticConcurrencyControl);

            // #2
            navRestriction = navigation.RestrictedProperties.Last();
            Assert.NotNull(navRestriction.Navigability);
            Assert.Equal(NavigationType.None, navRestriction.Navigability.Value);
            Assert.Equal("xyz", navRestriction.NavigationProperty);

            Assert.Null(navRestriction.SkipSupported);
            Assert.Null(navRestriction.TopSupported);

            Assert.NotNull(navRestriction.OptimisticConcurrencyControl);
            Assert.True(navRestriction.OptimisticConcurrencyControl.Value);
        }
    }
}
