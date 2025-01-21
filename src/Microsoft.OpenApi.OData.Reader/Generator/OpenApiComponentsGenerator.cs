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
        public static void AddComponentsToDocument(this ODataContext context, OpenApiDocument document)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(document, nameof(document));

            context.AddSchemasToDocument(document);
            context.AddParametersToDocument(document);
            context.AddResponsesToDocument(document);
            context.AddRequestBodiesToDocument(document);
            context.AddExamplesToDocument(document);
            context.AddSecuritySchemesToDocument(document);
            document.Components.Links = null;
            document.Components.Callbacks = null;
            document.Components.Extensions = null;
        }
    }
}
