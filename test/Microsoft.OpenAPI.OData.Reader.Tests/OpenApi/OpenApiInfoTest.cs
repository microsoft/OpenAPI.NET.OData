//---------------------------------------------------------------------
// <copyright file="OpenApiInfoTest.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using Xunit;

namespace Microsoft.OData.OpenAPI.Tests
{
    public class OpenApiInfoTest
    {
        internal static OpenApiInfo BasicInfo = CreateBasicInfo();
        internal static OpenApiInfo AdvanceInfo = CreateAdvanceInfo();

        [Fact]
        public void WriteBasicInfoToJson()
        {
            // Arrange
            string expect = @"
{
  ""title"": ""Sample OData Book Store App"",
  ""description"": ""This is a sample OData server for a book store."",
  ""termsOfService"": ""http://services.odata.org/"",
  ""version"": ""1.0.1""
}".Replace();

            // Act & Assert
            Assert.Equal(expect, BasicInfo.WriteToJson());
        }

        [Fact]
        public void WriteBasicInfoToYaml()
        {
            // Arrange
            string expect = @"
title: Sample OData Book Store App
description: This is a sample OData server for a book store.
termsOfService: http://services.odata.org/
version: 1.0.1
".Replace();

            // Act & Assert
            Assert.Equal(expect, BasicInfo.WriteToYaml());
        }

        [Fact]
        public void WriteAdvanceInfoToJson()
        {
            // Arrange
            string expect = @"
{
  ""title"": ""Sample Pet Store App"",
  ""description"": ""This is a sample server for a pet store."",
  ""termsOfService"": ""http://example.com/terms/"",
  ""contact"": {
    ""name"": ""API Support"",
    ""url"": ""http://www.example.com/support"",
    ""email"": ""support@example.com""
  },
  ""license"": {
    ""name"": ""Apache 2.0"",
    ""url"": ""http://www.apache.org/licenses/LICENSE-2.0.html""
  },
  ""version"": ""1.0.1""
}".Replace();

            // Act & Assert
            Assert.Equal(expect, AdvanceInfo.WriteToJson());
        }

        [Fact]
        public void WriteAdvanceInfoToYaml()
        {
            // Arrange
            string expect = @"
title: Sample Pet Store App
description: This is a sample server for a pet store.
termsOfService: http://example.com/terms/
contact:
  name: API Support
  url: http://www.example.com/support
  email: support@example.com
license:
  name: Apache 2.0
  url: http://www.apache.org/licenses/LICENSE-2.0.html
version: 1.0.1
".Replace();
            // Act & Assert
            Assert.Equal(expect, AdvanceInfo.WriteToYaml());
        }

        private static OpenApiInfo CreateBasicInfo()
        {
            return new OpenApiInfo
            {
                Title = "Sample OData Book Store App",
                Version = new Version(1, 0, 1),
                Description = "This is a sample OData server for a book store.",
                TermsOfService = new Uri("http://services.odata.org/")
            };
        }

        private static OpenApiInfo CreateAdvanceInfo()
        {
            return new OpenApiInfo
            {
                Title = "Sample Pet Store App",
                Version = new Version(1, 0, 1),
                Description = "This is a sample server for a pet store.",
                TermsOfService = new Uri("http://example.com/terms/"),
                Contact = OpenApiContactTest.AdvanceContact,
                License = OpenApiLicenseTest.AdvanceLicense
            };
        }
    }
}
