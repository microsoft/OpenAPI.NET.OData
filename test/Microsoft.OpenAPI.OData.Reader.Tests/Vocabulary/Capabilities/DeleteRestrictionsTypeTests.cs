// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;
using Xunit;

namespace Microsoft.OpenApi.OData.Reader.Vocabulary.Capabilities.Tests
{
    public class DeleteRestrictionsTypeTests
    {
        [Fact]
        public void TermAttributeAttachedOnDeleteRestrictionsType()
        {
            // Arrange & Act
            string qualifiedName = Utils.GetTermQualifiedName<DeleteRestrictionsType>();

            // Assert
            Assert.Equal("Org.OData.Capabilities.V1.DeleteRestrictions", qualifiedName);
        }

        [Fact]
        public void InitializeDeleteRestrictionsTypeWithRecordSuccess()
        {
            // Assert
            IEdmRecordExpression record = new EdmRecordExpression(
                new EdmPropertyConstructor("Deletable", new EdmBooleanConstant(false)),
                new EdmPropertyConstructor("NonDeletableNavigationProperties",
                    new EdmCollectionExpression(new EdmNavigationPropertyPathExpression("abc"), new EdmNavigationPropertyPathExpression("RelatedEvents"))),
                new EdmPropertyConstructor("MaxLevels", new EdmIntegerConstant(42)),
                new EdmPropertyConstructor("Permission", new EdmRecordExpression(
                    new EdmPropertyConstructor("Scheme", new EdmRecordExpression(new EdmPropertyConstructor("Authorization", new EdmStringConstant("schemeName")))))),
                new EdmPropertyConstructor("CustomQueryOptions", new EdmCollectionExpression(
                    new EdmRecordExpression(
                        new EdmPropertyConstructor("Name", new EdmStringConstant("odata-debug")),
                        new EdmPropertyConstructor("DocumentationURL", new EdmStringConstant("https://debug.html")))))
                // CustomHeaders
            );

            // Act
            DeleteRestrictionsType delete = new DeleteRestrictionsType();
            delete.Initialize(record);

            // Assert
            VerifyDeleteRestrictionsType(delete);
        }

        [Theory]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.Inline)]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.OutOfLine)]
        public void TargetOnEntityTypeReturnsCorrectDeleteRestrictionsValue(EdmVocabularyAnnotationSerializationLocation location)
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
            DeleteRestrictionsType delete = model.GetRecord<DeleteRestrictionsType>(calendars);

            // Assert
            VerifyDeleteRestrictionsType(delete);
        }

        [Theory]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.Inline)]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.OutOfLine)]
        public void TargetOnEntitySetReturnsCorrectDeleteRestrictionsValue(EdmVocabularyAnnotationSerializationLocation location)
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
            DeleteRestrictionsType delete = model.GetRecord<DeleteRestrictionsType>(calendars);

            // Assert
            VerifyDeleteRestrictionsType(delete);
        }

        private static IEdmModel GetEdmModel(string template, EdmVocabularyAnnotationSerializationLocation location)
        {
            string annotation = @"
                <Annotation Term=""Org.OData.Capabilities.V1.DeleteRestrictions"" >
                  <Record>
                    <PropertyValue Property=""Deletable"" Bool=""false"" />
                    <PropertyValue Property=""NonDeletableNavigationProperties"" >
                      <Collection>
                        <NavigationPropertyPath>abc</NavigationPropertyPath>
                        <NavigationPropertyPath>RelatedEvents</NavigationPropertyPath>
                      </Collection>
                    </PropertyValue>
                    <PropertyValue Property=""MaxLevels"" Int=""42"" />
                    <PropertyValue Property=""Permission"">
                      <Record>
                        <PropertyValue Property=""Scheme"" >
                          <Record>
                              <PropertyValue Property=""Authorization"" String=""schemeName"" />
                          </Record>
                        </PropertyValue>
                      </Record>
                    </PropertyValue>
                    <PropertyValue Property=""CustomQueryOptions"" >
                      <Collection>
                        <Record>
                          <PropertyValue Property=""Name"" String=""odata-debug"" />
                          <PropertyValue Property=""DocumentationURL"" String=""https://debug.html"" />
                      </Record>
                      </Collection>
                    </PropertyValue>
                  </Record>
                </Annotation>";

            if (location == EdmVocabularyAnnotationSerializationLocation.OutOfLine)
            {
                annotation = string.Format(template, annotation);
                return CapabilitiesModelHelper.GetEdmModelOutline(annotation);
            }
            else
            {
                return CapabilitiesModelHelper.GetEdmModelTypeInline(annotation);
            }
        }

        private static void VerifyDeleteRestrictionsType(DeleteRestrictionsType delete)
        {
            Assert.NotNull(delete);

            Assert.NotNull(delete.Deletable);
            Assert.False(delete.Deletable.Value);

            Assert.NotNull(delete.NonDeletableNavigationProperties);
            Assert.Equal(2, delete.NonDeletableNavigationProperties.Count);
            Assert.Equal("abc|RelatedEvents", String.Join("|", delete.NonDeletableNavigationProperties));

            Assert.True(delete.IsNonDeletableNavigationProperty("RelatedEvents"));

            Assert.NotNull(delete.Permission);
            Assert.Equal("schemeName", delete.Permission.Scheme.Authorization);

            Assert.Null(delete.CustomHeaders);

            Assert.NotNull(delete.CustomQueryOptions);
            CustomParameter parameter = Assert.Single(delete.CustomQueryOptions);
            Assert.Equal("odata-debug", parameter.Name);
            Assert.Equal("https://debug.html", parameter.DocumentationURL);
        }
    }
}
