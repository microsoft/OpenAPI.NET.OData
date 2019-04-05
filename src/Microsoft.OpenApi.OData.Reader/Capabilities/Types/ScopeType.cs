// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OpenApi.OData.Common;

namespace Microsoft.OpenApi.OData.Capabilities
{
    /// <summary>
    /// Complex type: Org.OData.Capabilities.V1.ScopeType
    /// </summary>
    internal class ScopeType
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ScopeType"/> class.
        /// </summary>
        /// <param name="scope">The non-nullable scope name.</param>
        /// <param name="restrictedProperties">
        /// The non-nullable of the comma-separated string value of all properties
        /// that will be included or excluded when using the scope
        /// </param>
        public ScopeType(string scope, string restrictedProperties)
        {
            Scope = scope ?? throw Error.ArgumentNull(nameof(scope));
            RestrictedProperties = restrictedProperties ?? throw Error.ArgumentNull(nameof(scope));
        }

        /// <summary>
        /// Gets the names of the scope.
        /// </summary>
        public string Scope { get; private set; }

        /// <summary>
        /// Gets the restricted properties.
        /// Comma-separated string value of all properties that will be included or excluded when using the scope.
        /// Possible string value identifiers when specifying properties are '*', _PropertyName_, '-'_PropertyName_.
        /// </summary>
        public string RestrictedProperties { get; private set; }
    }
}
