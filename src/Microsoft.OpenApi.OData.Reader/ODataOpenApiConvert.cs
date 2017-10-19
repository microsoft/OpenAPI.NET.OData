//---------------------------------------------------------------------
// <copyright file="ODataOpenApiConvert.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using Microsoft.OData.Edm;

namespace Microsoft.OData.OpenAPI
{
    internal static class ODataOpenApiConvert
    {
        public static OpenApiDocument ConvertTo(this IEdmModel model)
        {
            return model.ConvertTo(new OpenApiWriterSettings());
        }

        public static OpenApiDocument ConvertTo(this IEdmModel model, OpenApiWriterSettings settings)
        {
            return new EdmOpenApiDocumentGenerator(model, settings).Generate();
        }
    }
}
