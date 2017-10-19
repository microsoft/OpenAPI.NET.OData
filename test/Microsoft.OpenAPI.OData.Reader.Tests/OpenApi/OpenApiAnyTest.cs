//---------------------------------------------------------------------
// <copyright file="OpenApiAnyTest.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using Microsoft.OData.OpenAPI.Properties;
using Xunit;

namespace Microsoft.OData.OpenAPI.Tests
{
    public class OpenApiAnyTest
    {
        private OpenApiAny _wrongAny = new OpenApiAny
        {
            { "doubleProp", 6.8 }
        };

        [Fact]
        public void WriteNotSupportedValueTypeThrowNotSupportedException()
        {
            // Arrange & Act & Assert
            var jsonExcep = Assert.Throws<OpenApiException>(() => _wrongAny.WriteToJson());

            var yamlExcep = Assert.Throws<OpenApiException>(() => _wrongAny.WriteToYaml());

            Assert.Equal(jsonExcep.Message, yamlExcep.Message);

            Assert.Equal(String.Format(SRResource.OpenApiUnsupportedValueType, "System.Double"),
                jsonExcep.Message);
        }

        private OpenApiAny _basicAny = new OpenApiAny
        {
            { "StringProp", "value" },
            { "nullProp", null },
            { "intProp", 42 },
            { "boolProp", false }
        };

        [Fact]
        public void CanWriteAnyObjectWithBasicValueToJson()
        {
            // Arrange
            string expect = @"
{
  ""StringProp"": ""value"",
  ""nullProp"": null,
  ""intProp"": 42,
  ""boolProp"": false
}".Replace();

            // Act & Assert
            Assert.Equal(expect, _basicAny.WriteToJson());
        }

        [Fact]
        public void CanWriteAnyObjectWithBasicValueToYaml()
        {
            // Arrange
            string expect = @"
StringProp: value
nullProp: 
intProp: 42
boolProp: false
".Replace();

            // Act & Assert
            Assert.Equal(expect, _basicAny.WriteToYaml());
        }

        private OpenApiAny _anyWithWritableElement = new OpenApiAny
        {
            { "stringProp", "value" },
            { "writable", new OpenApiAny
                {
                    { "decimalProp", (decimal)6.8 },
                    { "nullProp", null },
                }
            }
        };

        [Fact]
        public void CanWriteAnyObjectWithWritableElementToJson()
        {
            // Arrange
            string expect = @"
{
  ""stringProp"": ""value"",
  ""writable"": {
    ""decimalProp"": 6.8,
    ""nullProp"": null
  }
}".Replace();

            // Act & Assert
            Assert.Equal(expect, _anyWithWritableElement.WriteToJson());
        }

        [Fact]
        public void CanWriteAnyObjectWithWritableElementToYaml()
        {
            // Arrange
            string expect = @"
stringProp: value
writable:
  decimalProp: 6.8
  nullProp: 
".Replace();

            // Act & Assert
            Assert.Equal(expect, _anyWithWritableElement.WriteToYaml());
        }
    }
}
