// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Linq;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Vocabulary.Authorization;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;
using Microsoft.OpenApi.Models.References;

namespace Microsoft.OpenApi.OData.Generator
{
    /// <summary>
    /// Extension methods to create <see cref="OpenApiSecurityRequirement"/> by <see cref="ODataContext"/>.
    /// </summary>
    internal static class OpenApiSecurityRequirementGenerator
    {
        /// <summary>
        /// Create the list of <see cref="OpenApiSecurityRequirement"/> object.
        /// </summary>
        /// <param name="context">The OData to Open API context.</param>
        /// <param name="permissions">The permissions.</param>
        /// <param name="document">The Open API document to use for references lookup.</param>
        /// <returns>The created <see cref="OpenApiSecurityRequirement"/> collection.</returns>
        public static IEnumerable<OpenApiSecurityRequirement> CreateSecurityRequirements(this ODataContext context,
            IList<PermissionType> permissions, OpenApiDocument document)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(document, nameof(document));

            if (permissions != null)
            {
                foreach (PermissionType permission in permissions)
                {
                    yield return new OpenApiSecurityRequirement
                    {
                        [
                            new OpenApiSecuritySchemeReference(permission.SchemeName, document)
                        ] = new List<string>(permission.Scopes?.Select(c => c.Scope) ?? new List<string>())
                    };
                }
            }
        }
    }
}
