//---------------------------------------------------------------------
// <copyright file="OpenApiTestBase.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using Xunit.Abstractions;

namespace Microsoft.OData.OpenAPI.Tests
{
    public class OpenApiTestBase
    {
        private readonly ITestOutputHelper output;

        public OpenApiTestBase(ITestOutputHelper output)
        {
            this.output = output;
        }
    }
}
