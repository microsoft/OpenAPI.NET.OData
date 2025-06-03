// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Properties;
using Microsoft.OpenApi.OData.Vocabulary.Authorization;
using Xunit;

namespace Microsoft.OpenApi.OData.Reader.Vocabulary.Authorization.Tests
{
    public class AuthorizationTests
    {
        [Fact]
        public void CreateAuthorizationReturnsNullWithNullRecord()
        {
            // Arrange & Act
            var authorization = OData.Vocabulary.Authorization.Authorization.CreateAuthorization(record: null);

            // Assert
            Assert.Null(authorization);
        }

        [Fact]
        public void CreateAuthorizationThrowsForOAuthAuthorizationRecord()
        {
            // Arrange & Act
            IEdmStructuredTypeReference structuredTypeRef = GetType("Org.OData.Authorization.V1.OAuthAuthorization");
            IEdmRecordExpression record = new EdmRecordExpression(structuredTypeRef,
                new EdmPropertyConstructor("Name", new EdmStringConstant("temp")));

            Action test = () => OData.Vocabulary.Authorization.Authorization.CreateAuthorization(record);

            // Assert
            OpenApiException exception = Assert.Throws<OpenApiException>(test);
            Assert.Equal(String.Format(SRResource.AuthorizationRecordTypeNameNotCorrect, structuredTypeRef.FullName()), exception.Message);
        }

        [Theory]
        [InlineData(typeof(OpenIDConnect))]
        [InlineData(typeof(Http))]
        [InlineData(typeof(ApiKey))]
        [InlineData(typeof(OAuth2ClientCredentials))]
        [InlineData(typeof(OAuth2Implicit))]
        [InlineData(typeof(OAuth2Password))]
        [InlineData(typeof(OAuth2AuthCode))]
        public void CreateAuthorizationReturnsOpenIDConnect(Type type)
        {
            // Arrange & Act
            string qualifiedName = AuthorizationConstants.Namespace + "." + type.Name;
            IEdmRecordExpression record = new EdmRecordExpression(GetType(qualifiedName),
                new EdmPropertyConstructor("Name", new EdmStringConstant("temp")));

            // Assert
            var authorization = OData.Vocabulary.Authorization.Authorization.CreateAuthorization(record);
            Assert.NotNull(authorization);
            Assert.Equal(type, authorization.GetType());

            Assert.Equal("temp", authorization.Name);
            Assert.Null(authorization.Description);
        }

        private static IEdmStructuredTypeReference GetType(string qualifiedName)
        {
            EdmModel model = new EdmModel();
            IEdmType edmType = model.FindType(qualifiedName);
            Assert.NotNull(edmType);

            IEdmComplexType complexType = edmType as IEdmComplexType;
            Assert.NotNull(complexType);

            return new EdmComplexTypeReference(complexType, true);
        }
    }
}
