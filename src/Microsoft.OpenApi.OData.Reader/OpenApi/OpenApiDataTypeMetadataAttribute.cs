//---------------------------------------------------------------------
// <copyright file="OpenApiDataTypeMetadataAttribute.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;

namespace Microsoft.OData.OpenAPI
{
    /// <summary>
    /// Represents the Open Api Data type metadata attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    internal class OpenApiDataTypeMetadataAttribute : Attribute
    {
        /// <summary>
        /// Common Name.
        /// </summary>
        public string CommonName { get; set; }

        /// <summary>
        /// Type name.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Format name
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// Comments.
        /// </summary>
        public string Comments { get; set; }
    }
}
