// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.Authorizations
{
    /// <summary>
    /// Complex type 'Org.OData.Core.V1.AuthorizationScope'
    /// </summary>
    internal class AuthorizationScope
    {
        /// <summary>
        /// Scope name.
        /// </summary>
        public string Scope { get; set; }

        /// <summary>
        /// Scope Grant.
        /// Identity that has access to the scope or can grant access to the scope.
        /// </summary>
        public string Grant { get; set; }

        /// <summary>
        /// Description of the scope.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Init the <see cref="AuthorizationScope"/>.
        /// </summary>
        /// <param name="record">The corresponding record.</param>
        public virtual void Init(IEdmRecordExpression record)
        {
            Utils.CheckArgumentNull(record, nameof(record));

            // Scope.
            Scope = record.GetString("Scope");

            // Grant.
            Grant = record.GetString("Grant");

            // Description.
            Description = record.GetString("Description");
        }
    }
}
