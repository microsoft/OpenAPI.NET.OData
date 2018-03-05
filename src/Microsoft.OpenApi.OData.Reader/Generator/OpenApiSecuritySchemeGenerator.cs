// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.OData.Properties;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;

namespace Microsoft.OpenApi.OData.Generator
{
    /// <summary>
    /// Extension methods to create <see cref="OpenApiSecurityScheme"/> by <see cref="IEdmModel"/>.
    /// </summary>
    internal static class OpenApiSecuritySchemeGenerator
    {
        /// <summary>
        /// Create the dictionary of <see cref="OpenApiSchema"/> object.
        /// The name of each pair is the namespace-qualified name of the type. It uses the namespace instead of the alias.
        /// The value of each pair is a <see cref="OpenApiSchema"/>.
        /// </summary>
        /// <param name="context">The OData to Open API context.</param>
        /// <returns>The string/schema dictionary.</returns>
        public static IDictionary<string, OpenApiSecurityScheme> CreateSecuritySchemes(this ODataContext context)
        {
            Utils.CheckArgumentNull(context, nameof(context));

            IDictionary<string, OpenApiSecurityScheme> securitySchemes = new Dictionary<string, OpenApiSecurityScheme>();

            if (context.Model.EntityContainer == null)
            {
                return null;
            }

            return securitySchemes;
        }
    }
}
