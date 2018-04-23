// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Annotations;
using Microsoft.OpenApi.OData.Edm;
using Xunit;

namespace Microsoft.OpenApi.OData.Generator.Tests
{
    public class OpenApiSecuritySchemeGeneratorTest
    {
        [Fact]
        public void CreateSecuritySchemesThrowArgumentNullContext()
        {
            // Arrange
            ODataContext context = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context", () => context.CreateSecuritySchemes());
        }

        [Fact]
        public void CreateSecuritySchemesReturnsEmptyForCoreModel()
        {
            // Arrange
            ODataContext context = new ODataContext(EdmCoreModel.Instance);

            // Act
            var securitySchemes = context.CreateSecuritySchemes();

            // Assert
            Assert.NotNull(securitySchemes);
            Assert.Empty(securitySchemes);
        }

        [Fact]
        public void CreateSecuritySchemesReturnsCorrectsForOAuth2Implicit()
        {
            // Arrange
            string record = @"<Record Type=""Org.OData.Authorization.V1.OAuth2Implicit"">
  <PropertyValue Property=""Name"" String=""DelegatedWork""/>
  <PropertyValue Property=""Description"" String=""DelegatedWork Description""/>
  <PropertyValue Property=""RefreshUrl"" String=""https://any1""/>
  <PropertyValue Property=""AuthorizationUrl"" String=""https://any2"" />
  <PropertyValue Property=""Scopes"">
    <Collection>
      <Record>
        <PropertyValue Property=""Scope"" String=""User.ReadAll""></PropertyValue>
        <PropertyValue Property=""Description"" String=""Read all user data""></PropertyValue>
      </Record>
      <Record>
        <PropertyValue Property=""Scope"" String=""User.WriteAll""></PropertyValue>
        <PropertyValue Property=""Description"" String=""Write all user data""></PropertyValue>
      </Record>
    </Collection>
  </PropertyValue>
</Record>";
            IEdmModel model = GetEdmModel(record);
            ODataContext context = new ODataContext(model);

            // Act
            var securitySchemes = context.CreateSecuritySchemes();

            // Assert
            Assert.NotNull(securitySchemes);
            var securityScheme = Assert.Single(securitySchemes);
            Assert.Equal("DelegatedWork", securityScheme.Key);

            Assert.Equal(SecuritySchemeType.OAuth2, securityScheme.Value.Type);
            Assert.Equal("DelegatedWork Description", securityScheme.Value.Description);

            Assert.NotNull(securityScheme.Value.Flows);
            Assert.NotNull(securityScheme.Value.Flows.Implicit);
            Assert.Null(securityScheme.Value.Flows.Password);

            OpenApiOAuthFlow flow = securityScheme.Value.Flows.Implicit;
            Assert.Equal(new Uri("https://any1"), flow.RefreshUrl);
            Assert.Equal(new Uri("https://any2"), flow.AuthorizationUrl);
            Assert.Equal("User.ReadAll/Read all user data,User.WriteAll/Write all user data",
                String.Join(",", flow.Scopes.Select(s => s.Key + "/" + s.Value)));

            Assert.Null(flow.TokenUrl);
        }

        [Fact]
        public void CreateSecuritySchemesReturnsCorrectsForHttpAndCanSerializeAsJsonCorrect()
        {
            // Arrange
            string record = @"<Record Type=""Org.OData.Authorization.V1.Http"">
  <PropertyValue Property=""Name"" String=""Http Name""/>
  <PropertyValue Property=""Description"" String=""Http Description""/>
  <PropertyValue Property=""Scheme"" String=""bearer""/>
  <PropertyValue Property=""BearerFormat"" String=""JWT"" />
</Record>";
            IEdmModel model = GetEdmModel(record);
            ODataContext context = new ODataContext(model);

            // Act
            var securitySchemes = context.CreateSecuritySchemes();

            // Assert
            Assert.NotNull(securitySchemes);
            var securityScheme = Assert.Single(securitySchemes);
            Assert.Equal("Http Name", securityScheme.Key);

            Assert.Equal(SecuritySchemeType.Http, securityScheme.Value.Type);
            string json = securityScheme.Value.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);

