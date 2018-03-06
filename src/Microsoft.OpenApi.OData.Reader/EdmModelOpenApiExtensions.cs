// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Xml;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Validation;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Exceptions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Annotations;
using Microsoft.OpenApi.OData.Common;
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

            model = model.AppendAnnotations();

            if (settings.VerifyEdmModel)
            {
                IEnumerable<EdmError> errors;
                if (!model.Validate(out errors))
                {
                    OpenApiDocument document = new OpenApiDocument();
                    int index = 1;
                    foreach (var error in errors)
                    {
                        document.Extensions.Add("x-edm-error-" + index++, new OpenApiString(error.ToString()));
                    }

                    return document;
                }
            }

            ODataContext context = new ODataContext(model, settings);
            return context.CreateDocument();
        }

        /// <summary>
        /// Convert CSDL to <see cref="OpenApiDocument"/> using a convert settings.
        /// </summary>
        /// <param name="csdl">The Edm CSDL.</param>
        /// <param name="settings">The convert settings.</param>
        /// <returns>The converted Open API document object, <see cref="OpenApiDocument"/>.</returns>
        public static OpenApiDocument ConvertToOpenApi(this string csdl, OpenApiConvertSettings settings)
        {
            if (csdl == null)
            {
                throw Error.ArgumentNull(nameof(csdl));
            }

            if (settings == null)
            {
                settings = new OpenApiConvertSettings(); // default
            }

            IEdmModel model = csdl.AppendAnnotations();

            ODataContext context = new ODataContext(model, settings);
            var a = context.Authorizations;
            return context.CreateDocument();
        }
    }
}
