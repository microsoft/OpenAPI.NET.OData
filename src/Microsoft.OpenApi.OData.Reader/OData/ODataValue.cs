// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;

namespace Microsoft.OpenApi.OData
{
    /// <summary>
    /// Represents an OData value.
    /// </summary>
    internal abstract class ODataValue
    {
        /// <summary>
        /// Gets or set the type reference of this value.
        /// </summary>
        public IEdmTypeReference TypeReference { get; set; }
    }
}
