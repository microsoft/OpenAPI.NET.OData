//---------------------------------------------------------------------
// <copyright file="OpenApiPathItemTest.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System.Collections.Generic;
using Xunit;

namespace Microsoft.OData.OpenAPI.Tests
{
    public class OpenApiPathItemTest
    {
        private OpenApiPathItem _pathItem = new OpenApiPathItem
        {
            Get = new OpenApiOperation
            {
                Description = "Returns pets based on ID",
                Summary = "Find pets by ID",
                OperationId = "getPetsById",
                Responses = new OpenApiResponses
                {
                    {
                        "200",
                        new OpenApiResponse
                        {
                            Description = "pet response",
                            Content = new Dictionary<string, OpenApiMediaType>
                            {
                                {
                                    "*/*",
                                    new OpenApiMediaType
                                    {
                                        Schema = new OpenApiSchema
                                        {
                                            Type = "array",
                                            Items = new OpenApiSchema
                                            {
                                                Reference = new OpenApiReference("#/components/schemas/pet")
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    {
                        "default",
                        new OpenApiResponse
                        {
                            Description = "error payload",
                            Content = new Dictionary<string, OpenApiMediaType>
                            {
                                {
                                    "text/html",
                                    new OpenApiMediaType
                                    {
                                        Schema = new OpenApiSchema
                                        {
                                            Reference = new OpenApiReference("#/components/schemas/ErrorModel")
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
                Parameters = new List<OpenApiParameter>
                {
                    new OpenApiParameter
                    {
                        Name = "id",
                        Description = "ID of pet to use",
                        In = ParameterLocation.path,
                        Required = true,
                        Schema = new OpenApiSchema
                        {
                            Type = "array",
                            Items = new OpenApiSchema
                            {
                                Type = "string"
                            }
                        },
                        Style = ParameterStyle.simple
                    }
                }
            }
        };

        [Fact]
        public void CanWritePathItemObjectToJson()
        {
            // Arrange
            string expect = @"
{
  ""get"": {
    ""summary"": ""Find pets by ID"",
    ""description"": ""Returns pets based on ID"",
    ""operationId"": ""getPetsById"",
    ""parameters"": [
      {
        ""name"": ""id"",
        ""in"": ""path"",
        ""description"": ""ID of pet to use"",
        ""required"": true,
        ""style"": ""simple"",
        ""schema"": {
          ""type"": ""array"",
          ""items"": {
            ""type"": ""string""
          }
        }
      }
    ],
    ""responses"": {
      ""200"": {
        ""description"": ""pet response"",
        ""content"": {
          ""*/*"": {
            ""schema"": {
              ""type"": ""array"",
              ""items"": {
                ""$ref"": ""#/components/schemas/pet""
              }
            }
          }
        }
      },
      ""default"": {
        ""description"": ""error payload"",
        ""content"": {
          ""text/html"": {
            ""schema"": {
              ""$ref"": ""#/components/schemas/ErrorModel""
            }
          }
        }
      }
    }
  }
}".Replace();

            // Act & Assert
            Assert.Equal(expect, _pathItem.WriteToJson());
        }

        [Fact]
        public void CanWritePathItemObjectToYaml()
        {
            // Arrange
            string expect = @"
get:
  summary: Find pets by ID
  description: Returns pets based on ID
  operationId: getPetsById
  parameters:
    - name: id
      in: path
      description: ID of pet to use
      required: true
      style: simple
      schema:
        type: array
        items:
          type: string
  responses:
    200:
      description: pet response
      content:
        */*:
          schema:
            type: array
            items:
              $ref: '#/components/schemas/pet'
    default:
      description: error payload
      content:
        text/html:
          schema:
            $ref: '#/components/schemas/ErrorModel'
".Replace();

            // Act & Assert
            Assert.Equal(expect, _pathItem.WriteToYaml());
        }
    }
}
