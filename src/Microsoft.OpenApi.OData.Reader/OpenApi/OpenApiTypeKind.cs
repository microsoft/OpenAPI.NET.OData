//---------------------------------------------------------------------
// <copyright file="OpenApiTypeKind.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

namespace Microsoft.OData.OpenAPI
{
    /// <summary>
    /// Type kind in the Open Api Specification.
    /// </summary>
    internal enum OpenApiTypeKind
    {
        /// <summary>
        /// None.
        /// </summary>
        [OpenApiDataTypeMetadata(CommonName = "none")]
        None,

        /// <summary>
        /// Array
        /// </summary>
        [OpenApiDataTypeMetadata(CommonName = "array")]
        Array,

        /// <summary>
        /// Object
        /// </summary>
        [OpenApiDataTypeMetadata(CommonName = "object")]
        Object,

        /// <summary>
        /// String
        /// </summary>
        [OpenApiDataTypeMetadata(CommonName = "string")]
        String,
    }

    /// <summary>
    /// Extension methods for Open Api Type kind.
    /// </summary>
    internal static class OpenApiTypeKindExtensions
    {
        /// <summary>
        /// Get the Common Name.
        /// </summary>
        /// <param name="kind">The Open Api type kind.</param>
        /// <returns>The Common Name.</returns>
        public static string GetDisplayName(this OpenApiTypeKind kind)
        {
            OpenApiDataTypeMetadataAttribute attribute = kind.GetAttributeOfType<OpenApiDataTypeMetadataAttribute>();
            return attribute?.CommonName;
        }
    }
}
