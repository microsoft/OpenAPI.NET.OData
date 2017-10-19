//---------------------------------------------------------------------
// <copyright file="OpenApiResponseTest.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System.Collections.Generic;
using Xunit;

namespace Microsoft.OData.OpenAPI.Tests
{
    public class OpenApiResponseTest
    {
        private OpenApiResponse _responseWithComplex = new OpenApiResponse
        {
            Description = "A complex object array response",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                {
                    "application/json",
                    new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "array",
                            Items = new OpenApiSchema
                            {
                                Reference = new OpenApiReference("#/components/schemas/VeryComplexType")
                            }
                        }
                    }
                }
            }
        };

        [Fact]
        public void CanWriteResonseObjectWithComplexToJson()
        {
            // Arrange
            string expect = @"
{
  ""description"": ""A complex object array response"",
  ""content"": {
    ""application/json"": {
      ""schema"": {
        ""type"": ""array"",
        ""items"": {
          ""$ref"": ""#/components/schemas/VeryComplexType""
        }
      }
    }
  }
}".Replace();

            // Act & Assert
            Assert.Equal(expect, _responseWithComplex.WriteToJson());
        }

        [Fact]
        public void CanWriteResonseObjectWithComplexToYaml()
        {
            // Arrange
            string expect = @"
description: A complex object array response
content:
  application/json:
    schema:
      type: array
      items:
        $ref: '#/components/schemas/VeryComplexType'
".Replace();

            // Act & Assert
            Assert.Equal(expect, _responseWithComplex.WriteToYaml());
        }

        private OpenApiResponse _responseWithStringType = new OpenApiResponse
        {
            Description = "A simple string response",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                {
                    "text/plain",
                    new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "string"
                        }
                    }
                }
            }
        };

        [Fact]
        public void CanWriteResponseObjectWithStringTypeToJson()
        {
            // Arrange
            string expect = @"
{
  ""description"": ""A simple string response"",
  ""content"": {
    ""text/plain"": {
      ""schema"": {
        ""type"": ""string""
      }
    }
  }
}".Replace();

            // Act & Assert
            Assert.Equal(expect, _responseWithStringType.WriteToJson());
        }

        [Fact]
        public void CanWriteResponseObjectWithStringTypeToYaml()
        {
            // Arrange
            string expect = @"
description: A simple string response
content:
  text/plain:
    schema:
      type: string
".Replace();

            // Act & Assert
            Assert.Equal(expect, _responseWithStringType.WriteToYaml());
        }

        private OpenApiResponse _responseWithHeaders = new OpenApiResponse
        {
            Description = "A simple string response",
            Headers = new Dictionary<string, OpenApiHeader>
            {
                // TODO: 
            }
        };
    }
}
