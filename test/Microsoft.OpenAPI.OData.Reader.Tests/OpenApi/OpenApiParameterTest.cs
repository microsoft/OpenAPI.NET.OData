// <copyright file="OpenApiParameterTest.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using Xunit;

namespace Microsoft.OData.OpenAPI.Tests
{
    public class OpenApiParameterTest
    {
        [Fact]
        public void CtorSetsPropertiesValue()
        {
            // Arrange & Act
            OpenApiParameter parameter = new OpenApiParameter
            {
                Name = "token",
                In = ParameterLocation.header
            };

            // Assert
            Assert.Equal("token", parameter.Name);
            Assert.Equal(ParameterLocation.header, parameter.In);
            Assert.Equal(ParameterStyle.simple, parameter.Style);
        }

        [Fact]
        public void CanWriteBasicParameterObjectAsJson()
        {
            // Arrange
            string expect = @"
{
  ""summary"": ""A bar example"",
  ""externalValue"": ""http://example.org/examples/address-example.xml""
}
".Replace();

            OpenApiExample example = new OpenApiExample
            {
                Summary = "A bar example",
                ExternalValue = new Uri("http://example.org/examples/address-example.xml")
            };

            // Act & Assert
            Assert.Equal(expect, example.WriteToJson());
        }

        [Fact]
        public void CanWriteBasicParameterObjectAsYaml()
        {
            // Arrange
            string expect = @"
summary: A bar example
description: A bar example description
value: 
".Replace();

            OpenApiExample example = new OpenApiExample
            {
                Summary = "A bar example",
                Description = "A bar example description",
                Value = new OpenApiAny()
            };

            // Act & Assert
            Assert.Equal(expect, example.WriteToYaml());
        }

        [Fact]
        public void CanWriteParameterObjectAsReferenceObjectJson()
        {
            // Arrange
            string expect = @"
{
  ""$ref"": ""#/components/schemas/Address""
}
".Replace();

            OpenApiExample example = new OpenApiExample
            {
                Reference = new OpenApiReference("#/components/schemas/Address")
            };

            // Act & Assert
            Assert.Equal(expect, example.WriteToJson());
        }

        [Fact]
        public void CanWriteParameterObjectAsReferenceObjectYaml()
        {
            // Arrange
            string expect = @"$ref: #/components/schemas/Address";

            OpenApiExample example = new OpenApiExample
            {
                Reference = new OpenApiReference("#/components/schemas/Address")
            };

            // Act & Assert
            Assert.Equal(expect, example.WriteToYaml());
        }
    }
}
