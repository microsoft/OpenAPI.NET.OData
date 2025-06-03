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
    /// Create a <see cref="OpenApiPathItem"/> for <see cref="IEdmSingleton"/>.
    /// </summary>
    internal class SingletonPathItemHandler : PathItemHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SingletonPathItemHandler"/> class.
        /// </summary>
        /// <param name="document">The document to use for references lookup.</param>
        public SingletonPathItemHandler(OpenApiDocument document): base(document)
        {
            
        }
        /// <inheritdoc/>
        protected override ODataPathKind HandleKind => ODataPathKind.Singleton;

        /// <summary>
        /// Gets the singleton.
        /// </summary>
        protected IEdmSingleton? Singleton { get; private set; }

        /// <inheritdoc/>
        protected override void SetOperations(OpenApiPathItem item)
        {
            // Retrieve a singleton.
            var readRestrictions = string.IsNullOrEmpty(TargetPath) ? null : Context?.Model.GetRecord<ReadRestrictionsType>(TargetPath, CapabilitiesConstants.ReadRestrictions);
            
            if (Context is not null && Singleton is not null)
            {
                var singletonReadRestrictions = Context.Model.GetRecord<ReadRestrictionsType>(Singleton, CapabilitiesConstants.ReadRestrictions);
                readRestrictions?.MergePropertiesIfNull(singletonReadRestrictions);
                readRestrictions ??= singletonReadRestrictions;
            }
            if (readRestrictions?.IsReadable ?? true)
            {
                AddOperation(item, HttpMethod.Get);
            }

            // Update a singleton
            var updateRestrictions = string.IsNullOrEmpty(TargetPath) ? null :  Context?.Model.GetRecord<UpdateRestrictionsType>(TargetPath, CapabilitiesConstants.UpdateRestrictions);
            if (Context is not null && Singleton is not null)
            {
                var singletonUpdateRestrictions = Context.Model.GetRecord<UpdateRestrictionsType>(Singleton, CapabilitiesConstants.UpdateRestrictions);
                updateRestrictions?.MergePropertiesIfNull(singletonUpdateRestrictions);
                updateRestrictions ??= singletonUpdateRestrictions;
            }
            if (updateRestrictions?.IsUpdatable ?? true)
            {
                AddOperation(item, HttpMethod.Patch);
            }
        }

        /// <inheritdoc/>
        protected override void Initialize(ODataContext context, ODataPath path)
        {
            base.Initialize(context, path);

            if (path.FirstSegment is ODataNavigationSourceSegment {NavigationSource: IEdmSingleton source})
                Singleton = source;
        }

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiPathItem pathItem)
        {
            base.SetBasicInfo(pathItem);
            pathItem.Description = $"Provides operations to manage the {Singleton?.EntityType.Name} singleton.";
        }

        /// <inheritdoc/>
        protected override void SetExtensions(OpenApiPathItem item)
        {
            base.SetExtensions(item);

            if (Context is not null && Singleton is not null)
            {
                item.Extensions ??= new Dictionary<string, IOpenApiExtension>();
                item.Extensions.AddCustomAttributesToExtensions(Context, Singleton);            
            }
        }
    }
}
