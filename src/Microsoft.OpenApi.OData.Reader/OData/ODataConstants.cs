// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using System.Collections.Generic;

namespace Microsoft.OpenApi.OData
{
    internal static class ODataConstants
    {
        /// <summary>
        /// Namespaces used in standard included models.
        /// </summary>
        public static IList<string> StandardNamespaces = new List<string>
        {
            "Org.OData.",
            "Edm",
            "OData.Community.",
        };

        /// <summary>
        /// @odata.nextLink KeyValue pair
        /// </summary>
        public static KeyValuePair<string, OpenApiSchema> OdataNextLink = new("@odata.nextLink", new OpenApiSchema { Type = Constants.StringType, Nullable = true });

        /// <summary>
        /// @odata.count KeyValue pair
        /// </summary>
        public static KeyValuePair<string, OpenApiSchema> OdataCount = new("@odata.count", new OpenApiSchema { Type = "integer", Format = "int64", Nullable = true });

        /// <summary>
        /// @odata.deltaLink KeyValue pair
        /// </summary>
        public static KeyValuePair<string, OpenApiSchema> OdataDeltaLink = new("@odata.deltaLink", new OpenApiSchema { Type = Constants.StringType, Nullable = true });
    }
}
