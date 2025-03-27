// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OpenApi.Models;

namespace Microsoft.OpenApi.OData.Extensions
{
    /// <summary>
    /// Default implementation of <see cref="IODataRoutePathPrefixProvider"/>.
    /// </summary>
    public class ODataRoutePathPrefixProvider : IODataRoutePathPrefixProvider
    {
        /// <summary>
        /// Gets/sets the path prefix.
        /// </summary>
        public string? PathPrefix { get; set; }

        /// <summary>
        /// Gets/sets the associated parameters for the path prefix.
        /// </summary>
        public IEnumerable<OpenApiParameter>? Parameters { get; set; }
    }
}
