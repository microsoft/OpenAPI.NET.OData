// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Net.Http;
using Microsoft.OData.Edm;
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
        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySetPathItemHandler"/> class.
        /// </summary>
        /// <param name="document">The document to use for references lookup.</param>
        public EntitySetPathItemHandler(OpenApiDocument document) : base(document)
        {
            
        }
        /// <inheritdoc/>
        protected override ODataPathKind HandleKind => ODataPathKind.EntitySet;

        /// <summary>
        /// Gets the entity set.
        /// </summary>
        protected IEdmEntitySet? EntitySet { get; private set; }

        /// <inheritdoc/>
        protected override void SetOperations(OpenApiPathItem item)
        {
            var readRestrictions = string.IsNullOrEmpty(TargetPath) ? null : Context?.Model.GetRecord<ReadRestrictionsType>(TargetPath, CapabilitiesConstants.ReadRestrictions);
            
            if (Context is not null && EntitySet is not null)
            {
                var entityReadRestrictions = Context.Model.GetRecord<ReadRestrictionsType>(EntitySet, CapabilitiesConstants.ReadRestrictions);
                readRestrictions?.MergePropertiesIfNull(entityReadRestrictions);
                readRestrictions ??= entityReadRestrictions;
            }
            if (readRestrictions?.IsReadable ?? true)
            {
                AddOperation(item, HttpMethod.Get);
            }

            var insertRestrictions = string.IsNullOrEmpty(TargetPath) ? null :  Context?.Model.GetRecord<InsertRestrictionsType>(TargetPath, CapabilitiesConstants.InsertRestrictions);

            if (Context is not null && EntitySet is not null)
            {
                var entityInsertRestrictions = Context.Model.GetRecord<InsertRestrictionsType>(EntitySet, CapabilitiesConstants.InsertRestrictions);
                insertRestrictions?.MergePropertiesIfNull(entityInsertRestrictions);
                insertRestrictions ??= entityInsertRestrictions;
            }
            if (insertRestrictions?.IsInsertable ?? true)
            {
                AddOperation(item, HttpMethod.Post);
            }
        }

        /// <inheritdoc/>
        protected override void Initialize(ODataContext context, ODataPath path)
        {
            base.Initialize(context, path);

            // The first segment should be the entity set segment.
            if (path.FirstSegment is ODataNavigationSourceSegment {NavigationSource: IEdmEntitySet sourceSet})
                EntitySet = sourceSet;
        }
        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiPathItem pathItem)
        {
            base.SetBasicInfo(pathItem);
            pathItem.Description = $"Provides operations to manage the collection of {EntitySet?.EntityType.Name} entities.";
        }

        /// <inheritdoc/>
        protected override void SetExtensions(OpenApiPathItem item)
        {
            base.SetExtensions(item);
            if (EntitySet is null || Context is null) return;
            item.Extensions ??= new Dictionary<string, IOpenApiExtension>();
            item.Extensions.AddCustomAttributesToExtensions(Context, EntitySet);            
        }
    }
}
