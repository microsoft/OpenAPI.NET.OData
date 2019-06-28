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
    public class ScopeTypeTests
    {
        [Fact]
        public void DefaultPropertyAsNull()
        {
            // Arrange & Act
            ScopeType scope = new ScopeType();

            // Assert
            Assert.Null(scope.Scope);
            Assert.Null(scope.RestrictedProperties);
        }

        [Fact]
        public void InitializeWithNullRecordThrows()
        {
            // Arrange & Act
            ScopeType scope = new ScopeType();

            // Assert
            Assert.Throws<ArgumentNullException>("record", () => scope.Initialize(record: null));
        }

        [Fact]
        public void InitializeWithScopeTypeRecordSuccess()
        {
            // Arrange
            IEdmRecordExpression record = new EdmRecordExpression(
                new EdmPropertyConstructor("Scope", new EdmStringConstant("name")),
                new EdmPropertyConstructor("RestrictedProperties", new EdmStringConstant("abc,xyz")));

            // Act
            ScopeType scope = new ScopeType();
            scope.Initialize(record);

            // Assert
            Assert.NotNull(scope.Scope);
            Assert.Equal("name", scope.Scope);

            Assert.NotNull(scope.RestrictedProperties);
            Assert.Equal("abc,xyz", scope.RestrictedProperties);
        }

        [Fact]
        public void ScopeTypeTermValueInitializeWorksForScopeType()
        {
            // Arrange
            string annotation = @"<Annotation Term=""NS.MyTerm"">
                <Record>
                  <PropertyValue Property=""Scope"" String=""name"" />
                  <PropertyValue Property=""RestrictedProperties"" String=""abc,xyz"" />
                </Record>
              </Annotation>";

            IEdmModel model = GetEdmModel(annotation);
            Assert.NotNull(model); // guard

            // Act
            ScopeType scope = model.GetRecord<ScopeType>(model.EntityContainer, "NS.MyTerm");

            // Assert
            Assert.NotNull(scope);
            Assert.Equal("name", scope.Scope);
            Assert.NotNull(scope.RestrictedProperties);
            Assert.Equal("abc,xyz", scope.RestrictedProperties);
        }

        private IEdmModel GetEdmModel(string annotation)
        {
            const string template = @"<edmx:Edmx Version=""4.0"" xmlns:edmx=""http://docs.oasis-open.org/odata/ns/edmx"">
  <edmx:DataServices>
    <Schema Namespace=""NS"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
      <EntityContainer Name =""Default"">
         {0}
      </EntityContainer>
      <Term Name=""MyTerm"" Type=""Capabilities.ScopeType"" />
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
