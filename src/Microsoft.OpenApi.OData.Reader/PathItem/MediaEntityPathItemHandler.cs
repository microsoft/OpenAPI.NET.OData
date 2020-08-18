// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;

namespace Microsoft.OpenApi.OData.PathItem
{
    /// <summary>
    /// Create a <see cref="OpenApiPathItem"/> for a media entity.
    /// </summary>
    internal class MediaEntityPathItemHandler : EntitySetPathItemHandler
    {
        /// <inheritdoc/>
        protected override ODataPathKind HandleKind => ODataPathKind.MediaEntity;

        /// <inheritdoc/>
        protected override void SetOperations(OpenApiPathItem item)
        {
            ReadRestrictionsType read = Context.Model.GetRecord<ReadRestrictionsType>(EntitySet);
            if (read == null ||
               (read.ReadByKeyRestrictions == null && read.IsReadable) ||
               (read.ReadByKeyRestrictions != null && read.ReadByKeyRestrictions.IsReadable))
            {
                AddOperation(item, OperationType.Get);
            }

            UpdateRestrictionsType update = Context.Model.GetRecord<UpdateRestrictionsType>(EntitySet);
            if (update == null || update.IsUpdatable)
            {
                AddOperation(item, OperationType.Put);
            }
        }
    }
}
