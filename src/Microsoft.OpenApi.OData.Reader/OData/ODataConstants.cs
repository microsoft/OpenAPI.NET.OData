// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    }
}
