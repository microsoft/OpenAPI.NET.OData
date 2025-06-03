// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Xml.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Vocabulary.Authorization;
using Xunit;

namespace Microsoft.OpenApi.OData.Reader.Vocabulary.Authorization.Tests
{
    public class OAuth2AuthCodeTests
    {
        [Fact]
        public void SchemeTypeKindAndOAuthTypeSetCorrectly()
        {
            // Arrange
            OAuth2AuthCode authCode = new OAuth2AuthCode();

            // Act & Assert
            Assert.Equal(SecuritySchemeType.OAuth2, authCode.SchemeType);
            Assert.Equal(OAuth2Type.AuthCode, authCode.OAuth2Type);
        }

        [Fact]
        public void InitializeThrowArgumentNullRecord()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>("record", () => new OAuth2AuthCode().Initialize(record: null));
        }

        [Fact]
        public void InitializeOAuth2AuthCodeWithRecordSuccess()
        {
            // Arrange
            IEdmRecordExpression record = new EdmRecordExpression(
                new EdmPropertyConstructor("TokenUrl", new EdmStringConstant("http://tokenUrl")),
                new EdmPropertyConstructor("AuthorizationUrl", new EdmStringConstant("http://authorizationUrl")));

            OAuth2AuthCode authCode = new OAuth2AuthCode();
            Assert.Null(authCode.Name);
            Assert.Null(authCode.Description);
            Assert.Null(authCode.Scopes);
            Assert.Null(authCode.AuthorizationUrl);
            Assert.Null(authCode.TokenUrl);

            // Act
            authCode.Initialize(record);

            // Assert
            Assert.Null(authCode.Name);
            Assert.Null(authCode.Description);
            Assert.Null(authCode.Scopes);
            Assert.Equal("http://authorizationUrl", authCode.AuthorizationUrl);
            Assert.Equal("http://tokenUrl", authCode.TokenUrl);
        }

        [Fact]
        public void InitializeOAuth2AuthCodeWorksWithCsdl()
        {
            // Arrange
            string annotation = @"<Annotation Term=""NS.MyOAuth2AuthCode"">
                <Record >
                  <PropertyValue Property=""TokenUrl"" String=""http://tokenUrl"" />
                  <PropertyValue Property=""AuthorizationUrl"" String=""http://authorizationUrl"" />
                </Record>
              </Annotation>";

            IEdmModel model = GetEdmModel(annotation);
            Assert.NotNull(model); // guard
            Assert.NotNull(model.EntityContainer);

            // Act
            OAuth2AuthCode authoCode = model.GetRecord<OAuth2AuthCode>(model.EntityContainer, "NS.MyOAuth2AuthCode");

            // Assert
            Assert.Null(authoCode.Name);
            Assert.Null(authoCode.Description);
            Assert.Null(authoCode.Scopes);
            Assert.Equal("http://tokenUrl", authoCode.TokenUrl);
            Assert.Equal("http://authorizationUrl", authoCode.AuthorizationUrl);
        }

        private IEdmModel GetEdmModel(string annotation)
        {
            const string template = @"<edmx:Edmx Version=""4.0"" xmlns:edmx=""http://docs.oasis-open.org/odata/ns/edmx"">
  <edmx:DataServices>
    <Schema Namespace=""NS"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
      <EntityContainer Name=""Container"">
        {0}
      </EntityContainer>
      <Term Name=""MyOAuth2AuthCode"" Type=""Org.OData.Authorization.V1.OAuth2AuthCode"" />
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>";
            string modelText = string.Format(template, annotation);

            IEdmModel model;
            bool result = CsdlReader.TryParse(XElement.Parse(modelText).CreateReader(), out model, out _);
            Assert.True(result);
            return model;
        }
    }
}
