// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Vocabulary.Authorization;
using Xunit;

namespace Microsoft.OpenApi.OData.Reader.Vocabulary.Authorization.Tests
{
    public class AuthorizationVocabularyTests
    {
        [Fact]
        public void GetAuthorizationsReturnsNullForTargetWithoutAuthorization()
        {
            // Arrange
            EdmModel model = new EdmModel();
            EdmEntityContainer container = new EdmEntityContainer("NS", "Container");
            model.AddElement(container);

            // Act
            var authorizations = model.GetAuthorizations(container);

            // Assert
            Assert.Null(authorizations);
        }

        [Fact]
        public void GetAuthorizationsReturnsForEdmModelNavigationSourceWithAuthroizations()
        {
            // Arrange
            IEdmModel model = GetEdmModel();
            Assert.NotNull(model.EntityContainer);

            // Act
            var authorizations = model.GetAuthorizations(model.EntityContainer);

            // Assert
            Assert.NotEmpty(authorizations);
            Assert.Equal(2, authorizations.Count());

            // #1
            OpenIDConnect openID = Assert.IsType<OpenIDConnect>(authorizations.First());
            Assert.Equal("OpenIDConnect Name", openID.Name);
            Assert.Equal("http://any", openID.IssuerUrl);
            Assert.Equal("OpenIDConnect Description", openID.Description);

            // #2
            Http http = Assert.IsType<Http>(authorizations.Last());
            Assert.Equal("Http Name", http.Name);
            Assert.Equal("Http Scheme", http.Scheme);
            Assert.Equal("Http BearerFormat", http.BearerFormat);
            Assert.Null(http.Description);
        }

        private static IEdmModel GetEdmModel()
        {
            const string schema = @"<?xml version=""1.0"" encoding=""utf-16""?>
<Schema Namespace=""NS"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
  <EntityContainer Name=""Container"">
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
        </Record>
      </Collection>
    </Annotation>
  </EntityContainer>
</Schema>";

            IEdmModel parsedModel;
            bool parsed = SchemaReader.TryParse(new XmlReader[] { XmlReader.Create(new StringReader(schema)) }, out parsedModel, out _);
            Assert.True(parsed);
            return parsedModel;
        }
    }
}
