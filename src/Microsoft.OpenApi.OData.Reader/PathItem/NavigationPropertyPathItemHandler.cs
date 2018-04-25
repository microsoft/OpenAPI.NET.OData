// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Capabilities;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.PathItem
{
    /// <summary>
    /// Create a <see cref="OpenApiPathItem"/> for a single <see cref="IEdmNavigationProperty"/>.
    /// </summary>
    internal class NavigationPropertyPathItemHandler : PathItemHandler
    {
        /// <summary>
        /// Gets the navigation property.
        /// </summary>
        public IEdmNavigationProperty NavigationProperty { get; private set; }

        /// <summary>
        /// Gets the navigation source.
        /// </summary>
        public IEdmNavigationSource NavigationSource { get; private set; }

        /// <summary>
        /// Gets a bool value indicating whether the last segment is a key segment.
        /// </summary>
        protected bool LastSegmentIsKeySegment { get; private set; }

        /// <inheritdoc/>
        protected override void SetOperations(OpenApiPathItem item)
        {
            // contaiment: Get / (Post - Collection | Patch - Single)
            // non-containment: only Get
            AddOperation(item, OperationType.Get);

            if (NavigationProperty.ContainsTarget)
            {
                if (NavigationProperty.TargetMultiplicity() == EdmMultiplicity.Many)
                {
                    InsertRestrictions insert = new InsertRestrictions(Context.Model, NavigationProperty);
                    if (insert.IsInsertable())
                    {
                        AddOperation(item, OperationType.Post);
                    }

                    if (LastSegmentIsKeySegment)
                    {
                        UpdateRestrictions update = new UpdateRestrictions(Context.Model, NavigationProperty);
                        if (update.IsUpdatable())
                        {
                            AddOperation(item, OperationType.Patch);
                        }
                    }
                }
                else
                {
                    UpdateRestrictions update = new UpdateRestrictions(Context.Model, NavigationProperty);
                    if (update.IsUpdatable())
                    {
                        AddOperation(item, OperationType.Patch);
                    }
                }
            }
        }

        /// <inheritdoc/>
        protected override void Initialize(ODataContext context, ODataPath path)
        {
            ODataNavigationSourceSegment navigationSourceSegment = path.FirstSegment as ODataNavigationSourceSegment;
            NavigationSource = navigationSourceSegment.NavigationSource;

            LastSegmentIsKeySegment = path.LastSegment is ODataKeySegment;
            ODataNavigationPropertySegment npSegment = path.LastSegment as ODataNavigationPropertySegment;
            if (npSegment == null)
            {
                npSegment = path.Segments[path.Count - 2] as ODataNavigationPropertySegment;
            }
            NavigationProperty = npSegment.NavigationProperty;

            base.Initialize(context, path);
        }
    }
}
