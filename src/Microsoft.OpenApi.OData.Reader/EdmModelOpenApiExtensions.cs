// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Generator;

namespace Microsoft.OpenApi.OData
{
    /// <summary>
    /// Extension methods to convert <see cref="IEdmModel"/> to <see cref="OpenApiDocument"/>.
    /// </summary>
    public static class EdmModelOpenApiExtensions
    {
        /// <summary>
        /// Convert <see cref="IEdmModel"/> to <see cref="OpenApiDocument"/>.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <returns>The converted Open API document object.</returns>
        public static OpenApiDocument ConvertToOpenApi(this IEdmModel model)
        {
            return model.ConvertToOpenApi(new OpenApiConvertSettings());
        }

        /// <summary>
        /// Convert <see cref="IEdmModel"/> to <see cref="OpenApiDocument"/> with referenced model.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="referencedDocs">The referenced models.</param>
        /// <returns>The converted Open API document object.</returns>
        public static OpenApiDocument ConvertToOpenApi(this IEdmModel model,
            out IEnumerable<OpenApiDocument> referencedDocs)
        {
            return model.ConvertToOpenApi(new OpenApiConvertSettings(), out referencedDocs);
        }

        /// <summary>
        /// Convert <see cref="IEdmModel"/> to <see cref="OpenApiDocument"/> with referenced model.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="settings">The convert settings.</param>
        /// <param name="referencedDocs">The referenced models.</param>
        /// <returns>The converted Open API document object.</returns>
        public static OpenApiDocument ConvertToOpenApi(this IEdmModel model, OpenApiConvertSettings settings,
            out IEnumerable<OpenApiDocument> referencedDocs)
        {
            if (model == null)
            {
                throw Error.ArgumentNull(nameof(model));
            }

            referencedDocs = null;
            if (model.ReferencedModels != null && model.ReferencedModels.Any())
            {
                IList<OpenApiDocument> references = new List<OpenApiDocument>();
                foreach (var referencedModel in model.ReferencedModels)
                {
                    references.Add(referencedModel.ConvertToOpenApi(settings));
                }
                referencedDocs = references;
            }

            return model.ConvertToOpenApi(settings);
        }

        /// <summary>
        /// Convert <see cref="IEdmModel"/> to <see cref="OpenApiDocument"/> using a configure action.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="configure">The configure action.</param>
        /// <returns>The converted Open API document object.</returns>
        public static OpenApiDocument ConvertToOpenApi(this IEdmModel model, OpenApiConvertSettings settings)
        {
            if (model == null)
            {
                throw Error.ArgumentNull(nameof(model));
            }

            if (settings == null)
            {
                settings = new OpenApiConvertSettings(); // default
            }

            ODataContext context = new ODataContext(model, settings);
            return context.CreateDocument();
        }
    }
}
