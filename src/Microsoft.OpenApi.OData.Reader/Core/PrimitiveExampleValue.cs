// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.OpenApi.OData.Capabilities
{
    /// <summary>
    /// Complex type: Org.OData.Capabilities.V1.PrimitiveExampleValue.
    /// </summary>
    internal class PrimitiveExampleValue : ExampleValue
    {
        /// <summary>
        /// Gets the Example value for the custom parameter
        /// </summary>
        public object Value { get; private set; }
    }
}
