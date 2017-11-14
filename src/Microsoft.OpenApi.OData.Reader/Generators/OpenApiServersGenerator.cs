// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Commons;

namespace Microsoft.OpenApi.OData.Generators
{
    /// <summary>
    /// Extension methods to create <see cref="OpenApiServer"/> by <see cref="IEdmModel"/>.
    /// </summary>
    internal static class OpenApiServersGenerator
    {
        /// <summary>
        /// Create the collection of <see cref="OpenApiServer"/> object.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <returns>The servers object.</returns>
        public static IList<OpenApiServer> CreateServers(this IEdmModel model)
        {
            if (model == null)
            {
                throw Error.ArgumentNull(nameof(model));
            }

            // The value of servers is an array of Server Objects.
            // It contains one object with a field url.
            // The value of url is a string cotaining the service root URL without the trailing forward slash.
            return new List<OpenApiServer>
            {
                new OpenApiServer
                {
                    Url = "http://localhost"
                }
            };
        }
    }
}
