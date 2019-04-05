// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OpenApi.OData.Authorizations;
using Microsoft.OpenApi.OData.Common;

namespace Microsoft.OpenApi.OData.Capabilities
{
    /// <summary>
    /// Complex type: Org.OData.Capabilities.V1.PermissionType
    /// </summary>
    internal class PermissionType
    {
        /// <summary>
        /// Initializes a new instance of <see cref="PermissionType"/> class.
        /// </summary>
        /// <param name="scheme">The Auth flow scheme name.</param>
        /// <param name="scopes">List of scopes that can provide access to the resource.</param>
        public PermissionType(SecurityScheme scheme, IEnumerable<ScopeType> scopes)
        {
            Scheme = scheme ?? throw Error.ArgumentNull(nameof(scheme));
            Scopes = scopes ?? throw Error.ArgumentNull(nameof(scopes));
        }

        /// <summary>
        /// Gets the auth flow scheme name.
        /// </summary>
        public SecurityScheme Scheme { get; private set; }

        /// <summary>
        /// Gets the list of scopes that can provide access to the resource.
        /// </summary>
        public IEnumerable<ScopeType> Scopes { get; private set; }
    }
}
