// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;

namespace Microsoft.OpenApi.OData.Generator
{
    /// <summary>
    /// Class to create <see cref="OpenApiDocument"/> by <see cref="IEdmModel"/>.
    /// </summary>
    internal class OpenApiDocumentGenerator : OpenApiGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenApiDocumentGenerator" /> class.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="settings">The Open Api convert settings.</param>
        public OpenApiDocumentGenerator(IEdmModel model, OpenApiConvertSettings settings)
            : base(model, settings)
        {
        }

        /// <summary>
        /// Create a <see cref="OpenApiDocument"/>, it's a single Open API Object.
        /// </summary>
        /// <returns>The created <see cref="OpenApiDocument"/> object.</returns>
        public static OpenApiDocument CreateDocument(this IEdmModel model, OpenApiConvertSettings settings)
        {
            if (model == null)
            {
                throw Error.ArgumentNull(nameof(model));
            }

            if (settings == null)
            {
                throw Error.ArgumentNull(nameof(settings));
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

                Info = model.CreateInfo(settings),

                Servers = model.CreateServers(settings),

                Tags = model.CreateTags(settings),

                Paths = model.CreatePaths(settings),

                Components = model.CreateComponents(settings)
            };
        }
    }
}
