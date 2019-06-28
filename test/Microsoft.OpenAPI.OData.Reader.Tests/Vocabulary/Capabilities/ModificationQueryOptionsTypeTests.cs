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
    public class ModificationQueryOptionsTypeTests
    {
        [Fact]
        public void TermAttributeAttachedOnModificationQueryOptionsType()
        {
            // Arrange & Act
            string qualifiedName = Utils.GetTermQualifiedName<ModificationQueryOptionsType>();

            // Assert
            Assert.Equal("Org.OData.Capabilities.V1.ModificationQueryOptions", qualifiedName);
        }

        [Fact]
        public void InitializInsertRestrictionsTypeWithRecordSuccess()
        {
            // Assert
            IEdmRecordExpression record = new EdmRecordExpression(
                    new EdmPropertyConstructor("ExpandSupported", new EdmBooleanConstant(true)),
                    new EdmPropertyConstructor("SelectSupported", new EdmBooleanConstant(true)),
                    new EdmPropertyConstructor("ComputeSupported", new EdmBooleanConstant(false)),
                    new EdmPropertyConstructor("FilterSupported", new EdmBooleanConstant(true))
                    // SearchSupported
                    // SortSupported
                    );

            // Act
            ModificationQueryOptionsType query = new ModificationQueryOptionsType();
            query.Initialize(record);

            // Assert
            VerifyModificationQueryOptions(query);
        }

        [Fact]
        public void InitializeModificationQueryOptionsTypeWorksWithCsdl()
        {
            // Arrange
            string annotation = @"<Annotation Term=""Org.OData.Capabilities.V1.ModificationQueryOptions"">
                <Record>
                  <PropertyValue Property=""ExpandSupported"" Bool=""true"" />
                  <PropertyValue Property=""SelectSupported"" Bool=""true"" />
                  <PropertyValue Property=""ComputeSupported"" Bool=""false"" />
                  <PropertyValue Property=""FilterSupported"" Bool=""true"" />
                </Record>
              </Annotation>";

            IEdmModel model = GetEdmModel(annotation);
            Assert.NotNull(model); // guard
            Assert.NotNull(model.EntityContainer);

            // Act
            ModificationQueryOptionsType query = model.GetRecord<ModificationQueryOptionsType>(model.EntityContainer);

            // Assert
            VerifyModificationQueryOptions(query);
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

        private static void VerifyModificationQueryOptions(ModificationQueryOptionsType query)
        {
            Assert.NotNull(query);

            Assert.Null(query.SearchSupported);
            Assert.Null(query.SortSupported);

            Assert.NotNull(query.ExpandSupported);
            Assert.True(query.ExpandSupported);

            Assert.NotNull(query.SelectSupported);
            Assert.True(query.SelectSupported);

            Assert.NotNull(query.ComputeSupported);
            Assert.False(query.ComputeSupported); // false

            Assert.NotNull(query.FilterSupported);
            Assert.True(query.FilterSupported);
        }
    }
}
