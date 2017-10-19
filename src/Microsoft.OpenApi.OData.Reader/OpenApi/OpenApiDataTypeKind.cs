//---------------------------------------------------------------------
// <copyright file="OpenApiDataTypeKind.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

namespace Microsoft.OData.OpenAPI
{
    /// <summary>
    /// Primitive Data Types in the Open Api Specification.
    /// </summary>
    internal enum OpenApiDataTypeKind
    {
        /// <summary>
        /// None.
        /// </summary>
        [OpenApiDataTypeMetadata]
        None,

        /// <summary>
        /// Integer
        /// </summary>
        [OpenApiDataTypeMetadata(CommonName = "integer", Type = "integer", Format = "int32", Comments ="signed 32 bits")]
        Integer,

        /// <summary>
        /// Long
        /// </summary>
        [OpenApiDataTypeMetadata(CommonName = "long", Type = "integer", Format = "int642", Comments = "signed 64 bits")]
        Long,

        /// <summary>
        /// Float
        /// </summary>
        [OpenApiDataTypeMetadata(CommonName = "float", Type = "number", Format = "float")]
        Float,

        /// <summary>
        /// Double
        /// </summary>
        [OpenApiDataTypeMetadata(CommonName = "double", Type = "number", Format = "double")]
        Double,

        /// <summary>
        /// String
        /// </summary>
        [OpenApiDataTypeMetadata(CommonName = "string", Type = "string")]
        String,

        /// <summary>
        /// Byte
        /// </summary>
        [OpenApiDataTypeMetadata(CommonName = "byte", Type = "string", Format = "byte", Comments = "base64 encoded characters")]
        Byte,

        /// <summary>
        /// Binary
        /// </summary>
        [OpenApiDataTypeMetadata(CommonName = "binary", Type = "string", Format = "binary", Comments = "any sequence of octets")]
        Binary,

        /// <summary>
        /// Boolean
        /// </summary>
        [OpenApiDataTypeMetadata(CommonName = "boolean", Type = "boolean")]
        Boolean,

        /// <summary>
        /// Date
        /// </summary>
        [OpenApiDataTypeMetadata(CommonName = "date", Type = "string", Format = "date", Comments = "As defined by full-date - RFC3339")]
        Date,

        /// <summary>
        /// DateTime
        /// </summary>
        [OpenApiDataTypeMetadata(CommonName = "dateTime", Type = "string", Format = "date-time", Comments = "As defined by date-time - RFC3339")]
        DateTime,

        /// <summary>
        /// Password
        /// </summary>
        [OpenApiDataTypeMetadata(CommonName = "password", Type = "string", Format = "password", Comments = "A hint to UIs to obscure input")]
        Password
    }

    /// <summary>
    /// Extension methods for Open Api Data Type kind.
    /// </summary>
    internal static class OpenApiDataTypeKindExtensions
    {
        /// <summary>
        /// Get the Common Name.
        /// </summary>
        /// <param name="kind">The Open Api data type kind.</param>
        /// <returns>The Common Name.</returns>
        public static string GetCommonName(this OpenApiDataTypeKind kind)
        {
            OpenApiDataTypeMetadataAttribute attribute = kind.GetAttributeOfType<OpenApiDataTypeMetadataAttribute>();
            return attribute?.CommonName;
        }

        /// <summary>
        /// Get the Type.
        /// </summary>
        /// <param name="kind">The Open Api data type kind.</param>
        /// <returns>The Type.</returns>
        public static string GetTypeName(this OpenApiDataTypeKind kind)
        {
            OpenApiDataTypeMetadataAttribute attribute = kind.GetAttributeOfType<OpenApiDataTypeMetadataAttribute>();
            return attribute?.Type;
        }

        /// <summary>
        /// Get the Format.
        /// </summary>
        /// <param name="kind">The Open Api data type kind.</param>
        /// <returns>The Format.</returns>
        public static string GetFormat(this OpenApiDataTypeKind kind)
        {
            OpenApiDataTypeMetadataAttribute attribute = kind.GetAttributeOfType<OpenApiDataTypeMetadataAttribute>();
            return attribute?.Format;
        }

        /// <summary>
        /// Get the Comments.
        /// </summary>
        /// <param name="kind">The Open Api data type kind.</param>
        /// <returns>The Comments.</returns>
        public static string GetComments(this OpenApiDataTypeKind kind)
        {
            OpenApiDataTypeMetadataAttribute attribute = kind.GetAttributeOfType<OpenApiDataTypeMetadataAttribute>();
            return attribute?.Comments;
        }
    }
}
