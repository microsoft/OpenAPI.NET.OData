// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using Microsoft.OpenApi.OData.Tests;
using Xunit;

namespace Microsoft.OpenApi.OData.Generator.Tests
{
    public class ODataContextTest
    {
        [Fact]
        public void CtorThrowArgumentNullModel()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>("model", () => new ODataContext(model: null));
        }

        [Fact]
        public void CtorThrowArgumentNullsetting()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>("settings", () => new ODataContext(EdmModelHelper.EmptyModel, settings: null));
        }
    }
}
