// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.Vocabulary.Capabilities
{
    /// <summary>
    /// Complex type: Org.OData.Capabilities.V1.PermissionType
    /// </summary>
    internal class PermissionType : IRecord
    {
        /// <summary>
        /// Gets the Authorization flow scheme name.
        /// </summary>
        public string? SchemeName { get; private set; }

        /// <summary>
        /// Gets the list of scopes that can provide access to the resource.
        /// </summary>
        public IList<ScopeType>? Scopes { get; private set; }

        /// <summary>
        /// Init the <see cref="PermissionType"/>.
        /// </summary>
        /// <param name="record">The input record.</param>
        public void Initialize(IEdmRecordExpression record)
        {
            Utils.CheckArgumentNull(record, nameof(record));

            // SchemeName
            SchemeName = record.GetString("SchemeName");

            // Scopes
            Scopes = record.GetCollection<ScopeType>("Scopes");
        }
    }
}
