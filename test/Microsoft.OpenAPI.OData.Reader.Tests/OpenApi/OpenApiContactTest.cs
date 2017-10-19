//---------------------------------------------------------------------
// <copyright file="OpenApiContactTest.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using Xunit;

namespace Microsoft.OData.OpenAPI.Tests
{
    public class OpenApiContactTest
    {
        internal static OpenApiContact BasicContact = new OpenApiContact();
        internal static OpenApiContact AdvanceContact = new OpenApiContact
        {
            Name = "API Support",
            Url = new Uri("http://www.example.com/support"),
            Email = "support@example.com"
        };

        [Fact]
        public void WriteBasicContactToJsonWorks()
        {
            // Arrange & Act & Assert
            Assert.Equal("{ }", BasicContact.WriteToJson());
        }

        [Fact]
        public void WriteBasicContactToYamlWorks()
        {
            // Arrange & Act & Assert
            Assert.Equal("", BasicContact.WriteToYaml());
        }

        [Fact]
        public void WriteAdvanceContactToJsonWorks()
        {
            // Arrange
            string expect = @"
{
  ""name"": ""API Support"",
  ""url"": ""http://www.example.com/support"",
  ""email"": ""support@example.com""
}"
.Replace();

            // Act & Assert
            Assert.Equal(expect, AdvanceContact.WriteToJson());
        }

        [Fact]
        public void WriteFullContactToYamlWorks()
        {
            // Arrange
            string expect = @"
name: API Support
url: http://www.example.com/support
email: support@example.com
"
.Replace();

            // Act & Assert
            Assert.Equal(expect, AdvanceContact.WriteToYaml());
        }
    }
}
