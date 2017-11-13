//---------------------------------------------------------------------
// <copyright file="ODataOpenApiConvert.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using System;

namespace Microsoft.OpenApi.OData
{
    /// <summary>
    /// Convert <see cref="IEdmModel"/> to Open API document, <see cref="OpenApiDocument"/>.
    /// </summary>
    public static class EdmModelOpenApiExtensions
    {
        public static OpenApiDocument Convert(this IEdmModel model)
        {
            return new OpenApiDocumentGenerator(model).Generate();
        }

        public static OpenApiDocument Convert(this IEdmModel model, Action<OpenApiDocument> configure)
        {
            return new OpenApiDocumentGenerator(model, configure).Generate();
        }
    }
}
