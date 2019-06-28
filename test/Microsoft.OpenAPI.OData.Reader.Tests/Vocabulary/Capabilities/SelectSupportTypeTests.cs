// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Xml.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;
using Xunit;

namespace Microsoft.OpenApi.OData.Reader.Vocabulary.Capabilities.Tests
{
    public class SelectSupportTypeTests
    {
        [Fact]
        public void TermAttributeAttachedOnSelectSupportType()
        {
            // Arrange & Act
            string qualifiedName = Utils.GetTermQualifiedName<SelectSupportType>();

            // Assert
            Assert.Equal("Org.OData.Capabilities.V1.SelectSupport", qualifiedName);
        }

        [Fact]
        public void InitializSelectSupportTypeWithRecordSuccess()
        {
            // Assert
            IEdmRecordExpression record = new EdmRecordExpression(
                    new EdmPropertyConstructor("Supported", new EdmBooleanConstant(true)),
                    new EdmPropertyConstructor("Filterable", new EdmBooleanConstant(true)),
                    new EdmPropertyConstructor("Expandable", new EdmBooleanConstant(false)),
                    new EdmPropertyConstructor("Searchable", new EdmBooleanConstant(true))
                    // TopSupported
                    // SkipSupported
                    // ComputeSupported
                    // Countable
                    // Sortable
                    );

            // Act
            SelectSupportType select = new SelectSupportType();
            select.Initialize(record);

            // Assert
            VerifySelectSupportType(select);
        }

        [Fact]
        public void InitializeSelectSupportTypeTypeWorksWithCsdl()
        {
            // Arrange
            string annotation = @"<Annotation Term=""Org.OData.Capabilities.V1.SelectSupport"">
                <Record>
                  <PropertyValue Property=""Supported"" Bool=""true"" />
                  <PropertyValue Property=""Filterable"" Bool=""true"" />
                  <PropertyValue Property=""Expandable"" Bool=""false"" />
                  <PropertyValue Property=""Searchable"" Bool=""true"" />
                </Record>
              </Annotation>";

            IEdmModel model = GetEdmModel(annotation);
            Assert.NotNull(model); // guard
            Assert.NotNull(model.EntityContainer);

            // Act
            SelectSupportType select = model.GetRecord<SelectSupportType>(model.EntityContainer);

            // Assert
            VerifySelectSupportType(select);
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

        private static void VerifySelectSupportType(SelectSupportType select)
        {
            Assert.NotNull(select);

            Assert.Null(select.TopSupported);
            Assert.Null(select.SkipSupported);
            Assert.Null(select.ComputeSupported);
            Assert.Null(select.Countable);
            Assert.Null(select.Sortable);

            Assert.True(select.Supported);
            Assert.False(select.Expandable);
            Assert.True(select.Filterable);
            Assert.True(select.Searchable);
        }
    }
}
