// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.IO;
using System.Xml;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;
using Microsoft.OpenApi.OData.Vocabulary.Core;
using Xunit;

namespace Microsoft.OpenApi.OData.Reader.Vocabulary.Capabilities.Tests
{
    public class CustomParameterTests
    {
        [Fact]
        public void InitializeCustomParameterWithRecordSuccess()
        {
            // Assert
            IEdmRecordExpression record = new EdmRecordExpression(
                new EdmPropertyConstructor("Name", new EdmStringConstant("odata-debug")),
                new EdmPropertyConstructor("Description", new EdmStringConstant("Debug support for OData services")),
                new EdmPropertyConstructor("DocumentationURL", new EdmStringConstant("https://debug.html")),
                new EdmPropertyConstructor("Required", new EdmBooleanConstant(false)),
                new EdmPropertyConstructor("ExampleValues",
                    new EdmCollectionExpression(
                        new EdmRecordExpression(new EdmPropertyConstructor("Value", new EdmStringConstant("html"))),
                        new EdmRecordExpression(new EdmPropertyConstructor("Value", new EdmTimeOfDayConstant(new TimeOnly(3, 4, 5, 6)))))));

            // Act
            CustomParameter parameter = new CustomParameter();
            parameter.Initialize(record);

            // Assert
            VerifyCustomParameter(parameter);
        }

        [Fact]
        public void InitializeCountRestrictionsWorksWithCsdl()
        {
            // Arrange
            string annotation = @"
                <Annotation Term=""NS.MyCustomParameter"" >
                  <Record>
                    <PropertyValue Property=""Name"" String=""odata-debug"" />
                    <PropertyValue Property=""Description"" String=""Debug support for OData services"" />
                    <PropertyValue Property=""DocumentationURL"" String=""https://debug.html"" />
                    <PropertyValue Property=""Required"" Bool=""false"" />
                    <PropertyValue Property=""ExampleValues"">
                      <Collection>
                        <Record>
                          <PropertyValue Property=""Value"" String=""html"" />
                        </Record>
                        <Record>
                          <PropertyValue Property=""Value"" TimeOfDay=""3:4:5.006"" />
                        </Record>
                      </Collection>
                    </PropertyValue>
                  </Record>
                </Annotation>";

            IEdmModel model = GetEdmModel(annotation);
            Assert.NotNull(model); // guard

            // Act
            CustomParameter count = model.GetRecord<CustomParameter>(model.EntityContainer, "NS.MyCustomParameter");

            // Assert
            VerifyCustomParameter(count);
        }

        private static void VerifyCustomParameter(CustomParameter parameter)
        {
            Assert.NotNull(parameter);

            Assert.NotNull(parameter.Name);
            Assert.Equal("odata-debug", parameter.Name);

            Assert.NotNull(parameter.Description);
            Assert.Equal("Debug support for OData services", parameter.Description);

            Assert.NotNull(parameter.DocumentationURL);
            Assert.Equal("https://debug.html", parameter.DocumentationURL);

            Assert.NotNull(parameter.Required);
            Assert.False(parameter.Required.Value);

            Assert.NotNull(parameter.ExampleValues);
            Assert.Equal(2, parameter.ExampleValues.Count);

            // #1
            PrimitiveExampleValue value = parameter.ExampleValues[0];
            Assert.Null(value.Description);
            Assert.NotNull(value.Value);
            Assert.Equal("html", value.Value.Value);

            // #2
            value = parameter.ExampleValues[1];
            Assert.Null(value.Description);
            Assert.NotNull(value.Value);
            Assert.Equal(new TimeOnly(3, 4, 5, 6).ToString("HH:mm:ss.fffffff"), value.Value.Value.ToString());
        }

        private IEdmModel GetEdmModel(string annotation)
        {
            const string template = @"<?xml version=""1.0"" encoding=""utf-16""?>
<Schema Namespace=""NS"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
  <EntityContainer Name=""Container"">
   {0}
  </EntityContainer>
  <Term Name=""MyCustomParameter"" Type=""Org.OData.Capabilities.V1.CustomParameter"" />
</Schema>
";
            string schema = string.Format(template, annotation);

            IEdmModel model;
            bool result = SchemaReader.TryParse(new XmlReader[] { XmlReader.Create(new StringReader(schema)) }, out model, out _);
            Assert.True(result);
            return model;
        }
    }
}
