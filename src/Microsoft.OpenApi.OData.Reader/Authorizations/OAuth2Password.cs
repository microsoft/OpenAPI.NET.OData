// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.Authorizations
{
    /// <summary>
    /// Complex type Org.OData.Core.V1.OAuth2Password
    /// </summary>
    internal class OAuth2Password : OAuthAuthorization
    {
        /// <summary>
        /// Token Url.
        /// </summary>
        public string TokenUrl { get; set; }

        /// <summary>
        /// Gets the OAuth2 type.
        /// </summary>
        public override OAuth2Type OAuth2Type => OAuth2Type.Pasword;

        /// <summary>
        /// Init <see cref="OAuth2Password"/>.
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