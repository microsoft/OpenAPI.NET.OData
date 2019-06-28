// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Xml.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Vocabulary.Authorization;
using Xunit;

namespace Microsoft.OpenApi.OData.Reader.Vocabulary.Authorization.Tests
{
    public class OAuth2ClientCredentialsTests
    {
        [Fact]
        public void SchemeTypeKindAndOAuthTypeSetCorrectly()
        {
            // Arrange
            OAuth2ClientCredentials credentials = new OAuth2ClientCredentials();

            // Act & Assert
            Assert.Equal(SecuritySchemeType.OAuth2, credentials.SchemeType);
            Assert.Equal(OAuth2Type.ClientCredentials, credentials.OAuth2Type);
        }

        [Fact]
        public void InitializeThrowArgumentNullRecord()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>("record", () => new OAuth2ClientCredentials().Initialize(record: null));
        }

        [Fact]
        public void InitializeOAuth2ClientCredentialsWithRecordSuccess()
        {
            // Arrange
            IEdmRecordExpression record = new EdmRecordExpression(
                new EdmPropertyConstructor("TokenUrl", new EdmStringConstant("http://tokenUrl")));

            OAuth2ClientCredentials credentials = new OAuth2ClientCredentials();
            Assert.Null(credentials.Name);
            Assert.Null(credentials.Description);
            Assert.Null(credentials.Scopes);
            Assert.Null(credentials.TokenUrl);

            // Act
            credentials.Initialize(record);

            // Assert
            Assert.Null(credentials.Name);
            Assert.Null(credentials.Description);
            Assert.Null(credentials.Scopes);
            Assert.Equal("http://tokenUrl", credentials.TokenUrl);
        }

        [Fact]
        public void InitializeOAuth2ClientCredentialsWorksWithCsdl()
        {
            // Arrange
            string annotation = @"<Annotation Term=""NS.MyOAuth2ClientCredentials"">
                <Record >
                  <PropertyValue Property=""TokenUrl"" String=""http://tokenUrl"" />
                </Record>
              </Annotation>";

            IEdmModel model = GetEdmModel(annotation);
            Assert.NotNull(model); // guard
            Assert.NotNull(model.EntityContainer);

            // Act
            OAuth2ClientCredentials credentials = model.GetRecord<OAuth2ClientCredentials>(model.EntityContainer, "NS.MyOAuth2ClientCredentials");

            // Assert
            Assert.Null(credentials.Name);
            Assert.Null(credentials.Description);
            Assert.Null(credentials.Scopes);
            Assert.Equal("http://tokenUrl", credentials.TokenUrl);
        }

        private IEdmModel GetEdmModel(string annotation)
        {
            const string template = @"<edmx:Edmx Version=""4.0"" xmlns:edmx=""http://docs.oasis-open.org/odata/ns/edmx"">
  <edmx:DataServices>
    <Schema Namespace=""NS"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
      <EntityContainer Name=""Container"">
        {0}
      </EntityContainer>
      <Term Name=""MyOAuth2ClientCredentials"" Type=""Org.OData.Authorization.V1.OAuth2ClientCredentials"" />
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
