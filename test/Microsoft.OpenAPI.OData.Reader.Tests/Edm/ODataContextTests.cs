// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.OpenApi.OData.Tests;
using Xunit;

namespace Microsoft.OpenApi.OData.Edm.Tests
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

        [Fact]
        public void Test()
        {
            OpenApiConvertSettings settings = new OpenApiConvertSettings();
            settings.NavigationPropertyDepth = 7;
            ODataContext context = new ODataContext(EdmModelHelper.GraphBetaModel, settings);

            IList<ODataPath> paths = context.Paths;

            StringBuilder sb = new StringBuilder();
            foreach (var path in paths)
            {
                sb.Append(path.GetPathItemName(settings)).Append("\n");
            }
            File.WriteAllText("c:\\c.xml", sb.ToString());

            Assert.Equal(321, paths.Count);
        }
    }
}
