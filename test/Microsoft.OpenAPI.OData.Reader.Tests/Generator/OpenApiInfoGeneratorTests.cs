// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using Microsoft.OpenApi.OData.Tests;
using Xunit;

namespace Microsoft.OpenApi.OData.Generator.Tests
{
    public class OpenApiInfoGeneratorTest
    {
        [Fact]
        public void CreateInfoThrowArgumentNullContext()
        {
            // Arrange
            ODataContext context = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context", () => context.CreateInfo());
        }

        [Fact]
        public void CreateInfoReturnsNullForEmptyModel()
        {
            // Arrange
            ODataContext context = new ODataContext(EdmModelHelper.EmptyModel);

            // Act
            var info = context.CreateInfo();

            // Assert
            Assert.NotNull(info);
        }
    }
}
