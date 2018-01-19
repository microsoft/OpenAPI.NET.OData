// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Capabilities;
using Microsoft.OpenApi.OData.Tests;
using Xunit;

namespace Microsoft.OpenApi.OData.Reader.Capabilities.Tests
{
    public class ExpandRestrictionsTests
    {
        [Fact]
        public void Test()
        {
            IEdmModel model = EdmModelHelper.GraphBetaModel;

            IEdmEntityType user = model.SchemaElements.OfType<IEdmEntityType>().First(e => e.Name == "user");
            IEdmNavigationProperty property = user.DeclaredNavigationProperties().First(c => c.Name == "messages");

            var a = model.GetCapabilitiesAnnotation(property, CapabilitiesConstants.ExpandRestrictions);

            //model.
        }
    }
}
