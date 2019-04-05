// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.OpenApi.OData.Capabilities
{
    /// <summary>
    /// Complex type: Org.OData.Capabilities.V1.ExternalExampleValue.
    /// </summary>
    internal class ExternalExampleValue : ExampleValue
    {
        /// <summary>
        /// Gets the Url reference to the value in its literal format
        /// </summary>
        public string ExternalValue { get; set; }
    }
}
