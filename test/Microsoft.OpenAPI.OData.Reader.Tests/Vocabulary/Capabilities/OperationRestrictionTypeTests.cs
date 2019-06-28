// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Linq;
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
    public class OperationRestrictionTypeTests
    {
        [Fact]
        public void TermAttributeAttachedOnOperationRestrictionType()
        {
            // Arrange & Act
            string qualifiedName = Utils.GetTermQualifiedName<OperationRestrictionType>();

            // Assert
            Assert.Equal("Org.OData.Capabilities.V1.OperationRestrictions", qualifiedName);
        }

        [Fact]
        public void InitializOperationRestrictionTypeWithRecordSuccess()
        {
            // Assert
            IEdmRecordExpression record = new EdmRecordExpression(
                    new EdmPropertyConstructor("CustomHeaders", new EdmCollectionExpression(
                        new EdmRecordExpression(
                            new EdmPropertyConstructor("Name", new EdmStringConstant("head name")),
                            new EdmPropertyConstructor("Description", new EdmStringConstant("head desc")),
                            new EdmPropertyConstructor("DocumentationURL", new EdmStringConstant("http://any3")),
                            new EdmPropertyConstructor("Required", new EdmBooleanConstant(true)))))
                    // Permission
                    // CustomQueryOptions
                    );

            // Act
            OperationRestrictionType operation = new OperationRestrictionType();
            operation.Initialize(record);

            // Assert
            VerifyOperationRestrictions(operation);
        }

        [Fact]
        public void InitializeModificationQueryOptionsTypeWorksWithCsdl()
        {
            // Arrange
            string annotation = @"<Annotation Term=""NS.MyOperationRestriction"">
                <Record>
                  <PropertyValue Property=""CustomHeaders"" >
                    <Collection>
                      <Record>
                        <PropertyValue Property=""Name"" String=""head name"" />
                        <PropertyValue Property=""Description"" String=""head desc"" />
                        <PropertyValue Property=""DocumentationURL"" String=""http://any3"" />
                        <PropertyValue Property=""Required"" Bool=""true"" />
                      </Record>
                    </Collection>
                  </PropertyValue>
                </Record>
              </Annotation>";

            IEdmModel model = GetEdmModel(annotation);
            Assert.NotNull(model); // guard
            IEdmOperation edmOperation = model.SchemaElements.OfType<IEdmFunction>().First();
            Assert.NotNull(edmOperation);

            // Act
            OperationRestrictionType operation = model.GetRecord<OperationRestrictionType>(edmOperation, "NS.MyOperationRestriction");

            // Assert
            VerifyOperationRestrictions(operation);
        }

        private IEdmModel GetEdmModel(string annotation)
        {
            const string template = @"<edmx:Edmx Version=""4.0"" xmlns:edmx=""http://docs.oasis-open.org/odata/ns/edmx"">
  <edmx:DataServices>
    <Schema Namespace=""NS"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
      <Function Name=""delta"" IsBound=""true"" >
         <Parameter Name=""bindingParameter"" Type=""Edm.String"" />
         <ReturnType Type=""Edm.String"" />
         {0}
      </Function>
    <Term Name=""MyOperationRestriction"" Type=""Org.OData.Capabilities.V1.OperationRestriction"" />
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>";

            string modelText = string.Format(template, annotation);
            IEdmModel model;
            bool result = CsdlReader.TryParse(XElement.Parse(modelText).CreateReader(), out model, out _);
            Assert.True(result);
            return model;
        }

        private static void VerifyOperationRestrictions(OperationRestrictionType operation)
        {
            Assert.NotNull(operation);

            Assert.Null(operation.Permission);
            Assert.Null(operation.CustomQueryOptions);

            Assert.NotNull(operation.CustomHeaders);

            CustomParameter parameter = Assert.Single(operation.CustomHeaders);
            Assert.Equal("head name", parameter.Name);
            Assert.Equal("http://any3", parameter.DocumentationURL);
            Assert.Equal("head desc", parameter.Description);
            Assert.True(parameter.Required.Value);
            Assert.Null(parameter.ExampleValues);
        }
    }
}
