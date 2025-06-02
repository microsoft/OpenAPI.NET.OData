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
    public class HttpTests
    {
        [Fact]
        public void SchemeTypeKindSetCorrectly()
        {
            // Arrange
            Http http = new Http();

            // Act & Assert
            Assert.Equal(SecuritySchemeType.Http, http.SchemeType);
        }

        [Fact]
        public void InitializeThrowArgumentNullRecord()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>("record", () => new Http().Initialize(record: null));
        }

        [Fact]
        public void InitializeHttpWithRecordSuccess()
        {
            // Arrange
            IEdmRecordExpression record = new EdmRecordExpression(
                new EdmPropertyConstructor("Name", new EdmStringConstant("HttpWork")),
                new EdmPropertyConstructor("Description", new EdmStringConstant("Description of the scheme")),
                new EdmPropertyConstructor("Scheme", new EdmStringConstant("Authorization scheme")),
                new EdmPropertyConstructor("BearerFormat", new EdmStringConstant("Format of the bearer token")));

            Http http = new Http();
            Assert.Null(http.Name);
            Assert.Null(http.Description);
            Assert.Null(http.Scheme);
            Assert.Null(http.BearerFormat);

            // Act
            http.Initialize(record);

            // Assert
            Assert.Equal("HttpWork", http.Name);
            Assert.Equal("Description of the scheme", http.Description);
            Assert.Equal("Authorization scheme", http.Scheme);
            Assert.Equal("Format of the bearer token", http.BearerFormat);
        }

        [Fact]
        public void InitializeHttpWorksWithCsdl()
        {
            // Arrange
            string annotation = @"<Annotation Term=""NS.MyHttp"">
                <Record >
                  <PropertyValue Property=""Name"" String=""HttpWork"" />
                  <PropertyValue Property=""Scheme"" String=""Authorization scheme"" />
                  <PropertyValue Property=""BearerFormat"" String=""Format of the bearer token"" />
                </Record>
              </Annotation>";

            IEdmModel model = GetEdmModel(annotation);
            Assert.NotNull(model); // guard
            Assert.NotNull(model.EntityContainer);

            // Act
            Http http = model.GetRecord<Http>(model.EntityContainer, "NS.MyHttp");

            // Assert
            Assert.NotNull(http);
            Assert.Equal("HttpWork", http.Name);
            Assert.Null(http.Description);
            Assert.Equal("Authorization scheme", http.Scheme);
            Assert.Equal("Format of the bearer token", http.BearerFormat);
        }

        private IEdmModel GetEdmModel(string annotation)
        {
            const string template = @"<edmx:Edmx Version=""4.0"" xmlns:edmx=""http://docs.oasis-open.org/odata/ns/edmx"">
  <edmx:DataServices>
    <Schema Namespace=""NS"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
      <EntityContainer Name=""Container"">
        {0}
      </EntityContainer>
      <Term Name=""MyHttp"" Type=""Org.OData.Authorization.V1.Http"" />
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
