// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Net.Http;
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
        /// <summary>
        /// Initializes a new instance of the <see cref="MediaEntityPathItemHandler"/> class.
        /// </summary>
        /// <param name="document">The document to use for references lookup.</param>
        public MediaEntityPathItemHandler(OpenApiDocument document) : base(document)
        {
            
        }
        /// <inheritdoc/>
        protected override ODataPathKind HandleKind => ODataPathKind.MediaEntity;

        /// <summary>
        /// Gets the entity set.
        /// </summary>
        protected IEdmEntitySet? EntitySet { get; private set; }

        /// <summary>
        /// Gets the singleton.
        /// </summary>
        protected IEdmSingleton? Singleton { get; private set; }

        /// <inheritdoc/>
        protected override void SetOperations(OpenApiPathItem item)
        {
            var readRestrictions = Context.Model.GetRecord<ReadRestrictionsType>(TargetPath, CapabilitiesConstants.ReadRestrictions);
            var navSourceReadRestrictions = EntitySet != null
                ? Context.Model.GetRecord<ReadRestrictionsType>(EntitySet)
                : (Singleton is null ? null : Context.Model.GetRecord<ReadRestrictionsType>(Singleton));
            readRestrictions ??= navSourceReadRestrictions;
            if (readRestrictions == null ||
               (readRestrictions.ReadByKeyRestrictions == null && readRestrictions.IsReadable) ||
               (readRestrictions.ReadByKeyRestrictions != null && readRestrictions.ReadByKeyRestrictions.IsReadable))
            {
                AddOperation(item, HttpMethod.Get);
            }

            var updateRestrictions = Context.Model.GetRecord<UpdateRestrictionsType>(TargetPath, CapabilitiesConstants.UpdateRestrictions);
            var navSourceUpdateRestrictions = EntitySet != null
                ? Context.Model.GetRecord<UpdateRestrictionsType>(EntitySet)
                : (Singleton is null ? null : Context.Model.GetRecord<UpdateRestrictionsType>(Singleton));
            updateRestrictions ??= navSourceUpdateRestrictions;
            if (updateRestrictions?.IsUpdatable ?? true)
            {
                AddOperation(item, HttpMethod.Put);
            }

            var deleteRestrictions = Context.Model.GetRecord<DeleteRestrictionsType>(TargetPath, CapabilitiesConstants.DeleteRestrictions);
            var navSourceDeleteRestrictions = EntitySet != null
                ? Context.Model.GetRecord<DeleteRestrictionsType>(EntitySet)
                : (Singleton is null ? null : Context.Model.GetRecord<DeleteRestrictionsType>(Singleton));
            deleteRestrictions ??= navSourceDeleteRestrictions;
            if (deleteRestrictions?.IsDeletable ?? true)
            {
                AddOperation(item, HttpMethod.Delete);
            }
        }

        /// <inheritdoc/>
        protected override void Initialize(ODataContext context, ODataPath path)
        {
            base.Initialize(context, path);

            // The first segment could be an entity set segment or a singleton segment.
            if (path.FirstSegment is ODataNavigationSourceSegment {NavigationSource: IEdmEntitySet entitySet})
                EntitySet = entitySet;
            if (path.FirstSegment is ODataNavigationSourceSegment {NavigationSource: IEdmSingleton singleton})
                Singleton = singleton;
        }
        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiPathItem pathItem)
        {
            base.SetBasicInfo(pathItem);
            pathItem.Description = $"Provides operations to manage the media for the {(EntitySet?.EntityType ?? Singleton?.EntityType)?.Name} entity.";
        }
    }
}
