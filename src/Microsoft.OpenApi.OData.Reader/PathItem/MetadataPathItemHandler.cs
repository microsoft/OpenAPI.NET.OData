// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.PathItem
{
    /// <summary>
    /// Path item handler for $metadata.
    /// </summary>
    internal class MetadataPathItemHandler : PathItemHandler
    {
        /// <inheritdoc/>
        protected override ODataPathKind HandleKind => ODataPathKind.Metadata;

        /// <inheritdoc/>
        protected override void SetOperations(OpenApiPathItem item)
        {
            AddOperation(item, OperationType.Get);
        }
    }
}
