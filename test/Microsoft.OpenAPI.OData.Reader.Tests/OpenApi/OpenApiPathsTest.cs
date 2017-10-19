//---------------------------------------------------------------------
// <copyright file="OpenApiPathsTest.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System.Collections.Generic;
using Xunit;

namespace Microsoft.OData.OpenAPI.Tests
{
    public class OpenApiPathsTest
    {
        internal static OpenApiPaths Paths = new OpenApiPaths
        {
            {
                "/pets",
                new OpenApiPathItem
                {
                    Get = new OpenApiOperation
                    {
                        Description = "Returns all pets from the system that the user has access to",
                        Responses = new OpenApiResponses
                        {
                            {
                                "200",
                                new OpenApiResponse
                                {
                                    Description = "A list of pets.",
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
                                                        Reference = new OpenApiReference("#/components/schemas/pet")
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        [Fact]
        public void CanWritePathsObjectToJson()
        {
            // Arrange
            string expect = @"
{
  ""/pets"": {
    ""get"": {
      ""description"": ""Returns all pets from the system that the user has access to"",
      ""responses"": {
        ""200"": {
          ""description"": ""A list of pets."",
          ""content"": {
            ""application/json"": {
              ""schema"": {
                ""type"": ""array"",
                ""items"": {
                  ""$ref"": ""#/components/schemas/pet""
                }
              }
            }
          }
        }
      }
    }
  }
}".Replace();

            // Act & Assert
            Assert.Equal(expect, Paths.WriteToJson());
        }

        [Fact]
        public void CanWritePathsObjectToYaml()
        {
            // Arrange
            string expect = @"
/pets:
  get:
    description: Returns all pets from the system that the user has access to
    responses:
      200:
        description: A list of pets.
        content:
          application/json:
            schema:
              type: array
              items:
                $ref: '#/components/schemas/pet'
".Replace();

            // Act & Assert
            Assert.Equal(expect, Paths.WriteToYaml());
        }
    }
}
