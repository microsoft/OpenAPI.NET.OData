// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OpenApi.Models;

namespace Microsoft.OpenApi.OData.Generator
{
    /// <summary>
    /// Extension methods to create <see cref="OpenApiServer"/> by Edm model.
    /// </summary>
    internal static class OpenApiServerGenerator
    {
        /// <summary>
        /// Create the collection of <see cref="OpenApiServer"/> object.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <returns>The created collection of <see cref="OpenApiServer"/> object.</returns>
        public static IList<OpenApiServer> CreateServers(this ODataContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            // The value of servers is an array of Server Objects.
            // It contains one object with a field url.
            // The value of url is a string containing the service root URL without the trailing forward slash.
            return new List<OpenApiServer>
            {
                new OpenApiServer
                {
                    Url = context.Settings.ServiceRoot?.OriginalString
                }
            };
        }
    }
}
