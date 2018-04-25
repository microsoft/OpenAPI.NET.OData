// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OpenApi.Models;

namespace Microsoft.OpenApi.OData.PathItem
{
    /// <summary>
    /// Create a <see cref="OpenApiPathItem"/> for <see cref="IEdmSingleton"/>.
    /// </summary>
    internal class SingletonPathItemHandler : PathItemHandler
    {
        /// <inheritdoc/>
        protected override void SetOperations(OpenApiPathItem item)
        {
            // Retrieve a singleton.
            AddOperation(item, OperationType.Get);

            // Update a singleton
            AddOperation(item, OperationType.Patch);
        }
    }
}
