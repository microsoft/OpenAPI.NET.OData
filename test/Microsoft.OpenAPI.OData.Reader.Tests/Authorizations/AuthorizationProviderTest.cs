// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Validation;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Authorizations;
using Xunit;

namespace Microsoft.OpenApi.OData.Reader.Authorizations.Tests
{
    public class AuthorizationProviderTest
    {
        [Fact]
        public void GetAuthorizationsThrowArgumentNullModel()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>("model",
                () => new AuthorizationProvider().GetAuthorizations(model: null, target: null));
        }

        [Fact]
        public void GetAuthorizationsThrowArgumentNullTarget()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>("target",
                () => new AuthorizationProvider().GetAuthorizations(model: new EdmModel(), target: null));
        }

        [Fact]
        public void GetAuthorizationsReturnsNullForTargetWithoutAuthorization()
        {
            // Arrange
            EdmModel model = new EdmModel();
            EdmEntityContainer container = new EdmEntityContainer("NS", "Container");
            model.AddElement(container);
            AuthorizationProvider provider = new AuthorizationProvider();

            // Act & Assert
            var authorizations = provider.GetAuthorizations(model, container);

            // Assert
            Assert.Empty(authorizations);
        }

        [Theory]
        [InlineData("Entities")]
        [InlineData("Me")]
        public void GetAuthorizationsReturnsForEdmModelNavigationSourceWithAuthroizations(string name)
        {
            // Arrange
            IEdmModel model = GetEdmModel();
            IEdmNavigationSource navigationSource = model.FindDeclaredNavigationSource(name);
            Assert.NotNull(navigationSource);
            AuthorizationProvider provider = new AuthorizationProvider();

            // Act
            var authorizations = provider.GetAuthorizations(model, navigationSource as IEdmVocabularyAnnotatable);

            // Assert
            Assert.NotEmpty(authorizations);
            Assert.Equal(2, authorizations.Count());
            Assert.IsType<OpenIDConnect>(authorizations.First());
            Assert.IsType<Http>(authorizations.Last());
        }

        private static IEdmModel GetEdmModel()
        {
            const string schema = @"<?xml version=""1.0"" encoding=""utf-16""?>
<Schema Namespace=""NS"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
  <EntityType Name=""Entity"">
    <Key>
      <PropertyRef Name=""Id"" />
    </Key>
    <Property Name=""Id"" Type=""Edm.Int32"" Nullable=""false"" />
  </EntityType>
  <EntityContainer Name=""Container"">
    <EntitySet Name=""Entities"" EntityType=""NS.Entity"">
      <Annotation Term=""Org.OData.Authorization.V1.Authorizations"">
        <Collection>
          <Record Type=""Org.OData.Authorization.V1.OpenIDConnect"">
            <PropertyValue Property=""IssuerUrl"" String=""http://any"" />
            <PropertyValue Property=""Name"" String=""OpenIDConnect Name"" />
            <PropertyValue Property=""Description"" String=""OpenIDConnect Description"" />
          </Record>
          <Record Type=""Org.OData.Authorization.V1.Http"">
            <PropertyValue Property=""BearerFormat"" String=""Http BearerFormat"" />
            <PropertyValue Property=""Scheme"" String=""Http Scheme"" />
            <PropertyValue Property=""Name"" String=""Http Name"" />
            <PropertyValue Property=""Description"" String=""Http Description"" />
          </Record>
        </Collection>
      </Annotation>
    </EntitySet>
    <Singleton Name=""Me"" Type=""NS.Entity"" />
  </EntityContainer>
  <Annotations Target=""NS.Container/Me"">
    <Annotation Term=""Org.OData.Authorization.V1.Authorizations"">
      <Collection>
        <Record Type=""Org.OData.Authorization.V1.OpenIDConnect"">
          <PropertyValue Property=""IssuerUrl"" String=""http://any"" />
          <PropertyValue Property=""Name"" String=""OpenIDConnect Name"" />
          <PropertyValue Property=""Description"" String=""OpenIDConnect Description"" />
        </Record>
        <Record Type=""Org.OData.Authorization.V1.Http"">
          <PropertyValue Property=""BearerFormat"" String=""Http BearerFormat"" />
          <PropertyValue Property=""Scheme"" String=""Http Scheme"" />
          <PropertyValue Property=""Name"" String=""Http Name"" />
          <PropertyValue Property=""Description"" String=""Http Description"" />
        </Record>
      </Collection>
    </Annotation>
  </Annotations>
</Schema>";

            IEdmModel parsedModel;
            IEnumerable<EdmError> errors;
            bool parsed = SchemaReader.TryParse(new XmlReader[] { XmlReader.Create(new StringReader(schema)) }, out parsedModel, out errors);
            Assert.True(parsed);
            return parsedModel;
        }
    }
}
