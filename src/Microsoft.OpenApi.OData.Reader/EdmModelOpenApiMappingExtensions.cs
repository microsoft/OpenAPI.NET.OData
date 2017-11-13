//---------------------------------------------------------------------
// <copyright file="EdmModelOpenApiExtensions.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;

namespace Microsoft.OpenApi.OData
{
    /// <summary>
    /// Extension methods to convert <see cref="IEdmModel"/>
    /// to Open API document, <see cref="OpenApiDocument"/>.
    /// </summary>
    public static class EdmModelOpenApiMappingExtensions
    {
        /// <summary>
        /// Convert <see cref="IEdmModel"/> to <see cref="OpenApiDocument"/>.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <returns>The converted Open API document object.</returns>
        public static OpenApiDocument Convert(this IEdmModel model)
        {
            return new OpenApiDocumentGenerator(model).Generate();
        }

        /// <summary>
        /// Convert <see cref="IEdmModel"/> to <see cref="OpenApiDocument"/> using a configure action.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="configure">The configure action.</param>
        /// <returns>The converted Open API document object.</returns>
        public static OpenApiDocument Convert(this IEdmModel model, Action<OpenApiDocument> configure)
        {
            return new OpenApiDocumentGenerator(model, configure).Generate();
        }
    }
}
