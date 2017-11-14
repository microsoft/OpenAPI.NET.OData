//---------------------------------------------------------------------
// <copyright file="EdmModelOpenApiExtensions.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Commons;
using Microsoft.OpenApi.OData.Generators;

namespace Microsoft.OpenApi.OData
{
    /// <summary>
    /// Extension methods to convert <see cref="IEdmModel"/>
    /// to Open API document, <see cref="OpenApiDocument"/>.
    /// </summary>
    public static class EdmModelOpenApiMappingExtensions
    {
        /// <summary>
        /// Convert <see cref="IEdmModel"/> to <see cref="OpenApiDocument"/> using a configure action.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="configure">The configure action.</param>
        /// <returns>The converted Open API document object.</returns>
        public static OpenApiDocument Convert(this IEdmModel model, Action<OpenApiDocument> configure)
        {
            if (model == null)
            {
                throw Error.ArgumentNull(nameof(model));
            }

            if (configure == null)
            {
                throw Error.ArgumentNull(nameof(configure));
            }

            OpenApiDocument document = model.CreateDocument();

            configure(document);

            return document;
        }

        /// <summary>
        /// Convert <see cref="IEdmModel"/> to <see cref="OpenApiDocument"/>.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <returns>The converted Open API document object.</returns>
        public static OpenApiDocument Convert(this IEdmModel model)
        {
            if (model == null)
            {
                throw Error.ArgumentNull(nameof(model));
            }

            return model.CreateDocument();
        }
    }
}
