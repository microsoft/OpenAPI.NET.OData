// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
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
        public static IDictionary<string, OpenApiPathItem> CreatePathItems(this ODataContext context)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            IDictionary<string, OpenApiPathItem> pathItems = new Dictionary<string, OpenApiPathItem>();
            int count = context.Paths.Count;
            int index = 1;
            foreach (ODataPath path in context.Paths)
            {
                index++;
                IPathItemHandler handler = context.PathItemHanderProvider.GetHandler(path.PathType);

                pathItems.Add(path.ToString(), handler.CreatePathItem(context, path));

                Console.Write(index + "/"  + count + " ....");
                Console.Write("\r\b");
            }

            return pathItems;
        }
    }
}
