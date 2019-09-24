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
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;
using Xunit;

namespace Microsoft.OpenApi.OData.Reader.Vocabulary.Capabilities.Tests
{
    public class PermissionTypeTests
    {
        [Fact]
        public void DefaultPropertyAsNull()
        {
            // Arrange & Act
            PermissionType permission = new PermissionType();

            // Assert
            Assert.Null(permission.SchemeName);
            Assert.Null(permission.Scopes);
        }

        [Fact]
        public void InitializeWithNullRecordThrows()
        {
            // Arrange & Act
            PermissionType permission = new PermissionType();

            // Assert
            Assert.Throws<ArgumentNullException>("record", () => permission.Initialize(record: null));
        }

        [Fact]
        public void InitializeWithPermissionTypeRecordSuccess()
        {
            // Arrange
            IEdmRecordExpression record = new EdmRecordExpression(
                new EdmPropertyConstructor("SchemeName", new EdmStringConstant("scheme name")),
                new EdmPropertyConstructor("Scopes", new EdmCollectionExpression(new EdmRecordExpression(
                    new EdmPropertyConstructor("Scope", new EdmStringConstant("scope name"))))));

            // Act
            PermissionType permission = new PermissionType();
            permission.Initialize(record);

            // Assert
            VerifyPermissionType(permission);
        }

        [Fact]
        public void ScopeTypeTermValueInitializeWorksForScopeType()
        {
            // Arrange
            string annotation = @"<Annotation Term=""NS.MyTerm"">
                <Record>
                  <PropertyValue Property=""SchemeName"" String=""scheme name"" />
                  <PropertyValue Property=""Scopes"">
                    <Collection>
                      <Record>
                        <PropertyValue Property=""Scope"" String=""scope name"" />
                      </Record>
                    </Collection>
                  </PropertyValue>
                </Record>
              </Annotation>";

            IEdmModel model = GetEdmModel(annotation);
            Assert.NotNull(model); // guard

            // Act
            PermissionType permission = model.GetRecord<PermissionType>(model.EntityContainer, "NS.MyTerm");

            // Assert
            VerifyPermissionType(permission);
        }

        private IEdmModel GetEdmModel(string annotation)
        {
            const string template = @"<edmx:Edmx Version=""4.0"" xmlns:edmx=""http://docs.oasis-open.org/odata/ns/edmx"">
  <edmx:DataServices>
    <Schema Namespace=""NS"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
      <EntityContainer Name =""Default"">
         {0}
      </EntityContainer>
      <Term Name=""MyTerm"" Type=""Capabilities.PermissionType"" />
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>";
            string modelText = string.Format(template, annotation);

            IEdmModel model;
            bool result = CsdlReader.TryParse(XElement.Parse(modelText).CreateReader(), out model, out _);
            Assert.True(result);
            return model;
        }

        private static void VerifyPermissionType(PermissionType permission)
        {
            Assert.NotNull(permission);

            Assert.NotNull(permission.SchemeName);
            Assert.Equal("scheme name", permission.SchemeName);

            Assert.NotNull(permission.Scopes);
            ScopeType scope = Assert.Single(permission.Scopes);
            Assert.Equal("scope name", scope.Scope);
            Assert.Null(scope.RestrictedProperties);
        }
    }
}
