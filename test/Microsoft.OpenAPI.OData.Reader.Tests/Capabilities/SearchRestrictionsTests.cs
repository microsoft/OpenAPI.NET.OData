// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Capabilities;
using Xunit;

namespace Microsoft.OpenApi.OData.Reader.Capabilities.Tests
{
    public class SearchRestrictionsTests
    {
        [Fact]
        public void UnknownAnnotatableTargetReturnsDefaultPropertyValues()
        {
            // Arrange
            EdmEntityType entityType = new EdmEntityType("NS", "Entity");

            // Act
            SearchRestrictions search = new SearchRestrictions(EdmCoreModel.Instance, entityType);

            // Assert
            Assert.Equal(CapabilitiesConstants.SearchRestrictions, search.QualifiedName);
            Assert.True(search.Searchable);
            Assert.Null(search.UnsupportedExpressions);
        }

        [Fact]
        public void AnnotatableTargetOnEntityTypeReturnsCorrectPropertyValue()
        {
            // Arrange
            const string searchAnnotation = @"
              <Annotations Target=""NS.Calendar"" >
                  <Annotation Term=""Org.OData.Capabilities.V1.SearchRestrictions"">
                    <Record>
                        <PropertyValue Property=""Searchable"" Bool=""false"" />
                        <PropertyValue Property=""UnsupportedExpressions"">
                            <EnumMember>Org.OData.Capabilities.V1.SearchExpressions/phrase</EnumMember>
                        </PropertyValue >
                    </Record>
                  </Annotation>
              </Annotations>";

            IEdmModel model = CapabilitiesModelHelper.GetModelInline(searchAnnotation);
            IEdmEntityType calendar = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Calendar");

            // Act
            SearchRestrictions search = new SearchRestrictions(model, calendar);

            // Assert
            Assert.False(search.Searchable);
            Assert.NotNull(search.UnsupportedExpressions);
            Assert.Equal(SearchExpressions.phrase, search.UnsupportedExpressions.Value);
        }

        [Fact]
        public void AnnotatableTargetOnEntitySetReturnsCorrectPropertyValue()
        {
            // Arrange
            const string searchAnnotation = @"
              <Annotations Target=""NS.Default/Calendars"" >
                  <Annotation Term=""Org.OData.Capabilities.V1.SearchRestrictions"">
                    <Record>
                        <PropertyValue Property=""Searchable"" Bool=""false"" />
                        <PropertyValue Property=""UnsupportedExpressions"">
                            <EnumMember>Org.OData.Capabilities.V1.SearchExpressions/group</EnumMember>
                        </PropertyValue >
                    </Record>
                  </Annotation>
              </Annotations>";

            IEdmModel model = CapabilitiesModelHelper.GetModelInline(searchAnnotation);
            IEdmEntitySet calendars = model.EntityContainer.FindEntitySet("Calendars");

            // Act
            SearchRestrictions search = new SearchRestrictions(model, calendars);

            // Assert
            Assert.False(search.Searchable);
            Assert.NotNull(search.UnsupportedExpressions);
            Assert.Equal(SearchExpressions.group, search.UnsupportedExpressions.Value);
        }

        [Fact]
        public void AnnotatableTargetOnNavigationPropertyReturnsCorrectPropertyValue()
        {
            // Arrange
            const string searchAnnotation = @"
              <Annotations Target=""NS.Calendar/RelatedEvents"" >
                  <Annotation Term=""Org.OData.Capabilities.V1.SearchRestrictions"">
                    <Record>
                        <PropertyValue Property=""Searchable"" Bool=""false"" />
                        <PropertyValue Property=""UnsupportedExpressions"">
                            <EnumMember>Org.OData.Capabilities.V1.SearchExpressions/AND</EnumMember>
                        </PropertyValue >
                    </Record>
                  </Annotation>
              </Annotations>";

            IEdmModel model = CapabilitiesModelHelper.GetModelInline(searchAnnotation);
            IEdmEntityType calendar = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Calendar");
            IEdmNavigationProperty property = calendar.DeclaredNavigationProperties().First(d => d.Name == "RelatedEvents");

            // Act
            SearchRestrictions search = new SearchRestrictions(model, property);

            // Assert
            Assert.False(search.Searchable);
            Assert.NotNull(search.UnsupportedExpressions);
            Assert.Equal(SearchExpressions.AND, search.UnsupportedExpressions.Value);
        }

