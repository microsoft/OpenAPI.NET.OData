//---------------------------------------------------------------------
// <copyright file="OpenApiLicenseTest.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using Xunit;

namespace Microsoft.OData.OpenAPI.Tests
{
    public class OpenApiLicenseTest
    {
        internal static OpenApiLicense BasicLicense = new OpenApiLicense();
        internal static OpenApiLicense AdvanceLicense = new OpenApiLicense
        {
            Name = "Apache 2.0",
            Url = new Uri("http://www.apache.org/licenses/LICENSE-2.0.html")
        };

        [Fact]
        public void WriteBasicLicenseToJsonWorks()
        {
            // Arrange
            string expect = @"
{
  ""name"": ""Default Name""
}".Replace();

            // Act & Assert
            Assert.Equal(expect, BasicLicense.WriteToJson());
        }

        [Fact]
        public void WriteBasicLicenseToYamlWorks()
        {
            // Arrange & Act & Assert
            Assert.Equal("name: Default Name", BasicLicense.WriteToYaml());
        }

        [Fact]
        public void WriteFullLicenseToJsonWorks()
        {
            // Arrange
            string expect = @"
{
  ""name"": ""Apache 2.0"",
  ""url"": ""http://www.apache.org/licenses/LICENSE-2.0.html""
}"
.Replace();

            // Act & Assert
            Assert.Equal(expect, AdvanceLicense.WriteToJson());
        }

        [Fact]
        public void WriteFullLicenseToYamlWorks()
        {
            // Arrange
            string expect = @"
name: Apache 2.0
url: http://www.apache.org/licenses/LICENSE-2.0.html
"
.Replace();

            // Act & Assert
            Assert.Equal(expect, AdvanceLicense.WriteToYaml());
        }
    }
}
