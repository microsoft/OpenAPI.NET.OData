// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;
using Xunit;

namespace Microsoft.OpenApi.OData.Reader.Vocabulary.Capabilities.Tests
{
    public class ReadRestrictionsTypeTests
    {
        [Fact]
        public void TermAttributeAttachedOnReadRestrictionsType()
        {
            // Arrange & Act
            string qualifiedName = Utils.GetTermQualifiedName<ReadRestrictionsType>();

            // Assert
            Assert.Equal("Org.OData.Capabilities.V1.ReadRestrictions", qualifiedName);
        }

        [Fact]
        public void InitializReadRestrictionsTypeWithRecordSuccess()
        {
            // Assert
            IEdmRecordExpression record = new EdmRecordExpression(
                    new EdmPropertyConstructor("Readable", new EdmBooleanConstant(false)),
                    new EdmPropertyConstructor("CustomQueryOptions", new EdmCollectionExpression(
                        new EdmRecordExpression(new EdmPropertyConstructor("Name", new EdmStringConstant("root query name"))))),
                    // Root Permission
                    // Root CustomHeaders
                    new EdmPropertyConstructor("ReadByKeyRestrictions", new EdmRecordExpression(
                        new EdmPropertyConstructor("Readable", new EdmBooleanConstant(true)),
                        new EdmPropertyConstructor("CustomHeaders", new EdmCollectionExpression(
                            new EdmRecordExpression(new EdmPropertyConstructor("Name", new EdmStringConstant("by key head name")))))
                        // ByKey Permission
                        // ByKey CustomQueryOptions
                        ))
                    );

            // Act
            ReadRestrictionsType read = new ReadRestrictionsType();
            read.Initialize(record);

            // Assert
            VerifyReadRestrictions(read);
        }

        [Theory]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.Inline)]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.OutOfLine)]
        public void TargetOnEntityTypeReturnsCorrectReadRestrictionsValue(EdmVocabularyAnnotationSerializationLocation location)
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
            ReadRestrictionsType read = model.GetRecord<ReadRestrictionsType>(calendars);

            // Assert
            VerifyReadRestrictions(read);
        }

        [Theory]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.Inline)]
        [InlineData(EdmVocabularyAnnotationSerializationLocation.OutOfLine)]
        public void TargetOnEntitySetReturnsCorrectReadRestrictionsValue(EdmVocabularyAnnotationSerializationLocation location)
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
            ReadRestrictionsType read = model.GetRecord<ReadRestrictionsType>(calendars);

            // Assert
            VerifyReadRestrictions(read);
        }

        private static IEdmModel GetEdmModel(string template, EdmVocabularyAnnotationSerializationLocation location)
        {
            string countAnnotation = @"
                <Annotation Term=""Org.OData.Capabilities.V1.ReadRestrictions"" >
                  <Record>
                    <PropertyValue Property=""Readable"" Bool=""false"" />
                    <PropertyValue Property=""CustomQueryOptions"" >
                      <Collection>
                        <Record>
                          <PropertyValue Property=""Name"" String=""root query name"" />
                        </Record>
                      </Collection>
                    </PropertyValue>
                    <PropertyValue Property=""ReadByKeyRestrictions"">
                      <Record>
                        <PropertyValue Property=""Readable"" Bool=""true"" />
                        <PropertyValue Property=""CustomHeaders"" >
                          <Collection>
                            <Record>
                              <PropertyValue Property=""Name"" String=""by key head name"" />
                            </Record>
                          </Collection>
                        </PropertyValue>
                      </Record>
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

        private static void VerifyReadRestrictions(ReadRestrictionsType read)
        {
            Assert.NotNull(read);

            Assert.NotNull(read.Readable);
            Assert.False(read.Readable.Value);

            Assert.Null(read.Permissions);
            Assert.Null(read.CustomHeaders);

            Assert.NotNull(read.CustomQueryOptions);
            CustomParameter parameter = Assert.Single(read.CustomQueryOptions);
            Assert.Equal("root query name", parameter.Name);
            Assert.Null(parameter.DocumentationURL);
            Assert.Null(parameter.ExampleValues);

            // ReadByKeyRestrictions
            Assert.NotNull(read.ReadByKeyRestrictions);
            Assert.NotNull(read.ReadByKeyRestrictions.Readable);
            Assert.True(read.ReadByKeyRestrictions.Readable.Value);

            Assert.Null(read.ReadByKeyRestrictions.Permissions);
            Assert.Null(read.ReadByKeyRestrictions.CustomQueryOptions);

            Assert.NotNull(read.ReadByKeyRestrictions.CustomHeaders);
            parameter = Assert.Single(read.ReadByKeyRestrictions.CustomHeaders);
            Assert.Equal("by key head name", parameter.Name);
            Assert.Null(parameter.DocumentationURL);
            Assert.Null(parameter.ExampleValues);
        }
    }
}
