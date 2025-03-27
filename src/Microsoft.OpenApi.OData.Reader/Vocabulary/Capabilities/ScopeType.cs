// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.Vocabulary.Capabilities
{
    /// <summary>
    /// Complex type: Org.OData.Capabilities.V1.ScopeType
    /// </summary>
    internal class ScopeType : IRecord
    {
        /// <summary>
        /// Gets the names of the scope.
        /// </summary>
        public string? Scope { get; private set; }

        /// <summary>
        /// Gets the restricted properties.
        /// Comma-separated string value of all properties that will be included or excluded when using the scope.
        /// Possible string value identifiers when specifying properties are '*', _PropertyName_, '-'_PropertyName_.
        /// </summary>
        public string? RestrictedProperties { get; private set; }

        /// <summary>
        /// Init the <see cref="ScopeType"/>.
        /// </summary>
        /// <param name="record">The input record.</param>
        public void Initialize(IEdmRecordExpression record)
        {
            Utils.CheckArgumentNull(record, nameof(record));

            // Scope
            Scope = record.GetString("Scope");

            // RestrictedProperties
            RestrictedProperties = record.GetString("RestrictedProperties");
        }
    }
}
