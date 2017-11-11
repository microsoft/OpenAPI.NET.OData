//---------------------------------------------------------------------
// <copyright file="EdmModelOpenApiExtensions.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System.IO;
using Microsoft.OData.Edm;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;
using System;

namespace Microsoft.OpenApi.OData
{
    /// <summary>
    /// Extension methods to write Entity Data Model (EDM) to Open API.
    /// </summary>
    public static class EdmModelOpenApiMappingExtensions
    {
        /// <summary>
        /// Outputs Edm model to an Open API artifact to the give stream.
        /// </summary>
        /// <param name="model">Edm model to be written.</param>
        /// <param name="stream">The output stream.</param>
        /// <param name="target">The Open API target.</param>
        /// <param name="settings">Settings for the generated Open API.</param>
        public static OpenApiDocument Convert(this IEdmModel model)
        {
            if (model == null)
            {
                throw Error.ArgumentNull(nameof(model));
            }

            return new OpenApiDocument();
        }

        public static OpenApiDocument Convert(this IEdmModel model, Action<OpenApiDocument> configure)
        {
            if (model == null)
            {
                throw Error.ArgumentNull(nameof(model));
            }

            return new OpenApiDocument();
        }
    }
}
