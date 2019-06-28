// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Linq;
using System.Xml.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Vocabulary.Authorization;
using Xunit;

namespace Microsoft.OpenApi.OData.Reader.Vocabulary.Authorization.Tests
{
    public class SecuritySchemeTests
    {
        [Fact]
        public void InitializeThrowArgumentNullRecord()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>("record", () => new SecurityScheme().Initialize(record: null));
        }

        [Fact]
        public void TermAttributeAttachedOnSecurityScheme()
        {
            // Arrange & Act
            string qualifiedName = Utils.GetTermQualifiedName<SecurityScheme>();

            // Assert
            Assert.Equal("Org.OData.Authorization.V1.SecuritySchemes", qualifiedName);
        }

        [Fact]
        public void InitializeSecuritySchemeWithRecordSuccess()
        {
            // Arrange
            IEdmRecordExpression record = new EdmRecordExpression(
                new EdmPropertyConstructor("Authorization", new EdmStringConstant("DelegatedWork")),
                new EdmPropertyConstructor("RequiredScopes", new EdmCollectionExpression(
                    new EdmStringConstant("User.ReadAll"),
                    new EdmStringConstant("User.WriteAll"))));

            SecurityScheme securityScheme = new SecurityScheme();
            Assert.Null(securityScheme.Authorization);
            Assert.Null(securityScheme.RequiredScopes);

            // Act
            securityScheme.Initialize(record);

            // Assert
            Assert.NotNull(securityScheme.Authorization);
            Assert.Equal("DelegatedWork", securityScheme.Authorization);

            Assert.NotNull(securityScheme.RequiredScopes);
            Assert.Equal(2, securityScheme.RequiredScopes.Count);
            Assert.Equal(new[] { "User.ReadAll", "User.WriteAll" }, securityScheme.RequiredScopes);
        }

        [Fact]
        public void InitializeSecuritySchemeWorksWithCsdl()
        {
            // Arrange
            string annotation = @"<Annotation Term=""Org.OData.Authorization.V1.SecuritySchemes"">
                <Collection>
                  <Record>
                    <PropertyValue Property=""Authorization"" String=""DelegatedWork"" />
                    <PropertyValue Property=""RequiredScopes"" >
                      <Collection>
                        <String>User.ReadAll</String>
                        <String>User.WriteAll</String>
                      </Collection>
                    </PropertyValue>
                  </Record>
                  <Record>
                    <PropertyValue Property=""Authorization"" String=""DelegatedPersonal"" />
                    <PropertyValue Property=""RequiredScopes"" >
                      <Collection>
                        <String>Directory.ReadAll</String>
                        <String>Directory.WriteAll</String>
                      </Collection>
                    </PropertyValue>
                  </Record>
                </Collection>
              </Annotation>";

            IEdmModel model = GetEdmModel(annotation);
            Assert.NotNull(model); // guard
            Assert.NotNull(model.EntityContainer);

            // Act
            SecurityScheme[] schemes = model.GetCollection<SecurityScheme>(model.EntityContainer).ToArray();

            // Assert
            Assert.NotNull(schemes);
            Assert.Equal(2, schemes.Length);

            // #1
            Assert.Equal("DelegatedWork", schemes[0].Authorization);
            Assert.Equal(2, schemes[0].RequiredScopes.Count);
            Assert.Equal(new[] { "User.ReadAll", "User.WriteAll" }, schemes[0].RequiredScopes);

            // #2
            Assert.Equal("DelegatedPersonal", schemes[1].Authorization);
            Assert.Equal(2, schemes[1].RequiredScopes.Count);
            Assert.Equal(new[] { "Directory.ReadAll", "Directory.WriteAll" }, schemes[1].RequiredScopes);
        }

        private IEdmModel GetEdmModel(string annotation)
        {
            const string template = @"<edmx:Edmx Version=""4.0"" xmlns:edmx=""http://docs.oasis-open.org/odata/ns/edmx"">
  <edmx:DataServices>
    <Schema Namespace=""NS"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
      <EntityContainer Name=""Container"">
        {0}
      </EntityContainer>
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
