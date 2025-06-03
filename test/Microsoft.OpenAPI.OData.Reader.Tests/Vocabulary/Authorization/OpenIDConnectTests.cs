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
    public class OpenIDConnectTests
    {
        [Fact]
        public void SchemeTypeKindSetCorrectly()
        {
            // Arrange
            OpenIDConnect openIDConnect = new OpenIDConnect();

            // Act & Assert
            Assert.Equal(SecuritySchemeType.OpenIdConnect, openIDConnect.SchemeType);
        }

        [Fact]
        public void InitializeThrowArgumentNullRecord()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>("record", () => new OpenIDConnect().Initialize(record: null));
        }

        [Fact]
        public void InitializeOpenIDConnectWithRecordSuccess()
        {
            // Arrange
            IEdmRecordExpression record = new EdmRecordExpression(
                new EdmPropertyConstructor("Name", new EdmStringConstant("OpenIDConnectWork")),
                new EdmPropertyConstructor("IssuerUrl", new EdmStringConstant("http://any")));

            OpenIDConnect idConnection = new OpenIDConnect();
            Assert.Null(idConnection.Name);
            Assert.Null(idConnection.Description);
            Assert.Null(idConnection.IssuerUrl);

            // Act
            idConnection.Initialize(record);

            // Assert
            Assert.Equal("OpenIDConnectWork", idConnection.Name);
            Assert.Null(idConnection.Description);
            Assert.Equal("http://any", idConnection.IssuerUrl);
        }

        [Fact]
        public void InitializeOpenIDConnectWorksWithCsdl()
        {
            // Arrange
            string annotation = @"<Annotation Term=""NS.MyOpenIDConnect"">
                <Record >
                  <PropertyValue Property=""IssuerUrl"" String=""http://any"" />
                </Record>
              </Annotation>";

            IEdmModel model = GetEdmModel(annotation);
            Assert.NotNull(model); // guard
            Assert.NotNull(model.EntityContainer);

            // Act
            OpenIDConnect idConnection = model.GetRecord<OpenIDConnect>(model.EntityContainer, "NS.MyOpenIDConnect");

            // Assert
            Assert.Null(idConnection.Name);
            Assert.Null(idConnection.Description);
            Assert.Equal("http://any", idConnection.IssuerUrl);
        }

        private IEdmModel GetEdmModel(string annotation)
        {
            const string template = @"<edmx:Edmx Version=""4.0"" xmlns:edmx=""http://docs.oasis-open.org/odata/ns/edmx"">
  <edmx:DataServices>
    <Schema Namespace=""NS"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
      <EntityContainer Name=""Container"">
        {0}
      </EntityContainer>
      <Term Name=""MyOpenIDConnect"" Type=""Org.OData.Authorization.V1.OpenIDConnect"" />
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
