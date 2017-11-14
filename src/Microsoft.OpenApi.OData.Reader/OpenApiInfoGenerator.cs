// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;

namespace Microsoft.OpenApi.OData
{
    /// <summary>
    /// Extension methods to create <see cref="OpenApiInfo"/> by <see cref="IEdmModel"/>.
    /// </summary>
    internal static class OpenApiInfoGenerator
    {
        /// <summary>
        /// Create <see cref="OpenApiInfo"/> object.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <returns>The info object.</returns>
        public static OpenApiInfo CreateInfo(this IEdmModel model)
        {
            if (model == null)
            {
                throw Error.ArgumentNull(nameof(model));
            }

            // The value of info is an Info Object,
            // It contains the fields title and version, and optionally the field description.
            return new OpenApiInfo
            {
                // The value of title is the value of the unqualified annotation Core.Description
                // of the main schema or the entity container of the OData service.
                // If no Core.Description is present, a default title has to be provided as this is a required OpenAPI field.
                Title = "OData Service for namespace " + model.DeclaredNamespaces.FirstOrDefault(),

                // The value of version is the value of the annotation Core.SchemaVersion(see[OData - VocCore]) of the main schema.
                // If no Core.SchemaVersion is present, a default version has to be provided as this is a required OpenAPI field.
                Version = "0.1.0",

                // The value of description is the value of the annotation Core.LongDescription
                // of the main schema or the entity container.
                // While this field is optional, it prominently appears in OpenAPI exploration tools,
                // so a default description should be provided if no Core.LongDescription annotation is present.
                // Description = "This OData service is located at " + Settings.BaseUri?.OriginalString
            };
        }
    }
}
