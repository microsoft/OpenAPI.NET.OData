// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.OData.Tests;
using Xunit;

namespace Microsoft.OpenApi.OData.Generator.Tests
{
    public class OpenApiResponseGeneratorTest
    {
        private OpenApiConvertSettings _settings = new OpenApiConvertSettings();

        [Fact]
        public void CreateResponsesThrowArgumentNullModel()
        {
            // Arrange
            IEdmModel model = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("model", () => model.CreateResponses(_settings));
        }

        [Fact]
        public void CreateResponsesThrowArgumentNullSettings()
        {
            // Arrange
            IEdmModel model = EdmCoreModel.Instance;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("settings", () => model.CreateResponses(settings: null));
        }

        [Fact]
        public void CreateResponsesReturnsCreatedResponses()
        {
            // Arrange
            IEdmModel model = EdmCoreModel.Instance;

            // Act
            var responses = model.CreateResponses(_settings);

            // Assert
            Assert.NotNull(responses);
            Assert.NotEmpty(responses);
            var response = Assert.Single(responses);
            Assert.Equal("error", response.Key);
            Assert.NotNull(response.Value.Content);
            Assert.Single(response.Value.Content);
            Assert.Equal("application/json", response.Value.Content.First().Key);
        }

        [Fact]
        public void CanSerializeAsJsonFromTheCreatedResponses()
        {
            // Arrange
            IEdmModel model = EdmCoreModel.Instance;

            // Act
            var responses = model.CreateResponses(_settings);

            // Assert
            var response = Assert.Single(responses).Value;
            string json = response.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);

            Assert.Equal(@"{
  ""description"": ""error"",
  ""content"": {
    ""application/json"": {
      ""schema"": {
        ""$ref"": ""#/components/schemas/odata.error""
      }
    }
  }
}".Replace(), json);
        }
    }
}
