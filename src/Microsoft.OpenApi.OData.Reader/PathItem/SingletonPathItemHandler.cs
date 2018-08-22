// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Capabilities;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Properties;

namespace Microsoft.OpenApi.OData.PathItem
{
    /// <summary>
    /// Create a <see cref="OpenApiPathItem"/> for <see cref="IEdmSingleton"/>.
    /// </summary>
    internal class SingletonPathItemHandler : PathItemHandler
    {
        /// <summary>
        /// Gets the singleton.
        /// </summary>
        protected IEdmSingleton Singleton { get; private set; }

        /// <inheritdoc/>
        protected override void SetOperations(OpenApiPathItem item)
        {
            // Retrieve a singleton.
            NavigationRestrictions navigation = new NavigationRestrictions(Context.Model, Singleton);
            if (navigation.IsNavigable)
            {
                AddOperation(item, OperationType.Get);
            }

            // Update a singleton
            UpdateRestrictions update = new UpdateRestrictions(Context.Model, Singleton);
            if (update.IsUpdatable)
            {
                AddOperation(item, OperationType.Patch);
            }
        }

        /// <inheritdoc/>
        protected override void Initialize(ODataContext context, ODataPath path)
        {
            if (path.Kind != ODataPathKind.Singleton)
            {
                throw Error.InvalidOperation(String.Format(SRResource.InvalidPathKindForPathItemHandler, nameof(SingletonPathItemHandler), path.Kind));
            }

            ODataNavigationSourceSegment navigationSourceSegment = path.FirstSegment as ODataNavigationSourceSegment;
            Singleton = navigationSourceSegment.NavigationSource as IEdmSingleton;
            base.Initialize(context, path);
        }
    }
}
