// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.PathItem
{
    /// <summary>
    /// Interface for <see cref="IPathItemHandler"/>.
    /// </summary>
    internal interface IPathItemHandlerProvider
    {
        /// <summary>
        /// Get the <see cref="IPathItemHandler"/> based on the path type.
        /// </summary>
        /// <param name="pathType">The path type.</param>
        /// <returns>The <see cref="IPathItemHandler"/>.</returns>
        IPathItemHandler GetHandler(ODataPathType pathType);
    }
}
