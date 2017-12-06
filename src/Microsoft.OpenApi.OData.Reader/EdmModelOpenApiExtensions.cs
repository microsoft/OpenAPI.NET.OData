// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

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
        /// <returns>The converted Open API document object, <see cref="OpenApiDocument"/>.</returns>
        public static OpenApiDocument ConvertToOpenApi(this IEdmModel model)
        {
            return model.ConvertToOpenApi(new OpenApiConvertSettings());
        }

        /// <summary>
        /// Convert <see cref="IEdmModel"/> to <see cref="OpenApiDocument"/> using a convert settings.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="settings">The convert settings.</param>
        /// <returns>The converted Open API document object, <see cref="OpenApiDocument"/>.</returns>
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
