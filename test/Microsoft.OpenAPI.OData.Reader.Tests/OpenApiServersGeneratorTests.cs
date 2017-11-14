// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.OpenApi.OData.Tests
{
    public class OpenApiServersGeneratorTest
    {
        [Fact]
        public void CreateServersThrowArgumentNull()
        {
            // Arrange
            IEdmModel model = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("model", () => model.CreateServers());
        }

        [Fact]
        public void CreateServersReturnsModel()
        {
            // Arrange & Act
            var servers = EdmModelHelper.BasicEdmModel.CreateServers();

            // Assert
            Assert.NotNull(servers);
            var server = Assert.Single(servers);
            Assert.Equal("http://localhost", server.Url);
        }
    }
}
