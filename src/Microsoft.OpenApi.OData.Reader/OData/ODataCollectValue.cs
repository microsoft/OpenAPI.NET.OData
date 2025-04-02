// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.OpenApi.OData
{
    /// <summary>
    /// Represents an OData value.
    /// </summary>
    internal class ODataCollectValue : ODataValue
    {
        public IList<ODataValue>? Elements { get; set; }
    }
}
