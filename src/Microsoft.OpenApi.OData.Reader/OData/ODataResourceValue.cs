// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.OpenApi.OData
{
    /// <summary>
    /// Represents an OData resource value (complex or entity).
    /// </summary>
    internal class ODataResourceValue : ODataValue
    {
        /// <summary>
        /// Gets or sets the properties.
        /// </summary>
        public IDictionary<string, ODataValue> Properties { get; set; }
    }
}
