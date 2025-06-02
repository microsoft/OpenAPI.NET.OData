// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.OpenApi.OData.Extensions
{
    /// <summary>
    /// The interface for route prefix.
    /// </summary>
    public interface IODataRoutePathPrefixProvider
    {
        /// <summary>
        /// The route prefix.
        /// </summary>
        public string? PathPrefix { get; }

        /// <summary>
        /// The route prefix parameters.
        /// </summary>
        public IEnumerable<OpenApiParameter>? Parameters { get; }
    }
}
