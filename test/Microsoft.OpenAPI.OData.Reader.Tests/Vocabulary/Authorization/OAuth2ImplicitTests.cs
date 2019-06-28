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
    public class OAuth2ImplicitTests
    {
        [Fact]
        public void SchemeTypeKindAndOAuthTypeSetCorrectly()
        {
            // Arrange
            OAuth2Implicit oAuthImplicit = new OAuth2Implicit();

            // Act & Assert
            Assert.Equal(SecuritySchemeType.OAuth2, oAuthImplicit.SchemeType);
            Assert.Equal(OAuth2Type.Implicit, oAuthImplicit.OAuth2Type);
        }

        [Fact]
        public void InitializeThrowArgumentNullRecord()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>("record", () => new OAuth2Implicit().Initialize(record: null));
        }

        [Fact]
        public void InitializeOAuth2ImplicitWithRecordSuccess()
        {
            // Arrange
            IEdmRecordExpression record = new EdmRecordExpression(
                new EdmPropertyConstructor("AuthorizationUrl", new EdmStringConstant("http://authorizationUrl")));

            OAuth2Implicit oAuthImplicit = new OAuth2Implicit();
            Assert.Null(oAuthImplicit.Name);
            Assert.Null(oAuthImplicit.Description);
            Assert.Null(oAuthImplicit.Scopes);
            Assert.Null(oAuthImplicit.AuthorizationUrl);

            // Act
            oAuthImplicit.Initialize(record);

            // Assert
            Assert.Null(oAuthImplicit.Name);
            Assert.Null(oAuthImplicit.Description);
            Assert.Null(oAuthImplicit.Scopes);
            Assert.Equal("http://authorizationUrl", oAuthImplicit.AuthorizationUrl);
        }

        [Fact]
        public void InitializeOAuth2ImplicitWorksWithCsdl()
        {
            // Arrange
            string annotation = @"<Annotation Term=""NS.MyOAuth2Implicit"">
                <Record >
                  <PropertyValue Property=""AuthorizationUrl"" String=""http://authorizationUrl"" />
                </Record>
              </Annotation>";

            IEdmModel model = GetEdmModel(annotation);
            Assert.NotNull(model); // guard
            Assert.NotNull(model.EntityContainer);

            // Act
            OAuth2Implicit oAuthImplicit = model.GetRecord<OAuth2Implicit>(model.EntityContainer, "NS.MyOAuth2Implicit");

            // Assert
            Assert.Null(oAuthImplicit.Name);
            Assert.Null(oAuthImplicit.Description);
            Assert.Null(oAuthImplicit.Scopes);
            Assert.Equal("http://authorizationUrl", oAuthImplicit.AuthorizationUrl);
        }

        private IEdmModel GetEdmModel(string annotation)
        {
            const string template = @"<edmx:Edmx Version=""4.0"" xmlns:edmx=""http://docs.oasis-open.org/odata/ns/edmx"">
  <edmx:DataServices>
    <Schema Namespace=""NS"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
      <EntityContainer Name=""Container"">
        {0}
      </EntityContainer>
      <Term Name=""MyOAuth2Implicit"" Type=""Org.OData.Authorization.V1.OAuth2Implicit"" />
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
