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
    /// 
    /// </summary>
    public static class EdmOpenApiExtensions
    {
        public static OpenApiDocument Convert(this IEdmModel model)
        {
            return model.Convert(configure: null);
        }

        public static OpenApiDocument Convert(this IEdmModel model, Action<OpenApiDocument> configure)
        {
            return new OpenApiDocumentGenerator(model, settings).Generate();
        }
    }
}
