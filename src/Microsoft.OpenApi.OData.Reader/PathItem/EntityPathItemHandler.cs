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
        private IEdmEntityType _entityType;

        protected override void Initialize(ODataContext context, ODataPath path)
        {
            base.Initialize(context, path);
            _entityType = EntitySet.EntityType();            
        }

        /// <inheritdoc/>
        protected override void SetOperations(OpenApiPathItem item)
        {
            ReadRestrictionsType read = Context.Model.GetRecord<ReadRestrictionsType>(EntitySet);
            ReadRestrictionsType readEntity = Context.Model.GetRecord<ReadRestrictionsType>(_entityType);

            bool isReadable = (read == null) || readEntity == null;

            if (isReadable ||
               (read.ReadByKeyRestrictions == null && read.IsReadable) ||
               (read.ReadByKeyRestrictions != null && read.ReadByKeyRestrictions.IsReadable) ||
               readEntity.IsReadable)
            {
                // If we don't have Read by key read restriction, we should check the set read restrction.
                AddOperation(item, OperationType.Get);
            }

            UpdateRestrictionsType update = Context.Model.GetRecord<UpdateRestrictionsType>(EntitySet);
            UpdateRestrictionsType updateEntity = Context.Model.GetRecord<UpdateRestrictionsType>(_entityType);
            bool isUpdatable = (update != null) ? update.IsUpdatable : (updateEntity == null || updateEntity.IsUpdatable);
            
            if (isUpdatable)
            {
                if ((update?.IsUpdateMethodPut ?? false) ||
                    (updateEntity?.IsUpdateMethodPut ?? false))
                {
                    AddOperation(item, OperationType.Put);
                }
                else
                {
                    AddOperation(item, OperationType.Patch);
                }
            }

            DeleteRestrictionsType delete = Context.Model.GetRecord<DeleteRestrictionsType>(EntitySet);
            DeleteRestrictionsType deleteEntity = Context.Model.GetRecord<DeleteRestrictionsType>(_entityType);
            bool isDeletable = (delete != null) ? delete.IsDeletable : (deleteEntity == null || deleteEntity.IsDeletable);

            if (isDeletable)
            {
                AddOperation(item, OperationType.Delete);
            }
        }

        /// <inheritdoc/>
        protected override void SetExtensions(OpenApiPathItem pathItem)
        {
            base.SetExtensions(pathItem);
            pathItem.Extensions.AddCustomAtributesToExtensions(Context, EntitySet.EntityType());
        }
    }
}
