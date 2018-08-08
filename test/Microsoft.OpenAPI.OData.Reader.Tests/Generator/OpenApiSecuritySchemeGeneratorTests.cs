// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Xml;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Validation;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Generator;
using Xunit;

namespace Microsoft.OpenApi.OData.Tests
{
    public class OpenApiSecuritySchemeGeneratorTest
    {
        [Fact]
        public void CreateSecuritySchemesWorksForAuthorizationsOnEntitySetContainer()
        {
            // Arrange
            ODataContext context = new ODataContext(GetEdmModel());

            // Act
            var schemes = context.CreateSecuritySchemes();

            // Assert
            Assert.NotNull(schemes);
            Assert.NotEmpty(schemes);
            Assert.Equal(new[] { "OAuth2ClientCredentials Name", "Http Name" }, schemes.Keys);

            var scheme = schemes["OAuth2ClientCredentials Name"];
            Assert.Equal(SecuritySchemeType.OAuth2, scheme.Type);
            Assert.NotNull(scheme.Flows.ClientCredentials);
            Assert.Equal("http://TokenUrl", scheme.Flows.ClientCredentials.TokenUrl.OriginalString);
            Assert.Equal("http://RefreshUrl", scheme.Flows.ClientCredentials.RefreshUrl.OriginalString);
            Assert.Equal("OAuth2ClientCredentials Description", scheme.Description);
            string json = scheme.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);
            Assert.Equal(@"{
  ""type"": ""oauth2"",
  ""description"": ""OAuth2ClientCredentials Description"",
  ""flows"": {
    ""clientCredentials"": {
      ""tokenUrl"": ""http://tokenurl/"",
      ""refreshUrl"": ""http://refreshurl/"",
      ""scopes"": {
        ""Scope1"": ""Description 1""
      }
    }
  }
}".ChangeLineBreaks(), json);

            scheme = schemes["Http Name"];
            Assert.Equal(SecuritySchemeType.Http, scheme.Type);
            json = scheme.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);
            Assert.Equal(@"{
  ""type"": ""http"",
  ""description"": ""Http Description"",
  ""scheme"": ""Http Scheme"",
  ""bearerFormat"": ""Http BearerFormat""
}".ChangeLineBreaks(), json);
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
    <EntitySet Name=""Entities"" EntityType=""NS.Entity"" />
  </EntityContainer>
  <Annotations Target=""NS.Container"">
    <Annotation Term=""Org.OData.Authorization.V1.Authorizations"">
      <Collection>
        <Record Type=""Org.OData.Authorization.V1.OAuth2ClientCredentials"">
          <PropertyValue Property=""TokenUrl"" String=""http://TokenUrl"" />
          <PropertyValue Property=""RefreshUrl"" String=""http://RefreshUrl"" />
          <PropertyValue Property=""Name"" String=""OAuth2ClientCredentials Name"" />
          <PropertyValue Property=""Description"" String=""OAuth2ClientCredentials Description"" />
          <PropertyValue Property=""Scopes"">
            <Collection>
              <Record>
                 <PropertyValue Property=""Scope"" String=""Scope1"" />
                 <PropertyValue Property=""Description"" String=""Description 1"" />
              </Record>
            </Collection>
          </PropertyValue>
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
