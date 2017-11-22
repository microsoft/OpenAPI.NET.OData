// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Generator;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.OpenApi.OData.Tests
{
    public class OpenApiSchemaGeneratorTest
    {
        private ITestOutputHelper _output;
        public OpenApiSchemaGeneratorTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void NonNullableBooleanPropertyWithDefaultValueWorks()
        {
            // Arrange
            EdmEntityType entitType = new EdmEntityType("NS", "Entity");
            IEdmStructuralProperty property = new EdmStructuralProperty(
                entitType, "BooleanValue", EdmCoreModel.Instance.GetBoolean(false), "false");

            // Act
            var schema = property.CreatePropertySchema();

            // Assert
            Assert.NotNull(schema);
            Assert.Equal("boolean", schema.Type);

            string json = schema.SerializeAsJson();
            Assert.Equal(@"{
  ""type"": ""boolean"",
  ""default"": false
}".Replace(), json);
        }

        [Fact]
        public void NonNullableBinaryPropertyWithBothMaxLengthAndDefaultValueWorks()
        {
            // Arrange
            EdmEntityType entitType = new EdmEntityType("NS", "Entity");
            var binaryType = new EdmBinaryTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Binary),
                false, false, 44);
            IEdmStructuralProperty property = new EdmStructuralProperty(
                entitType, "BinaryValue", binaryType, "T0RhdGE");

            // Act
            var schema = property.CreatePropertySchema();

            // Assert
            Assert.NotNull(schema);
            Assert.Equal("string", schema.Type);

            string json = schema.SerializeAsJson();
            Assert.Equal(@"{
  ""maxLength"": 44,
  ""type"": ""string"",
  ""format"": ""base64url"",
  ""default"": ""T0RhdGE""
}".Replace(), json);
        }

        [Fact]
        public void NonNullableIntegerPropertyWithDefaultValueWorks()
        {
            // Arrange
            EdmEntityType entitType = new EdmEntityType("NS", "Entity");
            IEdmStructuralProperty property = new EdmStructuralProperty(
                entitType, "IntegerValue", EdmCoreModel.Instance.GetInt32(false), "-128");

            // Act
            var schema = property.CreatePropertySchema();

            // Assert
            Assert.NotNull(schema);
            Assert.Equal("integer", schema.Type);

            string json = schema.SerializeAsJson();
            Assert.Equal(@"{
  ""maximum"": 2147483647,
  ""minimum"": -2147483648,
  ""type"": ""integer"",
  ""format"": ""int32"",
  ""default"": -128
}".Replace(), json);
        }

        [Fact]
        public void NonNullableDoublePropertyWithDefaultStringWorks()
        {
            // Arrange
            EdmEntityType entitType = new EdmEntityType("NS", "Entity");
            IEdmStructuralProperty property = new EdmStructuralProperty(
                entitType, "DoubleValue", EdmCoreModel.Instance.GetDouble(false), "3.1415926535897931");

            // Act
            var schema = property.CreatePropertySchema();

            // Assert
            Assert.NotNull(schema);
            Assert.Null(schema.Type);

            string json = schema.SerializeAsJson();

            Assert.Equal(@"{
  ""oneOf"": [
    {
      ""type"": ""number""
    },
    {
      ""type"": ""string""
    },
    {
      ""enum"": [
        ""-INF"",
        ""INF"",
        ""NaN""
      ]
    }
  ],
  ""format"": ""double"",
  ""default"": ""3.1415926535897931""
}".Replace(), json);
        }
    }
}
