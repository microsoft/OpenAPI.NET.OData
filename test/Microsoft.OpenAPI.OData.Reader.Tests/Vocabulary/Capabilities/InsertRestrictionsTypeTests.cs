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
using Microsoft.OpenApi.OData.Vocabulary.Core;
using Xunit;

namespace Microsoft.OpenApi.OData.Reader.Vocabulary.Capabilities.Tests
{
    public class InsertRestrictionsTypeTests
    {
        [Fact]
        public void TermAttributeAttachedOnInsertRestrictionsType()
        {
            // Arrange & Act
            string qualifiedName = Utils.GetTermQualifiedName<InsertRestrictionsType>();

            // Assert
            Assert.Equal("Org.OData.Capabilities.V1.InsertRestrictions", qualifiedName);
        }

        [Fact]
        public void InitializInsertRestrictionsTypeWithRecordSuccess()
        {
            // Assert
            IEdmRecordExpression primitiveExampleValue = new EdmRecordExpression(
                new EdmPropertyConstructor("Description", new EdmStringConstant("example desc")),
                new EdmPropertyConstructor("Value", new EdmStringConstant("example value")));

            IEdmRecordExpression record = new EdmRecordExpression(
                    new EdmPropertyConstructor("Insertable", new EdmBooleanConstant(false)),
                    new EdmPropertyConstructor("NonInsertableProperties", new EdmCollectionExpression(new EdmPathExpression("abc/xyz"))),
                    new EdmPropertyConstructor("NonInsertableNavigationProperties", new EdmCollectionExpression(new EdmNavigationPropertyPathExpression("abc"), new EdmNavigationPropertyPathExpression("RelatedEvents"))),
                    new EdmPropertyConstructor("MaxLevels", new EdmIntegerConstant(8)),
                    new EdmPropertyConstructor("CustomQueryOptions", new EdmCollectionExpression(
                        new EdmRecordExpression(
                            new EdmPropertyConstructor("Name", new EdmStringConstant("primitive name")),
                            new EdmPropertyConstructor("Description", new EdmStringConstant("primitive desc")),
                            new EdmPropertyConstructor("DocumentationURL", new EdmStringConstant("http://any3")),
                            new EdmPropertyConstructor("Required", new EdmBooleanConstant(true)),
                            new EdmPropertyConstructor("ExampleValues", new EdmCollectionExpression(primitiveExampleValue)))))
                    // QueryOptions
                    // Permission
                    // CustomHeaders
                    );

            // Act
            InsertRestrictionsType insert = new InsertRestrictionsType();
            insert.Initialize(record);

            // Assert
            VerifyInsertRestrictions(insert);
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
            InsertRestrictionsType insert = model.GetRecord<InsertRestrictionsType>(calendars);

            // Assert
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
            InsertRestrictionsType insert = model.GetRecord<InsertRestrictionsType>(calendars);

            // Assert
            VerifyInsertRestrictions(insert);
        }

        private static IEdmModel GetEdmModel(string template, EdmVocabularyAnnotationSerializationLocation location)
        {
            string countAnnotation = @"
                <Annotation Term=""Org.OData.Capabilities.V1.InsertRestrictions"" >
                  <Record>
                    <PropertyValue Property=""Insertable"" Bool=""false"" />
                    <PropertyValue Property=""NonInsertableProperties"" >
                      <Collection>
                        <PropertyPath>abc/xyz</PropertyPath>
                      </Collection>
                    </PropertyValue>
                    <PropertyValue Property=""NonInsertableNavigationProperties"" >
                      <Collection>
                        <NavigationPropertyPath>abc</NavigationPropertyPath>
                        <NavigationPropertyPath>RelatedEvents</NavigationPropertyPath>
                      </Collection>
                    </PropertyValue>
                    <PropertyValue Property=""MaxLevels"" Int=""8"" />
                    <PropertyValue Property=""CustomQueryOptions"" >
                      <Collection>
                        <Record>
                          <PropertyValue Property=""Name"" String=""primitive name"" />
                          <PropertyValue Property=""Description"" String=""primitive desc"" />
                          <PropertyValue Property=""DocumentationURL"" String=""http://any3"" />
                          <PropertyValue Property=""Required"" Bool=""true"" />
                          <PropertyValue Property=""ExampleValues"">
                            <Collection>
                              <Record>
                                <PropertyValue Property=""Description"" String=""example desc"" />
                                <PropertyValue Property=""Value"" String=""example value"" />
                              </Record>
                            </Collection>
                          </PropertyValue>
                        </Record>
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

        private static void VerifyInsertRestrictions(InsertRestrictionsType insert)
        {
            Assert.NotNull(insert);

            Assert.NotNull(insert.Insertable);
            Assert.False(insert.Insertable.Value);

            Assert.NotNull(insert.NonInsertableNavigationProperties);
            Assert.Equal(2, insert.NonInsertableNavigationProperties.Count);
            Assert.Equal("abc|RelatedEvents", String.Join("|", insert.NonInsertableNavigationProperties));

            Assert.True(insert.IsNonInsertableNavigationProperty("RelatedEvents"));
            Assert.False(insert.IsNonInsertableNavigationProperty("MyUnknownNavigationProperty"));

            Assert.Null(insert.QueryOptions);
            Assert.Null(insert.Permissions);
            Assert.Null(insert.CustomHeaders);

            Assert.NotNull(insert.MaxLevels);
            Assert.Equal(8, insert.MaxLevels.Value);

            Assert.NotNull(insert.CustomQueryOptions);
            CustomParameter parameter = Assert.Single(insert.CustomQueryOptions);
            Assert.Equal("primitive name", parameter.Name);
            Assert.Equal("http://any3", parameter.DocumentationURL);

            Assert.NotNull(parameter.ExampleValues);
            PrimitiveExampleValue example = Assert.Single(parameter.ExampleValues);
            Assert.Equal("example desc", example.Description);
            Assert.Equal("example value", example.Value.Value);
        }
    }
}
