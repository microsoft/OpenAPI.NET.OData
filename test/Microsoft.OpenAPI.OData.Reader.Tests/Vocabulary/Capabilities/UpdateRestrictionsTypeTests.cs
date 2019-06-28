// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;
using Xunit;

namespace Microsoft.OpenApi.OData.Reader.Vocabulary.Capabilities.Tests
{
    public class UpdateRestrictionsTypeTests
    {
        [Fact]
        public void TermAttributeAttachedOnUpdateRestrictionsType()
        {
            // Arrange & Act
            string qualifiedName = Utils.GetTermQualifiedName<UpdateRestrictionsType>();

            // Assert
            Assert.Equal("Org.OData.Capabilities.V1.UpdateRestrictions", qualifiedName);
        }

        [Fact]
        public void UnknownAnnotatableTargetReturnsDefaultUpdateRestrictionsValues()
        {
            // Arrange
            EdmEntityType entityType = new EdmEntityType("NS", "Entity");

            //  Act
            UpdateRestrictionsType update = EdmCoreModel.Instance.GetRecord<UpdateRestrictionsType>(entityType);

            // Assert
            Assert.Null(update);
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
            UpdateRestrictionsType update = model.GetRecord<UpdateRestrictionsType>(calendar);

            // Assert
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
            UpdateRestrictionsType update = model.GetRecord<UpdateRestrictionsType>(calendars);

            // Assert
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
                    <PropertyValue Property=""MaxLevels"" Int=""8"" />
                    <PropertyValue Property=""Permission"">
                      <Record Type=""Org.OData.Capabilities.V1.PermissionType"">
                        <PropertyValue Property=""Scheme"">
                          <Record Type=""Org.OData.Authorization.V1.SecurityScheme"">
                            <PropertyValue Property=""Authorization"" String=""authorizationName"" />
                            <PropertyValue Property=""RequiredScopes"">
                              <Collection>
                                <String>RequiredScopes1</String>
                                <String>RequiredScopes2</String>
                              </Collection>
                            </PropertyValue>
                          </Record>
                        </PropertyValue>
                        <PropertyValue Property=""Scopes"">
                          <Collection>
                            <Record Type=""Org.OData.Capabilities.V1.ScopeType"">
                              <PropertyValue Property=""Scope"" String=""scopeName1"" />
                              <PropertyValue Property=""RestrictedProperties"" String=""p1,p2"" />
                            </Record>
                            <Record Type=""Org.OData.Capabilities.V1.ScopeType"">
                              <PropertyValue Property=""Scope"" String=""scopeName2"" />
                              <PropertyValue Property=""RestrictedProperties"" String=""p3,p4"" />
                            </Record>
                          </Collection>
                        </PropertyValue>
                      </Record>
                    </PropertyValue>
                    <PropertyValue Property=""QueryOptions"">
                      <Record>
                        <PropertyValue Property=""ExpandSupported"" Bool=""true"" />
                        <PropertyValue Property=""SelectSupported"" Bool=""true"" />
                        <PropertyValue Property=""ComputeSupported"" Bool=""true"" />
                        <PropertyValue Property=""FilterSupported"" Bool=""true"" />
                        <PropertyValue Property=""SearchSupported"" Bool=""true"" />
                        <PropertyValue Property=""SortSupported"" Bool=""false"" />
                        <PropertyValue Property=""SortSupported"" Bool=""false"" />
                      </Record>
                    </PropertyValue>
                    <PropertyValue Property=""CustomHeaders"">
                      <Collection>
                        <Record>
                          <PropertyValue Property=""Name"" String=""HeadName1"" />
                          <PropertyValue Property=""Description"" String=""Description1"" />
                          <PropertyValue Property=""ComputeSupported"" String=""http://any1"" />
                          <PropertyValue Property=""Required"" Bool=""true"" />
                          <PropertyValue Property=""ExampleValues"">
                            <Collection>
                              <Record>
                                <PropertyValue Property=""Description"" String=""Description23"" />
                                <PropertyValue Property=""Value"" String=""value1"" />
                              </Record>
                            </Collection>
                          </PropertyValue>
                        </Record>
                        <Record>
                          <PropertyValue Property=""Name"" String=""HeadName2"" />
                          <PropertyValue Property=""Description"" String=""Description2"" />
                          <PropertyValue Property=""ComputeSupported"" String=""http://any2"" />
                          <PropertyValue Property=""Required"" Bool=""false"" />
                          <PropertyValue Property=""ExampleValues"">
                            <Collection>
                              <Record>
                                <PropertyValue Property=""Description"" String=""Description23"" />
                                <PropertyValue Property=""Value"" String=""value2"" />
                              </Record>
                            </Collection>
                          </PropertyValue>
                        </Record>
                      </Collection>
                    </PropertyValue>
                    <PropertyValue Property=""CustomQueryOptions"">
                      <Collection>
                        <Record>
                          <PropertyValue Property=""Name"" String=""HeadName1"" />
                          <PropertyValue Property=""Description"" String=""Description1"" />
                          <PropertyValue Property=""ComputeSupported"" String=""http://any3"" />
                          <PropertyValue Property=""Required"" Bool=""true"" />
                          <PropertyValue Property=""ExampleValues"">
                            <Collection>
                              <Record>
                                <PropertyValue Property=""Description"" String=""Description23"" />
                                <PropertyValue Property=""Value"" String=""value3"" />
                              </Record>
                            </Collection>
                          </PropertyValue>
                        </Record>
                        <Record>
                          <PropertyValue Property=""Name"" String=""HeadName2"" />
                          <PropertyValue Property=""Description"" String=""Description2"" />
                          <PropertyValue Property=""ComputeSupported"" String=""http://any4"" />
                          <PropertyValue Property=""Required"" Bool=""false"" />
                          <PropertyValue Property=""ExampleValues"">
                            <Collection>
                              <Record>
                                <PropertyValue Property=""Description"" String=""Description23"" />
                                <PropertyValue Property=""Value"" String=""value4"" />
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

        private static void VerifyUpdateRestrictions(UpdateRestrictionsType update)
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

            // MaxLevels
            Assert.NotNull(update.MaxLevels);
            Assert.Equal(8, update.MaxLevels);

            // QueryOptions
            Assert.NotNull(update.QueryOptions);
            Assert.True(update.QueryOptions.ComputeSupported);
            Assert.False(update.QueryOptions.SortSupported);

            // CustomHeaders
            Assert.NotNull(update.CustomHeaders);
            Assert.Equal(2, update.CustomHeaders.Count);

            // CustomQueryOptions
            Assert.NotNull(update.CustomQueryOptions);
            Assert.Equal(2, update.CustomQueryOptions.Count);
        }
    }
}
