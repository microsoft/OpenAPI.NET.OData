//---------------------------------------------------------------------
// <copyright file="OpenApiMediaTypeTest.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System.Collections.Generic;
using Xunit;

namespace Microsoft.OData.OpenAPI.Tests
{
    public class OpenApiMediaTypeTest
    {
        private OpenApiMediaType _mediaType = new OpenApiMediaType
        {
            Schema = new OpenApiSchema
            {
                Reference = new OpenApiReference("#/components/schemas/Pet")
            },
            Examples = new Dictionary<string, OpenApiExample>
            {
                {
                    "cat",
                    new OpenApiExample
                    {
                        Summary = "An example of a cat",
                        Value = new OpenApiAny
                        {
                            { "name", "Fluffy"},
                            { "petType", "Cat"},
                            { "color", "White"},
                            { "gender", "Male"},
                            { "breed", "Persian"}
                        }
                    }
                },
                {
                    "dog",
                    new OpenApiExample
                    {
                        Summary = "An example of a dog with a cat's name",
                        Value = new OpenApiAny
                        {
                            { "name", "Puma"},
                            { "petType", "Dog"},
                            { "color", "Black"},
                            { "gender", "Female"},
                            { "breed", "Mixed"}
                        }
                    }
                },
                {
                    "frog",
                    new OpenApiExample
                    {
                        Reference = new OpenApiReference("#/components/examples/frog-example")
                    }
                }
            }
        };

        [Fact]
        public void CanWriteMediaTypeObjectToJson()
        {
            // Arrange
            string expect = @"
{
  ""schema"": {
    ""$ref"": ""#/components/schemas/Pet""
  },
  ""examples"": {
    ""cat"": {
      ""summary"": ""An example of a cat"",
      ""value"": {
        ""name"": ""Fluffy"",
        ""petType"": ""Cat"",
        ""color"": ""White"",
        ""gender"": ""Male"",
        ""breed"": ""Persian""
      }
    },
    ""dog"": {
      ""summary"": ""An example of a dog with a cat's name"",
      ""value"": {
        ""name"": ""Puma"",
        ""petType"": ""Dog"",
        ""color"": ""Black"",
        ""gender"": ""Female"",
        ""breed"": ""Mixed""
      }
    },
    ""frog"": {
      ""$ref"": ""#/components/examples/frog-example""
    }
  }
}".Replace();

            // Act & Assert
            Assert.Equal(expect, _mediaType.WriteToJson());
        }

        [Fact]
        public void CanWriteMediaTypeObjectToYaml()
        {
            // Arrange
            string expect = @"
schema:
  $ref: '#/components/schemas/Pet'
examples:
  cat:
    summary: An example of a cat
    value:
      name: Fluffy
      petType: Cat
      color: White
      gender: Male
      breed: Persian
  dog:
    summary: An example of a dog with a cat's name
    value:
      name: Puma
      petType: Dog
      color: Black
      gender: Female
      breed: Mixed
  frog:
    $ref: '#/components/examples/frog-example'
".Replace();

            // Act & Assert
            Assert.Equal(expect, _mediaType.WriteToYaml());
        }
    }
}
