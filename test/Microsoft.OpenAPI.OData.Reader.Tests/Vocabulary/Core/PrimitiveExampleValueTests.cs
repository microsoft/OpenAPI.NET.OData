// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Vocabulary.Core;
using Xunit;

namespace Microsoft.OpenApi.OData.Reader.Vocabulary.Core.Tests
{
    public class PrimitiveExampleValueTests
    {
        [Fact]
        public void DefaultPropertyAsNull()
        {
            // Arrange
            PrimitiveExampleValue value = new PrimitiveExampleValue();

            //  Act & Assert
            Assert.Null(value.Description);
            Assert.Null(value.Value);
        }

        [Fact]
        public void InitializeWithNullRecordThrows()
        {
            // Arrange & Act
            PrimitiveExampleValue value = new PrimitiveExampleValue();

            // Assert
            Assert.Throws<ArgumentNullException>("record", () => value.Initialize(record: null));
        }

        [Fact]
        public void InitializeWithPrimitiveValueRecordSuccess()
        {
            // Arrange
            IEdmRecordExpression record = new EdmRecordExpression(
                new EdmPropertyConstructor("Description", new EdmStringConstant("HelloWorld!")),
                new EdmPropertyConstructor("Value", new EdmBooleanConstant(true)));
            PrimitiveExampleValue value = new PrimitiveExampleValue();
            Assert.Null(value.Description);
            Assert.Null(value.Value);

            // Act
            value.Initialize(record);

            // Assert
            Assert.NotNull(value.Description);
            Assert.Equal("HelloWorld!", value.Description);
            Assert.NotNull(value.Value);

            Assert.NotNull(value.Value.Value);
            Assert.Equal(true, value.Value.Value);
        }

        public static IEnumerable<object[]> PrimitiveData =>
            new List<object[]>
            {
                new object[] { @"String=""Hello World""", "Hello World" },
                new object[] { @"Int=""42""", (long)42 },
                new object[] { @"Bool=""true""", true },
                new object[] { @"Bool=""false""", false },
                new object[] { @"TimeOfDay=""15:38:25.1090000""", new TimeOnly(15, 38, 25, 109) },
                new object[] { @"Date=""2014-10-13""", new DateOnly(2014, 10, 13) },
                new object[] { @"Duration=""PT0S""", new TimeSpan() },
                // new object[] { @"Binary=""AQ==""", new byte[] { 1 }, }, has problem in ODL?
                new object[] { @"Float=""3.14""", 3.14 },
                new object[] { @"Decimal=""3.14""", 3.14m },
                new object[] { @"DateTimeOffset=""0001-01-01T00:00:00Z""", new DateTimeOffset() },
                new object[] { @"Guid=""21EC2020-3AEA-1069-A2DD-08002B30309D""", new Guid("21EC2020-3AEA-1069-A2DD-08002B30309D") },
            };

        [Theory]
        [MemberData(nameof(PrimitiveData))]
        public void PrimitiveExamplevalueInitializeWorksForPrimitiveData(string data, object except)
        {
            // Arrange
            string annotation = $@"<Annotation Term=""Org.OData.Core.V1.Example"">
                <Record Type=""Org.OData.Core.V1.PrimitiveExampleValue"">
                  <PropertyValue Property=""Description"" String=""Primitive example value"" />
                  <PropertyValue Property=""Value"" {data} />
                </Record>
              </Annotation>";

            IEdmModel model = GetEdmModel(annotation);
            Assert.NotNull(model); // guard

            IEdmEntityType customer = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Customer");
            Assert.NotNull(customer); // guard
            IEdmProperty dataProperty = customer.FindProperty("Data");
            Assert.NotNull(dataProperty);

            // Act
            PrimitiveExampleValue value = model.GetRecord<PrimitiveExampleValue>(dataProperty, "Org.OData.Core.V1.Example");

            // Assert
            Assert.NotNull(value);
            Assert.Equal("Primitive example value", value.Description);
            Assert.NotNull(value.Value);
            switch (except)
            {
              case DateOnly dateOnly:
                Assert.Equal(dateOnly.ToString("yyyy-MM-dd"), value.Value.Value.ToString());
                break;
              case TimeOnly timeOnly:
                Assert.Equal(timeOnly.ToString("HH:mm:ss.fffffff"), value.Value.Value.ToString());
                break;
              default:
                Assert.Equal(except, value.Value.Value);
              break;
            }
        }

        private IEdmModel GetEdmModel(string annotation)
        {
            const string template = @"<edmx:Edmx Version=""4.0"" xmlns:edmx=""http://docs.oasis-open.org/odata/ns/edmx"">
  <edmx:DataServices>
    <Schema Namespace=""NS"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
      <EntityType Name=""Customer"">
        <Key>
          <PropertyRef Name=""ID"" />
        </Key>
        <Property Name=""ID"" Type=""Edm.Int32"" Nullable=""false"" />
        <Property Name=""Data"" Type=""Edm.PrimitiveType"" Nullable=""false"" >
          {0}
        </Property>
      </EntityType>
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
