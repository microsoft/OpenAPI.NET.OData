// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Capabilities;

namespace Microsoft.OpenApi.OData.PathItem
{
    /// <summary>
    /// Path item handler for single Entity.
    /// </summary>
    internal class EntityPathItemHandler : EntitySetPathItemHandler
    {
        /// <inheritdoc/>
        protected override void SetOperations(OpenApiPathItem item)
        {
            IndexableByKey index = new IndexableByKey(Context.Model, EntitySet);
            if (index.IsSupported)
            {
                AddOperation(item, OperationType.Get);
            }

            UpdateRestrictions update = new UpdateRestrictions(Context.Model, EntitySet);
            if (update.IsUpdatable)
            {
                AddOperation(item, OperationType.Patch);
            }

            DeleteRestrictions delete = new DeleteRestrictions(Context.Model, EntitySet);
            if (delete.IsDeletable)
            {
                AddOperation(item, OperationType.Delete);
            }
        }
    }
}
