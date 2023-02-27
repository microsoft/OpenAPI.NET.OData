// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
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

            OpenApiConvertSettings settings = context.Settings.Clone();
            settings.EnableKeyAsSegment = context.KeyAsSegment;
            foreach (ODataPath path in context.AllPaths)
            {
                IPathItemHandler handler = context.PathItemHanderProvider.GetHandler(path.Kind);
                if (handler == null)
                {
                    continue;
                }

                OpenApiPathItem pathItem = handler.CreatePathItem(context, path);
                if (!pathItem.Operations.Any())
                {
                    continue;
                }

                pathItems.TryAddPath(context, path, pathItem);
            }

            if (settings.ShowRootPath)
            {
                OpenApiPathItem rootPath = new()
                {
                    Operations = new Dictionary<OperationType, OpenApiOperation> {
                        {
                            OperationType.Get, new OpenApiOperation {
                                OperationId = "graphService.GetGraphService",
                                Responses = new OpenApiResponses()
                                {
                                    { "200",new OpenApiResponse() {
                                        Description = "OK",
                                        Links = CreateRootLinks(context.EntityContainer)
                                    }
                                }
                            }
                          }
                        }
                    }
                };
                pathItems.Add("/", rootPath);
            }

            return pathItems;
        }

        private static IDictionary<string, OpenApiLink> CreateRootLinks(IEdmEntityContainer entityContainer)
        {
            var links = new Dictionary<string, OpenApiLink>();
            foreach (var element in entityContainer.Elements)
            {
                links.Add(element.Name, new OpenApiLink());
            }
            return links;
        }
    }
}
