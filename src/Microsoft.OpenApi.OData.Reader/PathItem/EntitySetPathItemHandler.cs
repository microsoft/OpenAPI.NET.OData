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
    /// Create a <see cref="OpenApiPathItem"/> for <see cref="IEdmEntitySet"/>.
    /// </summary>
    internal class EntitySetPathItemHandler : PathItemHandler
    {
        /// <inheritdoc/>
        protected override ODataPathKind HandleKind => ODataPathKind.EntitySet;

        /// <summary>
        /// Gets the entity set.
        /// </summary>
        protected IEdmEntitySet EntitySet { get; private set; }

        /// <inheritdoc/>
        protected override void SetOperations(OpenApiPathItem item)
        {
            ReadRestrictionsType readRestrictions = Context.Model.GetRecord<ReadRestrictionsType>(TargetPath, CapabilitiesConstants.ReadRestrictions);
            ReadRestrictionsType entityReadRestrictions = Context.Model.GetRecord<ReadRestrictionsType>(EntitySet, CapabilitiesConstants.ReadRestrictions);
            readRestrictions?.MergePropertiesIfNull(entityReadRestrictions);
            readRestrictions ??= entityReadRestrictions;
            if (readRestrictions?.IsReadable ?? true)
            {
                AddOperation(item, OperationType.Get);
            }

            InsertRestrictionsType insertRestrictions = Context.Model.GetRecord<InsertRestrictionsType>(TargetPath, CapabilitiesConstants.InsertRestrictions);
            InsertRestrictionsType entityInsertRestrictions = Context.Model.GetRecord<InsertRestrictionsType>(EntitySet, CapabilitiesConstants.InsertRestrictions);
            insertRestrictions?.MergePropertiesIfNull(entityInsertRestrictions);
            insertRestrictions ??= entityInsertRestrictions;
            if (insertRestrictions?.IsInsertable ?? true)
            {
                AddOperation(item, OperationType.Post);
            }
        }

        /// <inheritdoc/>
        protected override void Initialize(ODataContext context, ODataPath path)
        {
            base.Initialize(context, path);

            // The first segment should be the entity set segment.
            ODataNavigationSourceSegment navigationSourceSegment = path.FirstSegment as ODataNavigationSourceSegment;
            EntitySet = navigationSourceSegment.NavigationSource as IEdmEntitySet;
        }
        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiPathItem pathItem)
        {
            base.SetBasicInfo(pathItem);
            pathItem.Description = $"Provides operations to manage the collection of {EntitySet.EntityType.Name} entities.";
        }

        /// <inheritdoc/>
        protected override void SetExtensions(OpenApiPathItem pathItem)
        {
            base.SetExtensions(pathItem);
            pathItem.Extensions.AddCustomAttributesToExtensions(Context, EntitySet);            
        }
    }
}
