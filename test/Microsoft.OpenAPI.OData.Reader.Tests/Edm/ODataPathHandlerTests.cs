// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Properties;
using Microsoft.OpenApi.OData.Tests;
using Xunit;

namespace Microsoft.OpenApi.OData.Edm.Tests
{
    public class ODataPathHandlerTests
    {
        [Fact]
        public void ODataPathConstructorThrowsArgumentNull1()
        {
            IEdmModel model = EdmModelHelper.GraphBetaModel;
            ODataContext context = new ODataContext(model);

            var handler = new ODataPathHandler(context);

            var paths = handler.Paths;
            Assert.NotNull(paths);
        }
    }

    public class ODataPathProviderTests
    {
        [Fact]
        public void ODataPathConstructorThrowsArgumentNull123()
        {
            IEdmModel model = EdmModelHelper.GraphBetaModel;
            //ODataContext context = new ODataContext(model);

            ODataPathProvider provider = new ODataPathProvider(model);

            var paths = provider.CreatePaths();

            Assert.NotNull(paths);
        }
    }
}
