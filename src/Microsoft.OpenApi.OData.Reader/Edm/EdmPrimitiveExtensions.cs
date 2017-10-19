//---------------------------------------------------------------------
// <copyright file="EdmPrimitiveExtensions.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData.Edm;

namespace Microsoft.OData.OpenAPI
{
    /// <summary>
    /// Edm primitive type extension methods.
    /// </summary>
    internal static class EdmPrimitiveExtensions
    {
        private static readonly Dictionary<EdmPrimitiveTypeKind, OpenApiDataTypeKind> _builtInTypesMapping =
            new Dictionary<EdmPrimitiveTypeKind, OpenApiDataTypeKind>
        {
            { EdmPrimitiveTypeKind.None, OpenApiDataTypeKind.None },
            { EdmPrimitiveTypeKind.Binary, OpenApiDataTypeKind.Binary },
            { EdmPrimitiveTypeKind.Boolean, OpenApiDataTypeKind.Boolean },
            { EdmPrimitiveTypeKind.Byte, OpenApiDataTypeKind.Byte },
            { EdmPrimitiveTypeKind.DateTimeOffset, OpenApiDataTypeKind.DateTime }, // TODO:
            { EdmPrimitiveTypeKind.Decimal, OpenApiDataTypeKind.Double },
            { EdmPrimitiveTypeKind.Double, OpenApiDataTypeKind.Double },
            { EdmPrimitiveTypeKind.Guid, OpenApiDataTypeKind.String }, // TODO:
            { EdmPrimitiveTypeKind.Int16, OpenApiDataTypeKind.Integer },
            { EdmPrimitiveTypeKind.Int32, OpenApiDataTypeKind.Integer },
            { EdmPrimitiveTypeKind.Int64, OpenApiDataTypeKind.Long },
            { EdmPrimitiveTypeKind.SByte, OpenApiDataTypeKind.Integer },
            { EdmPrimitiveTypeKind.Single, OpenApiDataTypeKind.Float },
            { EdmPrimitiveTypeKind.String, OpenApiDataTypeKind.String },
            { EdmPrimitiveTypeKind.Stream, OpenApiDataTypeKind.String }, // TODO:
            { EdmPrimitiveTypeKind.Duration, OpenApiDataTypeKind.DateTime },
            { EdmPrimitiveTypeKind.Date, OpenApiDataTypeKind.DateTime },
            { EdmPrimitiveTypeKind.TimeOfDay, OpenApiDataTypeKind.DateTime },

            { EdmPrimitiveTypeKind.Geography, OpenApiDataTypeKind.String },
            { EdmPrimitiveTypeKind.GeographyPoint, OpenApiDataTypeKind.String },
            { EdmPrimitiveTypeKind.GeographyLineString, OpenApiDataTypeKind.String },
            { EdmPrimitiveTypeKind.GeographyPolygon, OpenApiDataTypeKind.String },
            { EdmPrimitiveTypeKind.GeographyCollection, OpenApiDataTypeKind.String },
            { EdmPrimitiveTypeKind.GeographyMultiPolygon, OpenApiDataTypeKind.String },
            { EdmPrimitiveTypeKind.GeographyMultiLineString, OpenApiDataTypeKind.String },
            { EdmPrimitiveTypeKind.GeographyMultiPoint, OpenApiDataTypeKind.String },

            { EdmPrimitiveTypeKind.Geometry, OpenApiDataTypeKind.String },
            { EdmPrimitiveTypeKind.GeometryPoint, OpenApiDataTypeKind.String },
            { EdmPrimitiveTypeKind.GeometryLineString, OpenApiDataTypeKind.String },
            { EdmPrimitiveTypeKind.GeometryPolygon, OpenApiDataTypeKind.String },
            { EdmPrimitiveTypeKind.GeometryCollection, OpenApiDataTypeKind.String },
            { EdmPrimitiveTypeKind.GeometryMultiPolygon, OpenApiDataTypeKind.String },
            { EdmPrimitiveTypeKind.GeometryMultiLineString, OpenApiDataTypeKind.String },
            { EdmPrimitiveTypeKind.GeometryMultiPoint, OpenApiDataTypeKind.String },
        };

        /// <summary>
        /// Get Open Api Data type kind.
        /// </summary>
        /// <param name="primitiveType">The primitive type.</param>
        /// <returns>The Open Api Data type kind.</returns>
        public static OpenApiDataTypeKind GetOpenApiDataType(this IEdmPrimitiveTypeReference primitiveType)
        {
            if (primitiveType == null)
            {
                return OpenApiDataTypeKind.None;
            }

            return GetOpenApiDataType(primitiveType.PrimitiveKind());
        }

        /// <summary>
        /// Get Open Api Data type kind.
        /// </summary>
        /// <param name="primitiveType">The primitive type.</param>
        /// <returns>The Open Api Data type kind.</returns>
        public static OpenApiDataTypeKind GetOpenApiDataType(this IEdmPrimitiveType typeReference)
        {
            if (typeReference == null)
            {
                return OpenApiDataTypeKind.None;
            }

            return GetOpenApiDataType(typeReference.PrimitiveKind);
        }

        /// <summary>
        /// Get Open Api Data type kind.
        /// </summary>
        /// <param name="primitiveType">The primitive type kind.</param>
        /// <returns>The Open Api Data type kind.</returns>
        private static OpenApiDataTypeKind GetOpenApiDataType(this EdmPrimitiveTypeKind primitiveKind)
        {
            return _builtInTypesMapping[primitiveKind];
        }
    }
}
