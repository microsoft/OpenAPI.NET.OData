// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Linq;
using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Capabilities;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.Any;

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
                    if (LastSegmentIsKeySegment)
                    {
                        UpdateRestrictions update = new UpdateRestrictions(Context.Model, NavigationProperty);
                        if (update.IsUpdatable())
                        {
                            AddOperation(item, OperationType.Patch);
                        }
                    }
                    else
                    {
                        InsertRestrictions insert = new InsertRestrictions(Context.Model, NavigationProperty);
                        if (insert.IsInsertable())
                        {
                            AddOperation(item, OperationType.Post);
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

        /// <inheritdoc/>
        protected override void SetExtensions(OpenApiPathItem item)
        {
            IList<ODataPath> samePaths = new List<ODataPath>();
            foreach (var path in Context.Paths.Where(p => p.Kind == ODataPathKind.NavigationProperty && p != Path))
            {
                bool lastIsKeySegment = path.LastSegment is ODataKeySegment;
                if (LastSegmentIsKeySegment != lastIsKeySegment)
                {
                    continue;
                }

                ODataNavigationSourceSegment navigationSourceSegment = path.FirstSegment as ODataNavigationSourceSegment;
                if (NavigationSource != navigationSourceSegment.NavigationSource)
                {
                    continue;
                }

                ODataNavigationPropertySegment npSegment = path.LastSegment as ODataNavigationPropertySegment;
                if (npSegment == null)
                {
                    npSegment = path.Segments[path.Count - 2] as ODataNavigationPropertySegment;
                }
                if (NavigationProperty != npSegment.NavigationProperty)
                {
                    continue;
                }

                samePaths.Add(path);
            }

            if (samePaths.Any())
            {
                OpenApiArray array = new OpenApiArray();
                foreach(var p in samePaths)
                {
                    array.Add(new OpenApiString(p.GetPathItemName(Context.Settings)));
                }

                item.Extensions.Add(Constants.xMsDosGroupPath, array);
            }
        }
    }
}
