// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OpenApi.Models;

namespace Microsoft.OpenApi.OData.Operation
{
    /// <summary>
    /// Update an Entity
    /// The Path Item Object for the entity set contains the keyword patch with an Operation Object as value
    /// that describes the capabilities for updating the entity.
    /// </summary>
    internal class EntityPatchOperationHandler : EntityUpdateOperationHandler
    {
        /// <inheritdoc/>
        public override OperationType OperationType => OperationType.Patch;
    }
}
