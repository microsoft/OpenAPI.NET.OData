// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.OpenApi.OData.Schema
{
    internal enum SchemaType
    {
        /// <summary>
        /// Object type.
        /// </summary>
        [Metadata("object")]
        Object,

        /// <summary>
        /// Array type.
        /// </summary>
        [Metadata("array")]
        Array,

        /// <summary>
        /// Integer type.
        /// </summary>
        [Metadata("integer")]
        Integer,

        /// <summary>
        /// Boolean type.
        /// </summary>
        [Metadata("boolean")]
        Boolean,

        /// <summary>
        /// Number type.
        /// </summary>
        [Metadata("number")]
        Number,

        /// <summary>
        /// String type.
        /// </summary>
        [Metadata("string")]
        String
    }
}
