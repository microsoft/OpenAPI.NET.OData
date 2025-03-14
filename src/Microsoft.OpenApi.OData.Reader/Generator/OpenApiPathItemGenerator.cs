// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.Interfaces;
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
        /// <param name="document">The Open API document to use to lookup references.</param>
        public static void AddPathItemsToDocument(this ODataContext context, OpenApiDocument document)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(document, nameof(document));

            if (context.EntityContainer == null)
            {
                return;
            }

            document.Paths ??= [];
            OpenApiConvertSettings settings = context.Settings.Clone();
            settings.EnableKeyAsSegment = context.KeyAsSegment;
            foreach (ODataPath path in context.AllPaths)
            {
                IPathItemHandler handler = context.PathItemHandlerProvider.GetHandler(path.Kind, document);
                if (handler == null)
                {
                    continue;
                }

                OpenApiPathItem pathItem = handler.CreatePathItem(context, path);
                if (!pathItem.Operations.Any())
                {
                    continue;
                }

                document.Paths.TryAddPath(context, path, pathItem);
            }

            if (settings.ShowRootPath)
            {
                OpenApiPathItem rootPath = new()
                {
                    Operations = new Dictionary<HttpMethod, OpenApiOperation> {
                        {
                            HttpMethod.Get, new OpenApiOperation {
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
                document.Paths.Add("/", rootPath);
            }
        }

        private static Dictionary<string, IOpenApiLink> CreateRootLinks(IEdmEntityContainer entityContainer)
        {
            var links = new Dictionary<string, IOpenApiLink>();
            foreach (var element in entityContainer.Elements)
            {
                links.Add(element.Name, new OpenApiLink());
            }
            return links;
        }
    }
}
