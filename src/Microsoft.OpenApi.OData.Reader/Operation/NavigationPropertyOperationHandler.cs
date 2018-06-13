// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.OpenApi.OData.Operation
{
    /// <summary>
    /// Base class for operation of <see cref="IEdmNavigationProperty"/>.
    /// </summary>
    internal abstract class NavigationPropertyOperationHandler : OperationHandler
    {
        /// <summary>
        /// Gets the navigation property.
        /// </summary>
        protected IEdmNavigationProperty NavigationProperty { get; private set; }

        /// <summary>
        /// Gets the navigation source.
        /// </summary>
        protected IEdmNavigationSource NavigationSource { get; private set; }

        /// <summary>
        /// Gets a bool value indicating whether the last segment is a key segment.
        /// </summary>
        protected bool LastSegmentIsKeySegment { get; private set; }

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
        protected override void SetTags(OpenApiOperation operation)
        {
            IList<string> items = new List<string>
            {
                NavigationSource.Name
            };

            foreach (var segment in Path.Segments.Skip(1))
            {
                if (segment is ODataNavigationPropertySegment)
                {
                    ODataNavigationPropertySegment npSegment = (ODataNavigationPropertySegment)segment;
                    if (npSegment.NavigationProperty == NavigationProperty)
                    {
                        items.Add(NavigationProperty.ToEntityType().Name);
                        break;
                    }
                    else
                    {
                        if (items.Count >= Context.Settings.TagDepth - 1)
                        {
                            items.Add(npSegment.NavigationProperty.ToEntityType().Name);
                            break;
                        }
                        else
                        {
                            items.Add(segment.Name);
                        }
                    }
                }
            }
            
            string name = string.Join(".", items);
            OpenApiTag tag = new OpenApiTag
            {
                // Name = NavigationSource.Name + "." + NavigationProperty.ToEntityType().Name,
                Name = name
            };
            tag.Extensions.Add(Constants.xMsTocType, new OpenApiString("page"));
            operation.Tags.Add(tag);

            Context.AppendTag(tag);
        }

        protected string GetOperationId(string prefix)
        {
            IList<string> items = new List<string>
            {
                NavigationSource.Name
            };

            foreach (var segment in Path.Segments.Skip(1))
            {
                if (segment is ODataNavigationPropertySegment)
                {
                    ODataNavigationPropertySegment npSegment = (ODataNavigationPropertySegment)segment;
                    if (npSegment.NavigationProperty == NavigationProperty)
                    {
                        items.Add(prefix + Utils.UpperFirstChar(NavigationProperty.ToEntityType().Name));
                        break;
                    }
                    else
                    {
                        items.Add(segment.Name);
                    }
                }
            }

            return string.Join(".", items);
        }
    }
}
