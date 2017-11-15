// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;

namespace Microsoft.OpenApi.OData
{
    /// <summary>
    /// Extension methods to create <see cref="OpenApiDocument"/> by <see cref="IEdmModel"/>.
    /// </summary>
    internal static class OpenApiDocumentGenerator
    {
        /// <summary>
        /// Create a <see cref="OpenApiDocument"/>, it's a single Open API Object.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <returns>The <see cref="OpenApiDocument"/> object.</returns>
        public static OpenApiDocument CreateDocument(this IEdmModel model)
        {
            if (model == null)
            {
                throw Error.ArgumentNull(nameof(model));
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

                Info = model.CreateInfo(),

                Servers = model.CreateServers(),

                Tags = model.CreateTags(),

                Paths = model.CreatePaths(),

                Components = model.CreateComponents(),

                ExternalDocs = null
            };
        }
    }
}
