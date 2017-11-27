// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using Microsoft.OpenApi.Models;

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
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            // An OAS document consists of a single OpenAPI Object represented as OpenApiDocument object.
            // {
            //   "openapi":"3.0.0",
            //   "info": …,
            //   "servers": …,
            //   "tags": …,
            //   "paths": …,
            //   "components": …
            // }
            return new OpenApiDocument
            {
                SpecVersion = new Version(3, 0, 0),

                Info = context.CreateInfo(),

                Servers = context.CreateServers(),

                Tags = context.CreateTags(),

                Paths = context.CreatePaths(),

                Components = context.CreateComponents(),

                SecurityRequirements = null,

                ExternalDocs = null
            };
        }
    }
}
