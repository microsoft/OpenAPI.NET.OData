// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;

namespace Microsoft.OpenApi.OData.PathItem
{
    /// <summary>
    /// Path item handler for single Entity.
    /// </summary>
    internal class EntityPathItemHandler : EntitySetPathItemHandler
    {
        /// <inheritdoc/>
        protected override ODataPathKind HandleKind => ODataPathKind.Entity;

        /// <inheritdoc/>
        protected override void SetOperations(OpenApiPathItem item)
        {
            ReadRestrictionsType readRestrictions = Context.Model.GetRecord<ReadRestrictionsType>(TargetPath, CapabilitiesConstants.ReadRestrictions);
            ReadRestrictionsType entityReadRestrictions = Context.Model.GetRecord<ReadRestrictionsType>(EntitySet, CapabilitiesConstants.ReadRestrictions);
            readRestrictions?.MergePropertiesIfNull(entityReadRestrictions);
            readRestrictions ??= entityReadRestrictions;
            if (readRestrictions == null ||
               (readRestrictions.ReadByKeyRestrictions == null && readRestrictions.IsReadable) ||
               (readRestrictions.ReadByKeyRestrictions != null && readRestrictions.ReadByKeyRestrictions.IsReadable))
            {
                // If we don't have Read by key read restriction, we should check the set read restrction.
                AddOperation(item, OperationType.Get);
            }

            UpdateRestrictionsType updateRestrictions = Context.Model.GetRecord<UpdateRestrictionsType>(TargetPath, CapabilitiesConstants.UpdateRestrictions);
            UpdateRestrictionsType entityUpdateRestrictions = Context.Model.GetRecord<UpdateRestrictionsType>(EntitySet, CapabilitiesConstants.UpdateRestrictions);
            updateRestrictions?.MergePropertiesIfNull(entityUpdateRestrictions);
            updateRestrictions ??= entityUpdateRestrictions;
            if (updateRestrictions?.IsUpdatable ?? true)
            {
                if (updateRestrictions?.IsUpdateMethodPutAndPatch == true)
                {
                    AddOperation(item, OperationType.Put);
                    AddOperation(item, OperationType.Patch);
                }
                else if (updateRestrictions?.IsUpdateMethodPut == true)
                {
                    AddOperation(item, OperationType.Put);
                }
                else
                {
                    AddOperation(item, OperationType.Patch);
                }
            }

            DeleteRestrictionsType deleteRestrictions = Context.Model.GetRecord<DeleteRestrictionsType>(TargetPath, CapabilitiesConstants.DeleteRestrictions);
            DeleteRestrictionsType entityDeleteRestrictions = Context.Model.GetRecord<DeleteRestrictionsType>(EntitySet, CapabilitiesConstants.DeleteRestrictions);
            deleteRestrictions?.MergePropertiesIfNull(entityDeleteRestrictions);
            deleteRestrictions ??= entityDeleteRestrictions;
            if (deleteRestrictions?.IsDeletable ?? true)
            {
                AddOperation(item, OperationType.Delete);
            }
        }

        /// <inheritdoc/>
        protected override void SetExtensions(OpenApiPathItem pathItem)
        {
            base.SetExtensions(pathItem);
            pathItem.Extensions.AddCustomAttributesToExtensions(Context, EntitySet.EntityType);
        }
    }
}
