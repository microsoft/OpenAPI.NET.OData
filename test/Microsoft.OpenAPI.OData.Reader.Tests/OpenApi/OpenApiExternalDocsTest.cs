//---------------------------------------------------------------------
// <copyright file="OpenApiExternalDocsTest.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using Xunit;

namespace Microsoft.OData.OpenAPI.Tests
{
    public class OpenApiExternalDocsTest
    {
        internal static OpenApiExternalDocs BasicExDocs = new OpenApiExternalDocs();
        internal static OpenApiExternalDocs AdvanceExDocs = new OpenApiExternalDocs()
        {
            Url = new Uri("https://example.com"),
            Description = "Find more info here"
        };

        [Fact]
        public void WriteBasicExternalDocsToJson()
        {
            // Arrange
            string expect = @"
{
  ""url"": ""http://localhost/""
}".Replace();

            // Act & Assert
            Assert.Equal(expect, BasicExDocs.WriteToJson());
        }

        [Fact]
        public void WriteBasicExternalDocsToYaml()
        {
            // Arrange & Act & Assert
            Assert.Equal("url: http://localhost/", BasicExDocs.WriteToYaml());
        }

        [Fact]
        public void WriteAdvanceExternalDocsToJson()
        {
            // Arrange
            string expect = @"
{
  ""description"": ""Find more info here"",
  ""url"": ""https://example.com""
}".Replace();

            // Act & Assert
            Assert.Equal(expect, AdvanceExDocs.WriteToJson());
        }

        [Fact]
        public void WriteAdvanceExternalDocsToYaml()
        {
            // Arrange
            string expect = @"
description: Find more info here
url: https://example.com
"
.Replace();

            // Act & Assert
            Assert.Equal(expect, AdvanceExDocs.WriteToYaml());
        }
    }
}
