// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Tests;
using Xunit;

namespace Microsoft.OpenApi.OData.Generator.Tests
{
    public class OpenApiServersGeneratorTest
    {
        private OpenApiConvertSettings _settings = new OpenApiConvertSettings();

        [Fact]
        public void CreateServersThrowArgumentNullModel()
        {
            // Arrange
            IEdmModel model = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("model", () => model.CreateServers(_settings));
        }

        [Fact]
        public void CreateServersThrowArgumentNullSettings()
        {
            // Arrange
            IEdmModel model = EdmCoreModel.Instance;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("settings", () => model.CreateServers(settings: null));
        }

        [Fact]
        public void CreateServersReturnsModel()
        {
            // Arrange & Act
            var servers = EdmModelHelper.BasicEdmModel.CreateServers(_settings);

            // Assert
            Assert.NotNull(servers);
            var server = Assert.Single(servers);
            Assert.Equal("http://localhost", server.Url);
        }
    }
}
