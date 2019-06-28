// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.Vocabulary.Authorization
{
    /// <summary>
    /// Complex type: Org.OData.Authorization.V1.SecurityScheme
    /// </summary>
    [Term("Org.OData.Authorization.V1.SecuritySchemes")]
    internal class SecurityScheme : IRecord
    {
        /// <summary>
        /// The name of a required authorization scheme.
        /// </summary>
        public string Authorization { get; set; }

        /// <summary>
        /// The names of scopes required from this authorization scheme.
        /// </summary>
        public IList<string> RequiredScopes { get; set; }

        /// <summary>
        /// Init the <see cref="SecurityScheme"/>.
        /// </summary>
        /// <param name="record">The input record.</param>
        public void Initialize(IEdmRecordExpression record)
        {
            Utils.CheckArgumentNull(record, nameof(record));

            // AuthorizationSchemeName
            Authorization = record.GetString("Authorization");

            // RequiredScopes
            RequiredScopes = record.GetCollection("RequiredScopes");
        }
    }
}