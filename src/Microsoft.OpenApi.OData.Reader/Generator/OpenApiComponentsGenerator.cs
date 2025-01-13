// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.Generator
{
    /// <summary>
    /// Extension methods to create <see cref="OpenApiComponents"/> by Edm model.
    /// </summary>
    internal static class OpenApiComponentsGenerator
    {
        /// <summary>
        /// Create a <see cref="OpenApiComponents"/>.
        /// The value of components is a Components Object.
        /// It holds maps of reusable schemas describing message bodies, operation parameters, and responses.
        /// </summary>
        /// <param name="context">The OData to Open API context.</param>
        /// <param name="document">The Open API document.</param>
        /// <returns>The created <see cref="OpenApiComponents"/> object.</returns>
        public static OpenApiComponents CreateComponents(this ODataContext context, OpenApiDocument document)
        {
            Utils.CheckArgumentNull(context, nameof(context));

            context.AddSchemasToDocument(document);
            //TODO convert all other create methods to add
            document.Components.Parameters = context.CreateParameters();
            document.Components.Responses = context.CreateResponses(document);
            document.Components.RequestBodies = context.CreateRequestBodies(document);
            document.Components.Examples = context.CreateExamples(document);
            document.Components.SecuritySchemes = context.CreateSecuritySchemes();
            document.Components.Links = null;
            document.Components.Callbacks = null;
            document.Components.Extensions = null;
            return document.Components;
        }
    }
}
