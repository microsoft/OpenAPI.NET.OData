// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;

namespace Microsoft.OpenApi.OData.Generator
{
    /// <summary>
    /// Extension methods to create <see cref="OpenApiInfo"/> by <see cref="IEdmModel"/>.
    /// </summary>
    internal static class OpenApiInfoGenerator
    {
        /// <summary>
        /// Create a <see cref="OpenApiInfo"/> object.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="settings">The convert settings.</param>
        /// <returns>The created <see cref="OpenApiInfo"/> object.</returns>
        public static OpenApiInfo CreateInfo(this IEdmModel model, OpenApiConvertSettings settings)
        {
            if (model == null)
            {
                throw Error.ArgumentNull(nameof(model));
            }

            if (settings == null)
            {
                throw Error.ArgumentNull(nameof(settings));
            }

            // The value of info is an Info Object,
            // It contains the fields title and version, and optionally the field description.
            return new OpenApiInfo
            {
                Title = model.GetTitle(),
                Version = model.GetVersion(settings),
                Description = model.GetDescription(settings)
            };
        }

        private static string GetTitle(this IEdmModel model)
        {
            // The value of title is the value of the unqualified annotation Core.Description
            // of the main schema or the entity container of the OData service.
            // TODO: https://github.com/Microsoft/OpenAPI.NET.OData/issues/2

            //  or the entity container
            if (model.EntityContainer != null)
            {
                string longDescription = model.GetDescriptionAnnotation(model.EntityContainer);
                if (longDescription != null)
                {
                    return longDescription;
                }
            }

            // If no Core.Description is present, a default title has to be provided as this is a required OpenAPI field.
            return "OData Service for namespace " + model.DeclaredNamespaces.FirstOrDefault();
        }

        private static string GetVersion(this IEdmModel model, OpenApiConvertSettings settings)
        {
            // The value of version is the value of the annotation Core.SchemaVersion of the main schema.
            // If no Core.SchemaVersion is present, a default version has to be provided as this is a required OpenAPI field.
            // TODO: https://github.com/Microsoft/OpenAPI.NET.OData/issues/2

            return settings.Version.ToString();
        }

        private static string GetDescription(this IEdmModel model, OpenApiConvertSettings settings)
        {
            // The value of description is the value of the annotation Core.LongDescription of the main schema.
            // TODO: https://github.com/Microsoft/OpenAPI.NET.OData/issues/2

            //  or the entity container
            if (model.EntityContainer != null)
            {
                string longDescription = model.GetLongDescriptionAnnotation(model.EntityContainer);
                if (longDescription != null)
                {
                    return longDescription;
                }
            }

            // so a default description should be provided if no Core.LongDescription annotation is present.
            return "This OData service is located at " + settings.ServiceRoot.AbsolutePath;
        }
    }
}