        [Fact]
        public void AnnotatableTargetOnEntityTypeWithUnknownEnumMemberDoesnotReturnsUnsupportedExpressions()
        {
            // Arrange
            const string searchAnnotation = @"
              <Annotations Target=""NS.Calendar"" >
                  <Annotation Term=""Org.OData.Capabilities.V1.SearchRestrictions"">
                    <Record>
                        <PropertyValue Property=""Searchable"" Bool=""false"" />
                        <PropertyValue Property=""UnsupportedExpressions"">
                            <EnumMember>Org.OData.Capabilities.V1.SearchExpressions/Unknown</EnumMember>
                        </PropertyValue >
                    </Record>
                  </Annotation>
              </Annotations>";

            IEdmModel model = CapabilitiesModelHelper.GetModelInline(searchAnnotation);
            IEdmEntityType calendar = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Calendar");

            // Act
            SearchRestrictions search = new SearchRestrictions(model, calendar);

            // Assert
            Assert.False(search.Searchable);
            Assert.Null(search.UnsupportedExpressions);
        }

        [Fact]
        public void AnnotatableTargetOnEntitySetWithUnknownEnumMemberDoesnotReturnsUnsupportedExpressions()
        {
            // Arrange
            const string searchAnnotation = @"
              <Annotations Target=""NS.Default/Calendars"" >
                  <Annotation Term=""Org.OData.Capabilities.V1.SearchRestrictions"">
                    <Record>
                        <PropertyValue Property=""Searchable"" Bool=""false"" />
                        <PropertyValue Property=""UnsupportedExpressions"">
                            <EnumMember>Org.OData.Capabilities.V1.SearchExpressions/Unknown</EnumMember>
                        </PropertyValue >
                    </Record>
                  </Annotation>
              </Annotations>";

            IEdmModel model = CapabilitiesModelHelper.GetModelInline(searchAnnotation);
            IEdmEntitySet calendars = model.EntityContainer.FindEntitySet("Calendars");

            // Act
            SearchRestrictions search = new SearchRestrictions(model, calendars);

            // Assert
            Assert.False(search.Searchable);
            Assert.Null(search.UnsupportedExpressions);
        }
        [Fact]
        public void AnnotatableTargetOnNavigationPropertyWithUnknownEnumMemberDoesnotReturnsUnsupportedExpressions()
        {
            // Arrange
            const string searchAnnotation = @"
              <Annotations Target=""NS.Calendar/RelatedEvents"" >
                  <Annotation Term=""Org.OData.Capabilities.V1.SearchRestrictions"">
                    <Record>
                        <PropertyValue Property=""Searchable"" Bool=""false"" />
                        <PropertyValue Property=""UnsupportedExpressions"">
                            <EnumMember>Org.OData.Capabilities.V1.SearchExpressions/Unknown</EnumMember>
                        </PropertyValue >
                    </Record>
                  </Annotation>
              </Annotations>";

            IEdmModel model = CapabilitiesModelHelper.GetModelInline(searchAnnotation);
            IEdmEntityType calendar = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Calendar");
            IEdmNavigationProperty property = calendar.DeclaredNavigationProperties().First(d => d.Name == "RelatedEvents");

            // Act
            SearchRestrictions search = new SearchRestrictions(model, property);

            // Assert
            Assert.False(search.Searchable);
            Assert.Null(search.UnsupportedExpressions);
        }

