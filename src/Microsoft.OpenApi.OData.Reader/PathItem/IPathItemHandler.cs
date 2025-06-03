// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.PathItem
{
    /// <summary>
    /// Interface for <see cref="OpenApiPathItem"/>.
    /// </summary>
    internal interface IPathItemHandler
    {
        /// <summary>
        /// Create <see cref="OpenApiPathItem"/> based on <see cref="ODataContext"/> and <see cref="ODataPath"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="path">The path.</param>
        /// <returns>The created <see cref="OpenApiPathItem"/>.</returns>
        OpenApiPathItem CreatePathItem(ODataContext context, ODataPath path);
    }
}
