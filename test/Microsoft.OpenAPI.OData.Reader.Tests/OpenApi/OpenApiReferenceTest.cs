// <copyright file="OpenApiReferenceTest.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using Xunit;

namespace Microsoft.OData.OpenAPI.Tests
{
    public class OpenApiReferenceTest
    {
        [Fact]
        public void CtorThrowsArgumentNullRef()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentException>("ref", () => new OpenApiReference(null));
        }

        [Fact]
        public void CtorSetsRefPropertyValue()
        {
            // Arrange & Act
            OpenApiReference oar = new OpenApiReference("#/components/schemas/Pet");

            // Assert
            Assert.Equal("#/components/schemas/Pet", oar.Ref);
        }

        [Fact]
        public void CanWriteReferenceObjectIntoJson()
        {
            // Arrange & Act
            string expect = @"
{
  ""$ref"": ""#/components/schemas/Pet""
}".Replace();

            OpenApiReference oar = new OpenApiReference("#/components/schemas/Pet");

            // Act & Assert
            Assert.Equal(expect, oar.WriteToJson());
        }

        [Fact]
        public void CanWriteReferenceObjectIntoYaml()
        {
            // Arrange & Act
            OpenApiReference oar = new OpenApiReference("#/components/schemas/Pet");

            // Act & Assert
            Assert.Equal("$ref: #/components/schemas/Pet", oar.WriteToYaml());
        }
    }
}
