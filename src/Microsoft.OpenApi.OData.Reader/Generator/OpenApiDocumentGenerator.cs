﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.Generator
{
    /// <summary>
    /// Extension methods to create <see cref="OpenApiDocument"/> by Edm model.
    /// </summary>
    internal static class OpenApiDocumentGenerator
    {
        /// <summary>
        /// Create a <see cref="OpenApiDocument"/>, it's a single Open API Object.
        /// </summary>
        /// <param name="context">The OData to Open API context.</param>
        /// <returns>The created <see cref="OpenApiDocument"/> object.</returns>
        public static OpenApiDocument CreateDocument(this ODataContext context)
        {
            Utils.CheckArgumentNull(context, nameof(context));

            // An OAS document consists of a single OpenAPI Object represented as OpenApiDocument object.
            // {
            //   "openapi":"3.0.0",
            //   "info": …,
            //   "servers": …,
            //   "tags": …,
            //   "paths": …,
            //   "components": …
            // }
            OpenApiDocument doc = new()
            {
                Info = context.CreateInfo(),

                Servers = context.CreateServers(),

                Security = null,

                ExternalDocs = null,
            };

            context.AddComponentsToDocument(doc);
            context.AddPathsToDocument(doc);
            doc.Tags = context.CreateTags(); // order matters so the operation generators have populated the tags


            return doc;
        }
    }
}