            Assert.Equal(@"{
  ""type"": ""http"",
  ""description"": ""Http Description"",
  ""scheme"": ""bearer"",
  ""bearerFormat"": ""JWT""
}".Replace("\r\n", "\n"), json);
        }

        [Fact]
        public void CreateSecuritySchemesReturnsCorrectsForApiKeyAndCanSerializeAsJsonCorrect()
        {
            // Arrange
            string record = @"<Record Type=""Org.OData.Authorization.V1.ApiKey"">
  <PropertyValue Property=""Name"" String=""ApiKey Name""/>
  <PropertyValue Property=""Description"" String=""ApiKey Description""/>
  <PropertyValue Property=""KeyName"" String=""ApiKey KeyName""/>
  <PropertyValue Property=""Location"" >
    <EnumMember>Org.OData.Authorization.V1.KeyLocation/Header</EnumMember>
  </PropertyValue>
</Record>";
            IEdmModel model = GetEdmModel(record);
            ODataContext context = new ODataContext(model);

            // Act
            var securitySchemes = context.CreateSecuritySchemes();

            // Assert
            Assert.NotNull(securitySchemes);
            var securityScheme = Assert.Single(securitySchemes);
            Assert.Equal("ApiKey Name", securityScheme.Key);

            Assert.Equal(SecuritySchemeType.ApiKey, securityScheme.Value.Type);
            string json = securityScheme.Value.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);

            Assert.Equal(@"{
  ""type"": ""apiKey"",
  ""description"": ""ApiKey Description"",
  ""name"": ""ApiKey KeyName"",
  ""in"": ""header""
}".Replace("\r\n", "\n"), json);
        }

        [Fact]
        public void CreateSecuritySchemesReturnsCorrectsForOpenIDConnectAndCanSerializeAsYamlCorrect()
        {
            // Arrange
            string record = @"<Record Type=""Org.OData.Authorization.V1.OpenIDConnect"">
  <PropertyValue Property=""Name"" String=""OpenIDConnect Name""/>
  <PropertyValue Property=""Description"" String=""OpenIDConnect Description""/>
  <PropertyValue Property=""IssuerUrl"" String=""http://any""/>
</Record>";
            IEdmModel model = GetEdmModel(record);
            ODataContext context = new ODataContext(model);

            // Act
            var securitySchemes = context.CreateSecuritySchemes();

            // Assert
            Assert.NotNull(securitySchemes);
            var securityScheme = Assert.Single(securitySchemes);
            Assert.Equal("OpenIDConnect Name", securityScheme.Key);

            Assert.Equal(SecuritySchemeType.OpenIdConnect, securityScheme.Value.Type);
            string yaml = securityScheme.Value.SerializeAsYaml(OpenApiSpecVersion.OpenApi3_0);

            Assert.Equal(@"type: openIdConnect
description: OpenIDConnect Description
openIdConnectUrl: http://any/".Replace("\r\n", "\n"), yaml);
        }

        public static IEdmModel GetEdmModel(string records)
        {
            const string template = @"<edmx:Edmx Version=""4.0"" xmlns:edmx=""http://docs.oasis-open.org/odata/ns/edmx"">
  <edmx:DataServices>
    <Schema Namespace=""NS"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
      <EntityContainer Name=""Container"" />
      <Annotations Target=""NS.Container"">
        <Annotation Term=""Org.OData.Authorization.V1.Authorizations"" >
          <Collection>
            {0}
          </Collection>
        </Annotation>
      </Annotations>
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>";

            string modelText = string.Format(template, records);

            // Once updated to latest ODL, please remove the append function call.
            return modelText.AppendAnnotations();
        }
    }
}
