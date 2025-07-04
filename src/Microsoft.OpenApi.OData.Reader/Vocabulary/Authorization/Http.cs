// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.Vocabulary.Authorization
{
    /// <summary>
    /// Complex type 'Org.OData.Authorization.V1.Http'
    /// </summary>
    internal class Http : Authorization
    {
        /// <summary>
        /// HTTP Authorization scheme to be used in the Authorization header, as per RFC7235.
        /// </summary>
        public string? Scheme { get; set; }

        /// <summary>
        /// Format of the bearer token.
        /// </summary>
        public string? BearerFormat { get; set; }

        /// <summary>
        /// Gets the security scheme type.
        /// </summary>
        public override SecuritySchemeType SchemeType => SecuritySchemeType.Http;

        /// <summary>
        /// Init <see cref="Http"/>.
        /// </summary>
        /// <param name="record">the input record.</param>
        public override void Initialize(IEdmRecordExpression record)
        {
            // base checked.
            base.Initialize(record);

            // Scheme
            Scheme = record.GetString("Scheme");

            // BearerFormat
            BearerFormat = record.GetString("BearerFormat");
        }
    }
}
