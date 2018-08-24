// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Capabilities;

namespace Microsoft.OpenApi.OData.PathItem
{
    /// <summary>
    /// Create a <see cref="OpenApiPathItem"/> for <see cref="IEdmEntitySet"/>.
    /// </summary>
    internal class EntitySetPathItemHandler : PathItemHandler
    {
        /// <inheritdoc/>
        protected override ODataPathKind HandleKind { get; } = ODataPathKind.EntitySet;

        /// <summary>
        /// Gets the entity set.
        /// </summary>
        protected IEdmEntitySet EntitySet { get; private set; }

        /// <inheritdoc/>
        protected override void SetOperations(OpenApiPathItem item)
        {
            NavigationRestrictions navigation = Context.Model.GetNavigationRestrictions(EntitySet);
            if (navigation == null || navigation.IsNavigable)
            {
                AddOperation(item, OperationType.Get);
            }

            InsertRestrictions insert = Context.Model.GetInsertRestrictions(EntitySet);
            if (insert == null || insert.IsInsertable)
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
    }
}
