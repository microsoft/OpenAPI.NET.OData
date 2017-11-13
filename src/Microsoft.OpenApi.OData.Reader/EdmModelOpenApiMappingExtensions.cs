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
    /// Extension methods to write Entity Data Model (EDM) to Open API.
    /// </summary>
    public static class EdmModelOpenApiMappingExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static OpenApiDocument Convert(this IEdmModel model)
        {
            return new OpenApiDocumentGenerator(model).Generate();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static OpenApiDocument Convert(this IEdmModel model, Action<OpenApiDocument> configure)
        {
            return new OpenApiDocumentGenerator(model, configure).Generate();
        }
    }
}
