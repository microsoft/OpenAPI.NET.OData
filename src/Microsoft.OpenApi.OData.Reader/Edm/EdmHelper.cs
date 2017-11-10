//---------------------------------------------------------------------
// <copyright file="EdmHelper.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Commons;
using System.Diagnostics;

namespace Microsoft.OData.OpenAPI
{
    internal static class EdmHelper
    {
        public static string GetOpenApiTypeName(this IEdmTypeReference edmType)
        {
            Debug.Assert(edmType != null);

            return edmType.Definition.GetOpenApiTypeName();
        }

        public static string GetOpenApiTypeName(this IEdmType edmType)
        {
            Debug.Assert(edmType != null);

            switch (edmType.TypeKind)
            {
                case EdmTypeKind.Collection:
                    return AnyTypeKind.Array.GetDisplayName();

                case EdmTypeKind.Complex:
                case EdmTypeKind.Entity:
                case EdmTypeKind.EntityReference:
                    return AnyTypeKind.Object.GetDisplayName();

                case EdmTypeKind.Enum:
                    return "string";

                case EdmTypeKind.Primitive:
                    return ((IEdmPrimitiveType)(edmType)).GetOpenApiDataType().GetCommonName();

                default:
                    return AnyTypeKind.Null.GetDisplayName();
            }
        }
    }
}
