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
    public class AuthorizationScopeTests
    {
        [Fact]
        public void InitializeThrowArgumentNullRecord()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>("record", () => new AuthorizationScope().Initialize(record: null));
        }

        [Fact]
        public void InitializeAuthorizationScopeWithRecordSuccess()
        {
            // Arrange
            IEdmRecordExpression record = new EdmRecordExpression(
                new EdmPropertyConstructor("Scope", new EdmStringConstant("ScopeName")),
                new EdmPropertyConstructor("Grant", new EdmStringConstant("GrantAccess")));

            AuthorizationScope scope = new AuthorizationScope();
            Assert.Null(scope.Scope);
            Assert.Null(scope.Description);
            Assert.Null(scope.Grant);

            // Act
            scope.Initialize(record);

            // Assert
            Assert.Equal("ScopeName", scope.Scope);
            Assert.Null(scope.Description);
            Assert.Equal("GrantAccess", scope.Grant);
        }

        [Fact]
        public void InitializeAuthorizationScopeWorksWithCsdl()
        {
            // Arrange
            string annotation = @"<Annotation Term=""NS.MyAuthorizationScope"">
                <Record Type=""Org.OData.Authorization.V1.AuthorizationScope"" >
                  <PropertyValue Property=""Scope"" String=""Scope name"" />
                  <PropertyValue Property=""Description"" String=""Description of the scope"" />
                  <PropertyValue Property=""Grant"" String=""grant access"" />
                </Record>
              </Annotation>";

            IEdmModel model = GetEdmModel(annotation);
            Assert.NotNull(model); // guard
            Assert.NotNull(model.EntityContainer);

            // Act
            AuthorizationScope scope = model.GetRecord<AuthorizationScope>(model.EntityContainer, "NS.MyAuthorizationScope");

            // Assert
            Assert.NotNull(scope);
            Assert.Equal("Scope name", scope.Scope);
            Assert.Equal("Description of the scope", scope.Description);
            Assert.Equal("grant access", scope.Grant);
        }

        private IEdmModel GetEdmModel(string annotation)
        {
            const string template = @"<edmx:Edmx Version=""4.0"" xmlns:edmx=""http://docs.oasis-open.org/odata/ns/edmx"">
  <edmx:DataServices>
    <Schema Namespace=""NS"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
      <EntityContainer Name=""Container"">
        {0}
      </EntityContainer>
      <Term Name=""MyAuthorizationScope"" Type=""Org.OData.Authorization.V1.AuthorizationScope"" />
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
