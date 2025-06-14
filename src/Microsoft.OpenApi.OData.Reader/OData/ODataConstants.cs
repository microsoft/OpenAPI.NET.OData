﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.OpenApi.OData
{
    internal static class ODataConstants
    {
        /// <summary>
        /// Namespaces used in standard included models.
        /// </summary>
        public readonly static List<string> StandardNamespaces =
        [
            "Org.OData.",
            "Edm",
            "OData.Community.",
        ];

        /// <summary>
        /// @odata.nextLink KeyValue pair
        /// </summary>
        public readonly static KeyValuePair<string, IOpenApiSchema> OdataNextLink = new("@odata.nextLink", new OpenApiSchema { Type = JsonSchemaType.String | JsonSchemaType.Null });

        /// <summary>
        /// @odata.count KeyValue pair
        /// </summary>
        public readonly static KeyValuePair<string, IOpenApiSchema> OdataCount = new("@odata.count", new OpenApiSchema { Type = JsonSchemaType.Number | JsonSchemaType.Null, Format = "int64"});

        /// <summary>
        /// @odata.deltaLink KeyValue pair
        /// </summary>
        public readonly static KeyValuePair<string, IOpenApiSchema> OdataDeltaLink = new("@odata.deltaLink", new OpenApiSchema { Type = JsonSchemaType.String | JsonSchemaType.Null });
    }
}
