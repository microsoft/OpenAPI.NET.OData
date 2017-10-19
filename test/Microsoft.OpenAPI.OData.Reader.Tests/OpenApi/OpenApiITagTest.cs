//---------------------------------------------------------------------
// <copyright file="OpenApiTagTest.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using Xunit;

namespace Microsoft.OData.OpenAPI.Tests
{
    public class OpenApiTagTest
    {
        internal static OpenApiTag BasicTag = new OpenApiTag();
        internal static OpenApiTag AdvanceTag = new OpenApiTag()
        {
            Name = "pet",
            Description = "Pets operations",
            ExternalDocs = OpenApiExternalDocsTest.AdvanceExDocs
        };

        [Fact]
        public void WriteBasicTagToJsonWorks()
        {
            // Arrange
            string expect = @"
{
  ""name"": ""Default Name""
}".Replace();

            // Act & Assert
            Assert.Equal(expect, BasicTag.WriteToJson());
        }

        [Fact]
        public void WriteBasicTagToYamlWorks()
        {
            // Arrange & Act & Assert
            Assert.Equal("name: Default Name", BasicTag.WriteToYaml());
        }

        [Fact]
        public void WriteAdvanceTagToJsonWorks()
        {
            // Arrange
            string expect = @"
{
  ""name"": ""pet"",
  ""description"": ""Pets operations"",
  ""externalDocs"": {
    ""description"": ""Find more info here"",
    ""url"": ""https://example.com""
  }
}"
.Replace();

            // Act & Assert
            Assert.Equal(expect, AdvanceTag.WriteToJson());
        }

        [Fact]
        public void WriteAdvanceTagToYamlWorks()
        {
            // Arrange
            string expect = @"
name: pet
description: Pets operations
externalDocs:
  description: Find more info here
  url: https://example.com
"
.Replace();

            // Act & Assert
            Assert.Equal(expect, AdvanceTag.WriteToYaml());
        }
    }
}
