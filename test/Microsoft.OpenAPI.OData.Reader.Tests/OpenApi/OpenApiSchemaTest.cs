// <copyright file="OpenApiSchemaTest.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.OData.OpenAPI.Tests
{
    public class OpenApiSchemaTest
    {
        #region Primitive Sample

        private OpenApiSchema _primitiveSample = new OpenApiSchema
        {
            Type = "string",
            Format = "email"
        };

        [Fact]
        public void CanWritePrimitiveSchemaSampleToJson()
        {
            // Arrange
            string expect = @"
{
  ""type"": ""string"",
  ""format"": ""email""
}
".Replace();

            // Act & Assert
            Assert.Equal(expect, _primitiveSample.WriteToJson());
        }

        [Fact]
        public void CanWritePrimitiveSchemaSampleToYaml()
        {
            // Arrange
            string expect = @"
type: string
format: email
".Replace();

            // Act & Assert
            Assert.Equal(expect, _primitiveSample.WriteToYaml());
        }

        #endregion

        #region Simple Model

        private OpenApiSchema _simpleModel = new OpenApiSchema
        {
            Type = "object",
            Required = new List<string> { "name" },
            Properties = new Dictionary<string, OpenApiSchema>
            {
                {
                    "name",
                    new OpenApiSchema
                    {
                        Type = "string"
                    }
                },
                {
                    "address",
                    new OpenApiSchema
                    {
                        Reference = new OpenApiReference("#/components/schemas/Address")
                    }
                },
                {
                    "age",
                    new OpenApiSchema
                    {
                        Type = "integer",
                        Format = "int32",
                        Minimum = 0
                    }
                }
            }
        };

        [Fact]
        public void CanWriteSimpleModelSchemaToJson()
        {
            // Arrange
            string expect = @"
{
  ""type"": ""object"",
  ""required"": [
    ""name""
  ],
  ""properties"": {
    ""name"": {
      ""type"": ""string""
    },
    ""address"": {
      ""$ref"": ""#/components/schemas/Address""
    },
    ""age"": {
      ""type"": ""integer"",
      ""format"": ""int32"",
      ""minimum"": 0
    }
  }
}
".Replace();

            // Act & Assert
            Assert.Equal(expect, _simpleModel.WriteToJson());
        }

        [Fact]
        public void CanWriteSimpleModelSchemaToYaml()
        {
            // Arrange
            string expect = @"
type: object
required:
  - name
properties:
  name:
    type: string
  address:
    $ref: '#/components/schemas/Address'
  age:
    type: integer
    format: int32
    minimum: 0
".Replace();

            // Act & Assert
            Assert.Equal(expect, _simpleModel.WriteToYaml());
        }

        #endregion

        #region Simple String to String Mapping

        private OpenApiSchema _simpleStringToStringMapping = new OpenApiSchema
        {
            Type = "object",
            AdditionalProperties = new OpenApiSchema
            {
                Type = "string"
            }
        };

        [Fact]
        public void CanWriteSimpleStringToStringMappingSchemaToJson()
        {
            // Arrange
            string expect = @"
{
  ""type"": ""object"",
  ""additionalProperties"": {
    ""type"": ""string""
  }
}".Replace();

            // Act & Assert
            Assert.Equal(expect, _simpleStringToStringMapping.WriteToJson());
        }

        [Fact]
        public void CanWriteSimpleStringToStringMappingSchemaToYaml()
        {
            // Arrange
            string expect = @"
type: object
additionalProperties:
  type: string
".Replace();

            // Act & Assert
            Assert.Equal(expect, _simpleStringToStringMapping.WriteToYaml());
        }

        #endregion

        #region String to Model mapping

        private OpenApiSchema _simpleStringToModelMapping = new OpenApiSchema
        {
            Type = "object",
            AdditionalProperties = new OpenApiSchema
            {
                Reference = new OpenApiReference("#/components/schemas/ComplexModel")
            }
        };

        [Fact]
        public void CanWriteSimpleStringToModelMappingSchemaToJson()
        {
            // Arrange
            string expect = @"
{
  ""type"": ""object"",
  ""additionalProperties"": {
    ""$ref"": ""#/components/schemas/ComplexModel""
  }
}".Replace();

            // Act & Assert
            Assert.Equal(expect, _simpleStringToModelMapping.WriteToJson());
        }

        [Fact]
        public void CanWriteSimpleStringToModelMappingSchemaToYaml()
        {
            // Arrange
            string expect = @"
type: object
additionalProperties:
  $ref: '#/components/schemas/ComplexModel'
".Replace();

            // Act & Assert
            Assert.Equal(expect, _simpleStringToModelMapping.WriteToYaml());
        }

        #endregion

        #region Model with Example

        private OpenApiSchema _modelWithExampleMapping = new OpenApiSchema
        {
            Type = "object",
            Example = new OpenApiAny
            {
                { "name", "Puma" },
                { "id", 1 }
            }
        };

        [Fact]
        public void CanWriteModelWithExampleSchemaToJson()
        {
            // Arrange
            string expect = @"
{
  ""type"": ""object"",
  ""example"": {
    ""name"": ""Puma"",
    ""id"": 1
  }
}".Replace();

            // Act & Assert
            Assert.Equal(expect, _modelWithExampleMapping.WriteToJson());
        }

        [Fact]
        public void CanWriteModelWithExampleSchemaToYaml()
        {
            // Arrange
            string expect = @"
type: object
example:
  name: Puma
  id: 1
".Replace();

            // Act & Assert
            Assert.Equal(expect, _modelWithExampleMapping.WriteToYaml());
        }

        #endregion

        #region Model with Composition

        private OpenApiSchema _modelWithCompositionMapping = new OpenApiSchema
        {
            Type = "object",
            Required = new List<string> { "message", "code" },
            Properties = new Dictionary<string, OpenApiSchema>
            {
                {
                    "message",
                    new OpenApiSchema
                    {
                        Type = "string"
                    }
                },
                {
                    "code",
                    new OpenApiSchema
                    {
                        Type = "integer",
                        Minimum = 100,
                        Maximum = 600
                    }
                }
            },
            AllOf = new List<OpenApiSchema>
            {
                new OpenApiSchema
                {
                    Reference = new OpenApiReference("#/components/schemas/ErrorModel")
                },
                new OpenApiSchema
                {
                    Type = "object",
                    Required = new List<string> { "rootCause" },
                    Properties = new Dictionary<string, OpenApiSchema>
                    {
                        {
                            "rootCause",
                            new OpenApiSchema
                            {
                                Type = "string"
                            }
                        }
                    }
                }
            }
        };

        [Fact]
        public void CanWriteModelWithCompositionSchemaToJson()
        {
            // Arrange
            string expect = @"
{
  ""type"": ""object"",
  ""required"": [
    ""message"",
    ""code""
  ],
  ""allOf"": [
    {
      ""$ref"": ""#/components/schemas/ErrorModel""
    },
    {
      ""type"": ""object"",
      ""required"": [
        ""rootCause""
      ],
      ""properties"": {
        ""rootCause"": {
          ""type"": ""string""
        }
      }
    }
  ],
  ""properties"": {
    ""message"": {
      ""type"": ""string""
    },
    ""code"": {
      ""type"": ""integer"",
      ""maximum"": 600,
      ""minimum"": 100
    }
  }
}".Replace();


            // Act & Assert
            Assert.Equal(expect, _modelWithCompositionMapping.WriteToJson());
        }

        [Fact]
        public void CanWriteModelWithCompositionSchemaToYaml()
        {
            // Arrange
            string expect = @"
type: object
required:
  - message
  - code
allOf:
  - $ref: '#/components/schemas/ErrorModel',
  - type: object
    required:
      - rootCause
    properties:
      rootCause:
        type: string
properties:
  message:
    type: string
  code:
    type: integer
    maximum: 600
    minimum: 100
".Replace();

            // Act & Assert
            Assert.Equal(expect, _modelWithCompositionMapping.WriteToYaml());
        }

        #endregion
    }
}
