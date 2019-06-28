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
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Vocabulary.Authorization;
using Xunit;

namespace Microsoft.OpenApi.OData.Reader.Vocabulary.Authorization.Tests
{
    public class ApiKeyTests
    {
        [Fact]
        public void SchemeTypeKindSetCorrectly()
        {
            // Arrange
            ApiKey apiKey = new ApiKey();

            // Act & Assert
            Assert.Equal(SecuritySchemeType.ApiKey, apiKey.SchemeType);
        }

        [Fact]
        public void InitializeThrowArgumentNullRecord()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>("record", () => new ApiKey().Initialize(record: null));
        }

        [Fact]
        public void InitializeApiKeyWithRecordSuccess()
        {
            // Arrange
            EdmModel model = new EdmModel();
            IEdmType edmType = model.FindType("Org.OData.Authorization.V1.KeyLocation");
            IEdmEnumType enumType = edmType as IEdmEnumType;
            IEdmEnumMember enumMember = enumType.Members.FirstOrDefault(c => c.Name == "Header");
            Assert.NotNull(enumMember);

            IEdmRecordExpression record = new EdmRecordExpression(
                new EdmPropertyConstructor("Name", new EdmStringConstant("DelegatedWork")),
                new EdmPropertyConstructor("Description", new EdmStringConstant("Description of the authorization scheme")),
                new EdmPropertyConstructor("KeyName", new EdmStringConstant("keyName")),
                new EdmPropertyConstructor("Location", new EdmEnumMemberExpression(enumMember)));

            ApiKey apiKey = new ApiKey();
            Assert.Null(apiKey.Name);
            Assert.Null(apiKey.Description);
            Assert.Null(apiKey.Location);
            Assert.Null(apiKey.KeyName);

            // Act
            apiKey.Initialize(record);

            // Assert
            Assert.Equal("DelegatedWork", apiKey.Name);
            Assert.Equal("Description of the authorization scheme", apiKey.Description);
            Assert.Equal("keyName", apiKey.KeyName);
            Assert.Equal(KeyLocation.Header, apiKey.Location);
        }

        [Fact]
        public void InitializeApiKeyWorksWithCsdl()
        {
            // Arrange
            string annotation = @"<Annotation Term=""NS.MyApiKey"">
                <Record Type=""Org.OData.Authorization.V1.ApiKey"" >
                  <PropertyValue Property=""Name"" String=""DelegatedWork"" />
                  <PropertyValue Property=""Description"" String=""Description of the authorization scheme"" />
                  <PropertyValue Property=""KeyName"" String=""keyName"" />
                  <PropertyValue Property=""Location"" EnumMember=""Org.OData.Authorization.V1.KeyLocation/QueryOption"" />
                </Record>
              </Annotation>";

            IEdmModel model = GetEdmModel(annotation);
            Assert.NotNull(model); // guard
            Assert.NotNull(model.EntityContainer);

            // Act
            ApiKey apiKey = model.GetRecord<ApiKey>(model.EntityContainer, "NS.MyApiKey");

            // Assert
            Assert.NotNull(apiKey);
            Assert.Equal("DelegatedWork", apiKey.Name);
            Assert.Equal("Description of the authorization scheme", apiKey.Description);
            Assert.Equal("keyName", apiKey.KeyName);
            Assert.Equal(KeyLocation.QueryOption, apiKey.Location);
        }

        private IEdmModel GetEdmModel(string annotation)
        {
            const string template = @"<edmx:Edmx Version=""4.0"" xmlns:edmx=""http://docs.oasis-open.org/odata/ns/edmx"">
  <edmx:DataServices>
    <Schema Namespace=""NS"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
      <EntityContainer Name=""Container"">
        {0}
      </EntityContainer>
      <Term Name=""MyApiKey"" Type=""Org.OData.Authorization.V1.ApiKey"" />
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
