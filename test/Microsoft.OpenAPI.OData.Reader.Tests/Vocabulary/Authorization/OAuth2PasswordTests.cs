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
    public class OAuth2PasswordTests
    {
        [Fact]
        public void SchemeTypeKindAndOAuthTypeSetCorrectly()
        {
            // Arrange
            OAuth2Password password = new OAuth2Password();

            // Act & Assert
            Assert.Equal(SecuritySchemeType.OAuth2, password.SchemeType);
            Assert.Equal(OAuth2Type.Pasword, password.OAuth2Type);
        }

        [Fact]
        public void InitializeThrowArgumentNullRecord()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>("record", () => new OAuth2Password().Initialize(record: null));
        }

        [Fact]
        public void InitializeOAuth2PasswordWithRecordSuccess()
        {
            // Arrange
            IEdmRecordExpression record = new EdmRecordExpression(
                new EdmPropertyConstructor("RefreshUrl", new EdmStringConstant("http://refreshUrl")),
                new EdmPropertyConstructor("TokenUrl", new EdmStringConstant("http://tokenUrl")));

            OAuth2Password password = new OAuth2Password();
            Assert.Null(password.Name);
            Assert.Null(password.Description);
            Assert.Null(password.Scopes);
            Assert.Null(password.RefreshUrl);
            Assert.Null(password.TokenUrl);

            // Act
            password.Initialize(record);

            // Assert
            Assert.Null(password.Name);
            Assert.Null(password.Description);
            Assert.Null(password.Scopes);
            Assert.Equal("http://refreshUrl", password.RefreshUrl);
            Assert.Equal("http://tokenUrl", password.TokenUrl);
        }

        [Fact]
        public void InitializeOAuth2PasswordWorksWithCsdl()
        {
            // Arrange
            string annotation = @"<Annotation Term=""NS.MyOAuth2Password"">
                <Record >
                  <PropertyValue Property=""RefreshUrl"" String=""http://refreshUrl"" />
                  <PropertyValue Property=""TokenUrl"" String=""http://tokenUrl"" />
                </Record>
              </Annotation>";

            IEdmModel model = GetEdmModel(annotation);
            Assert.NotNull(model); // guard
            Assert.NotNull(model.EntityContainer);

            // Act
            OAuth2Password password = model.GetRecord<OAuth2Password>(model.EntityContainer, "NS.MyOAuth2Password");

            // Assert
            Assert.Null(password.Name);
            Assert.Null(password.Description);
            Assert.Null(password.Scopes);
            Assert.Equal("http://refreshUrl", password.RefreshUrl);
            Assert.Equal("http://tokenUrl", password.TokenUrl);
        }

        private IEdmModel GetEdmModel(string annotation)
        {
            const string template = @"<edmx:Edmx Version=""4.0"" xmlns:edmx=""http://docs.oasis-open.org/odata/ns/edmx"">
  <edmx:DataServices>
    <Schema Namespace=""NS"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
      <EntityContainer Name=""Container"">
        {0}
      </EntityContainer>
      <Term Name=""MyOAuth2Password"" Type=""Org.OData.Authorization.V1.OAuth2Password"" />
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
