// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Net.Http;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Interfaces;
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
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityPathItemHandler"/> class.
        /// </summary>
        /// <param name="document">The document to use for references lookup.</param>
        public EntityPathItemHandler(OpenApiDocument document) : base(document)
        {
            
        }
        /// <inheritdoc/>
        protected override ODataPathKind HandleKind => ODataPathKind.Entity;

        /// <inheritdoc/>
        protected override void SetOperations(OpenApiPathItem item)
        {
            var readRestrictions = string.IsNullOrEmpty(TargetPath) ? null : Context?.Model.GetRecord<ReadRestrictionsType>(TargetPath, CapabilitiesConstants.ReadRestrictions);
            
            if (Context is not null &&  EntitySet is not null)
            {
                var entityReadRestrictions = Context.Model.GetRecord<ReadRestrictionsType>(EntitySet, CapabilitiesConstants.ReadRestrictions);
                readRestrictions?.MergePropertiesIfNull(entityReadRestrictions);
                readRestrictions ??= entityReadRestrictions;
            }
            if (readRestrictions == null ||
               (readRestrictions.ReadByKeyRestrictions == null && readRestrictions.IsReadable) ||
               (readRestrictions.ReadByKeyRestrictions != null && readRestrictions.ReadByKeyRestrictions.IsReadable))
            {
                // If we don't have Read by key read restriction, we should check the set read restrction.
                AddOperation(item, HttpMethod.Get);
            }

            var updateRestrictions = string.IsNullOrEmpty(TargetPath) ? null : Context?.Model.GetRecord<UpdateRestrictionsType>(TargetPath, CapabilitiesConstants.UpdateRestrictions);
            
            if (Context is not null &&  EntitySet is not null)
            {
                var entityUpdateRestrictions = Context.Model.GetRecord<UpdateRestrictionsType>(EntitySet, CapabilitiesConstants.UpdateRestrictions);
                updateRestrictions?.MergePropertiesIfNull(entityUpdateRestrictions);
                updateRestrictions ??= entityUpdateRestrictions;
            }
            if (updateRestrictions?.IsUpdatable ?? true)
            {
                if (updateRestrictions?.IsUpdateMethodPutAndPatch == true)
                {
                    AddOperation(item, HttpMethod.Put);
                    AddOperation(item, HttpMethod.Patch);
                }
                else if (updateRestrictions?.IsUpdateMethodPut == true)
                {
                    AddOperation(item, HttpMethod.Put);
                }
                else
                {
                    AddOperation(item, HttpMethod.Patch);
                }
            }

            var deleteRestrictions = string.IsNullOrEmpty(TargetPath) ? null : Context?.Model.GetRecord<DeleteRestrictionsType>(TargetPath, CapabilitiesConstants.DeleteRestrictions);
            
            if (Context is not null &&  EntitySet is not null)
            {
                var entityDeleteRestrictions = Context.Model.GetRecord<DeleteRestrictionsType>(EntitySet, CapabilitiesConstants.DeleteRestrictions);
                deleteRestrictions?.MergePropertiesIfNull(entityDeleteRestrictions);
                deleteRestrictions ??= entityDeleteRestrictions;
            }
            if (deleteRestrictions?.IsDeletable ?? true)
            {
                AddOperation(item, HttpMethod.Delete);
            }
        }

        /// <inheritdoc/>
        protected override void SetExtensions(OpenApiPathItem item)
        {
            base.SetExtensions(item);
            if (EntitySet is null || Context is null) return;

            item.Extensions ??= new Dictionary<string, IOpenApiExtension>();
            item.Extensions.AddCustomAttributesToExtensions(Context, EntitySet.EntityType);
        }
    }
}
