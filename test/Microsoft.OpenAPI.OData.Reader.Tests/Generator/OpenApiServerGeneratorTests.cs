// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Tests;
using Xunit;

namespace Microsoft.OpenApi.OData.Generator.Tests
{
    public class OpenApiServerGeneratorTest
    {
        [Fact]
        public void CreateServersThrowArgumentNullContext()
        {
            // Arrange
            ODataContext context = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context", () => context.CreateServers());
        }

        [Fact]
        public void CreateServersReturnsModel()
        {
            // Arrange
            ODataContext context = new ODataContext(EdmModelHelper.BasicEdmModel);

            // Act
            var servers = context.CreateServers();

            // Assert
            Assert.NotNull(servers);
            var server = Assert.Single(servers);
            Assert.Equal("http://localhost", server.Url);
        }
    }
}
