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
    public class OpenApiParameterGeneratorTest
    {
        private OpenApiConvertSettings _settings = new OpenApiConvertSettings();

        [Fact]
        public void CreateParametersThrowArgumentNullModel()
        {
            // Arrange
            IEdmModel model = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("model", () => model.CreateParameters(_settings));
        }

        [Fact]
        public void CreateParametersThrowArgumentNullSettings()
        {
            // Arrange
            IEdmModel model = EdmCoreModel.Instance;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("settings", () => model.CreateParameters(settings: null));
        }

        [Fact]
        public void CreateParametersReturnsCreatedParameters()
        {
            // Arrange
            IEdmModel model = EdmCoreModel.Instance;

            // Act
            var parameters = model.CreateParameters(_settings);

            // Assert
            Assert.NotNull(parameters);
            Assert.NotEmpty(parameters);
            Assert.Equal(5, parameters.Count);
            Assert.Equal(new[] { "top", "skip", "count", "filter", "search" },
                parameters.Select(p => p.Key));
        }

        [Fact]
        public void CanSeralizeAsYamlFromTheCreatedParameters()
        {
            // Arrange
            IEdmModel model = EdmCoreModel.Instance;

            // Act
            var parameters = model.CreateParameters(_settings);

            // Assert
            Assert.Contains("skip", parameters.Select(p => p.Key));
            var skip = parameters.First(c => c.Key == "skip").Value;

            string yaml = skip.SerializeAsYaml(OpenApiSpecVersion.OpenApi3_0);
            Assert.Equal(
@"name: $skip
in: query
description: Skip the first n items
schema:
  minimum: 0
  type: integer
".Replace(), yaml);
        }
    }
}
