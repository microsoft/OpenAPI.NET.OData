// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;

namespace Microsoft.OpenApi.OData.Generator
{
    /// <summary>
    /// Extension methods to create <see cref="OpenApiServer"/> by <see cref="IEdmModel"/>.
    /// </summary>
    internal static class OpenApiServerGenerator
    {
        /// <summary>
        /// Create the collection of <see cref="OpenApiServer"/> object.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="settings">The convert settings.</param>
        /// <returns>The created collection of <see cref="OpenApiServer"/> object.</returns>
        public static IList<OpenApiServer> CreateServers(this IEdmModel model, OpenApiConvertSettings settings)
        {
            if (model == null)
            {
                throw Error.ArgumentNull(nameof(model));
            }

            if (settings == null)
            {
                throw Error.ArgumentNull(nameof(settings));
            }

            // The value of servers is an array of Server Objects.
            // It contains one object with a field url.
            // The value of url is a string containing the service root URL without the trailing forward slash.
            return new List<OpenApiServer>
            {
                new OpenApiServer
                {
                    Url = settings.ServiceRoot?.OriginalString
                }
            };
        }
    }
}
