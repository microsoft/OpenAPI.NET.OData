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
    /// Base class for operation of media entity.
    /// </summary>
    internal abstract class MediaEntityOperationalHandler : NavigationPropertyOperationHandler
    {
        /// <summary>
        /// Gets/sets the <see cref="IEdmEntitySet"/>.
        /// </summary>
        protected IEdmEntitySet EntitySet { get; private set; }

        /// <summary>
        /// Gets the <see cref="IEdmSingleton"/>.
        /// </summary>
        protected IEdmSingleton Singleton { get; private set; }

        /// <inheritdoc/>
        protected override void Initialize(ODataContext context, ODataPath path)
        {
            // The first segment will either be an entity set navigation source or a singleton navigation source.
            ODataNavigationSourceSegment navigationSourceSegment = path.FirstSegment as ODataNavigationSourceSegment;
            EntitySet = navigationSourceSegment.NavigationSource as IEdmEntitySet;

            if (EntitySet == null)
            {
                // Singleton
                base.Initialize(context, path);
                Singleton = NavigationSource as IEdmSingleton;
            }
        }

        /// <inheritdoc/>
        protected override void SetTags(OpenApiOperation operation)
        {
            if (EntitySet == null)
            {
                // Singleton
                base.SetTags(operation);
            }
            else // Entityset
            {
                string tagIdentifier = EntitySet.Name + "." + EntitySet.EntityType().Name;

                OpenApiTag tag = new OpenApiTag
                {
                    Name = tagIdentifier
                };

                // Use an extension for TOC (Table of Content)
                tag.Extensions.Add(Constants.xMsTocType, new OpenApiString("page"));

                operation.Tags.Add(tag);

                Context.AppendTag(tag);
            }
        }

        /// <inheritdoc/>
        protected override void SetExtensions(OpenApiOperation operation)
        {
            base.SetExtensions(operation);
        }

        /// <summary>
        /// Retrieves the operation Id for a navigation property stream path.
        /// </summary>
        /// <param name="prefix">The http method identifier name.</param>
        /// <param name="identifier">The stream segment identifier name.</param>
        /// <returns></returns>
        protected string GetOperationId(string prefix, string identifier)
        {
            Utils.CheckArgumentNull(prefix, nameof(prefix));
            Utils.CheckArgumentNull(identifier, nameof(identifier));

            IList<string> items = new List<string>
            {
                NavigationSource.Name
            };

            var lastpath = Path.Segments.Last(c => c is ODataStreamContentSegment || c is ODataStreamPropertySegment);
            foreach (var segment in Path.Segments.Skip(1))
            {
                if (segment == lastpath)
                {
                    items.Add(prefix + Utils.UpperFirstChar(identifier));
                    break;
                }
                else
                {
                    if (segment is ODataNavigationPropertySegment npSegment)
                    {
                        items.Add(npSegment.NavigationProperty.Name);
                    }
                }
            }

            return string.Join(".", items);
        }
    }
}