        [Fact]
        public void AnnotatableTargetOnEntityTypeWithMultipleEnumMemberReturnsCorrectPropertyValue()
        {
            // Arrange
            const string searchAnnotation = @"
              <Annotations Target=""NS.Calendar"" >
                  <Annotation Term=""Org.OData.Capabilities.V1.SearchRestrictions"">
                    <Record>
                        <PropertyValue Property=""Searchable"" Bool=""false"" />
                        <PropertyValue Property=""UnsupportedExpressions"">
                            <EnumMember>Org.OData.Capabilities.V1.SearchExpressions/AND Org.OData.Capabilities.V1.SearchExpressions/OR</EnumMember>
                        </PropertyValue >
                    </Record>
                  </Annotation>
              </Annotations>";

            IEdmModel model = CapabilitiesModelHelper.GetModelInline(searchAnnotation);
            IEdmEntityType calendar = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Calendar");

            // Act
            SearchRestrictions search = new SearchRestrictions(model, calendar);

            // Assert
            Assert.False(search.Searchable);
            Assert.NotNull(search.UnsupportedExpressions);
            Assert.Equal(SearchExpressions.AND | SearchExpressions.OR, search.UnsupportedExpressions.Value);
        }

        [Fact]
        public void AnnotatableTargetOnEntitySetWithMultipleEnumMemberReturnsCorrectPropertyValue()
        {
            // Arrange
            const string searchAnnotation = @"
              <Annotations Target=""NS.Default/Calendars"" >
                  <Annotation Term=""Org.OData.Capabilities.V1.SearchRestrictions"">
                    <Record>
                        <PropertyValue Property=""Searchable"" Bool=""false"" />
                        <PropertyValue Property=""UnsupportedExpressions"">
                            <EnumMember>Org.OData.Capabilities.V1.SearchExpressions/AND Org.OData.Capabilities.V1.SearchExpressions/OR</EnumMember>
                        </PropertyValue >
                    </Record>
                  </Annotation>
              </Annotations>";

            IEdmModel model = CapabilitiesModelHelper.GetModelInline(searchAnnotation);
            IEdmEntitySet calendars = model.EntityContainer.FindEntitySet("Calendars");

            // Act
            SearchRestrictions search = new SearchRestrictions(model, calendars);

            // Assert
            Assert.False(search.Searchable);
            Assert.NotNull(search.UnsupportedExpressions);
            Assert.Equal(SearchExpressions.AND | SearchExpressions.OR, search.UnsupportedExpressions.Value);
        }

        [Fact]
        public void AnnotatableTargetOnNavigationPropertyWithMultipleEnumMemberReturnsCorrectPropertyValue()
        {
            // Arrange
            const string searchAnnotation = @"
              <Annotations Target=""NS.Calendar/RelatedEvents"" >
                  <Annotation Term=""Org.OData.Capabilities.V1.SearchRestrictions"">
                    <Record>
                        <PropertyValue Property=""Searchable"" Bool=""false"" />
                        <PropertyValue Property=""UnsupportedExpressions"">
                            <EnumMember>Org.OData.Capabilities.V1.SearchExpressions/AND Org.OData.Capabilities.V1.SearchExpressions/OR</EnumMember>
                        </PropertyValue >
                    </Record>
                  </Annotation>
              </Annotations>";

            IEdmModel model = CapabilitiesModelHelper.GetModelInline(searchAnnotation);
            IEdmEntityType calendar = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Calendar");
            IEdmNavigationProperty property = calendar.DeclaredNavigationProperties().First(d => d.Name == "RelatedEvents");

            // Act
            SearchRestrictions search = new SearchRestrictions(model, property);

            // Assert
            Assert.False(search.Searchable);
            Assert.NotNull(search.UnsupportedExpressions);
            Assert.Equal(SearchExpressions.AND | SearchExpressions.OR, search.UnsupportedExpressions.Value);
        }
    }
}
