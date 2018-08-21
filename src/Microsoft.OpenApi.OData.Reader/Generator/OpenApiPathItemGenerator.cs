// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.PathItem;

namespace Microsoft.OpenApi.OData.Generator
{
    /// <summary>
    /// Class to create <see cref="OpenApiPathItem"/> by Edm elements.
    /// </summary>
    internal static class OpenApiPathItemGenerator
    {
        /// <summary>
        /// Create a map of <see cref="OpenApiPathItem"/>.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <returns>The created map of <see cref="OpenApiPathItem"/>.</returns>
        public static IDictionary<string, OpenApiPathItem> CreatePathItems(this ODataContext context)
        {
            Utils.CheckArgumentNull(context, nameof(context));

            IDictionary<string, OpenApiPathItem> pathItems = new Dictionary<string, OpenApiPathItem>();
            if (context.EntityContainer == null)
            {
                return pathItems;
            }

            foreach (ODataPath path in context.Paths)
            {
                IPathItemHandler handler = context.PathItemHanderProvider.GetHandler(path.Kind);
                pathItems.Add(path.GetPathItemName(context.Settings), handler.CreatePathItem(context, path));
            }

            return pathItems;
        }
    }
}
