using Microsoft.OData.OpenAPI.Properties;
using System;

namespace Microsoft.OData.OpenAPI
{
    internal static class OpenApiElementValidator
    {
        public static void ValidateReference(this IOpenApiReferencable reference)
        {
            if (reference != null && reference.Reference != null)
            {
                throw new OpenApiException(String.Format(SRResource.OpenApiObjectMarkAsReference, reference.GetType().Name));
            }
        }
    }
}
