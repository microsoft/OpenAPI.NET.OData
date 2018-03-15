// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Common;

namespace Microsoft.OpenApi.OData.Authorizations
{
    /// <summary>
    /// Complex type Org.OData.Core.V1.OAuth2ClientCredentials
    /// </summary>
    internal class OAuth2ClientCredentials : OAuthAuthorization
    {
        /// <summary>
        /// Token Url.
        /// </summary>
        public string TokenUrl { get; set; }

        /// <summary>
        /// Gets the OAuth2 type.
        /// </summary>
        public override OAuth2Type OAuth2Type => OAuth2Type.ClientCredentials;

        /// <summary>
        /// Init <see cref="OAuth2ClientCredentials"/>.
        /// </summary>
        /// <param name="record">the input record.</param>
        public override void Init(IEdmRecordExpression record)
        {
            // base checked.
            base.Init(record);

            // TokenUrl
            TokenUrl = record.GetString("TokenUrl");
        }
    }
}