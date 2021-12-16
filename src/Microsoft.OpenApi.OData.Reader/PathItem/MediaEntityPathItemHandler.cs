// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;

namespace Microsoft.OpenApi.OData.PathItem
{
    /// <summary>
    /// Create a <see cref="OpenApiPathItem"/> for a media entity.
    /// </summary>
    internal class MediaEntityPathItemHandler : PathItemHandler
    {
        /// <inheritdoc/>
        protected override ODataPathKind HandleKind => ODataPathKind.MediaEntity;

        /// <summary>
        /// Gets the entity set.
        /// </summary>
        protected IEdmEntitySet EntitySet { get; private set; }

        /// <summary>
        /// Gets the singleton.
        /// </summary>
        protected IEdmSingleton Singleton { get; private set; }

        /// <inheritdoc/>
        protected override void SetOperations(OpenApiPathItem item)
        {
            ReadRestrictionsType read = EntitySet != null
                ? Context.Model.GetRecord<ReadRestrictionsType>(EntitySet)
                : Context.Model.GetRecord<ReadRestrictionsType>(Singleton);

            if (read == null ||
               (read.ReadByKeyRestrictions == null && read.IsReadable) ||
               (read.ReadByKeyRestrictions != null && read.ReadByKeyRestrictions.IsReadable))
            {
                AddOperation(item, OperationType.Get);
            }

            UpdateRestrictionsType update = EntitySet != null
                ? Context.Model.GetRecord<UpdateRestrictionsType>(EntitySet)
                : Context.Model.GetRecord<UpdateRestrictionsType>(Singleton);

            if (update == null || update.IsUpdatable)
            {
                AddOperation(item, OperationType.Put);
            }
        }

        /// <inheritdoc/>
        protected override void Initialize(ODataContext context, ODataPath path)
        {
            base.Initialize(context, path);

            // The first segment could be an entity set segment or a singleton segment.
            ODataNavigationSourceSegment navigationSourceSegment = path.FirstSegment as ODataNavigationSourceSegment;

            EntitySet = navigationSourceSegment.NavigationSource as IEdmEntitySet;
            if (EntitySet == null)
            {
                Singleton = navigationSourceSegment.NavigationSource as IEdmSingleton;
            }
        }
        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiPathItem pathItem)
        {
            base.SetBasicInfo(pathItem);
            pathItem.Description = $"Provides operations to manage the media for the {(EntitySet?.EntityType() ?? Singleton?.EntityType()).Name} entity.";
        }
    }
}
