// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;

namespace Microsoft.OpenApi.OData.Authorizations
{
    /// <summary>
    /// Complex type Org.OData.Core.V1.OpenIDConnect
    /// </summary>
    internal class OpenIDConnect : Authorization
    {
        /// <summary>
        /// Issuer location for the OpenID Provider.
        /// Configuration information can be obtained by appending `/.well-known/openid-configuration` to this Url.
        /// </summary>
        public string IssuerUrl { get; set; }

        /// <summary>
        /// Gets the security scheme type.
        /// </summary>
        public override SecuritySchemeType SchemeType => SecuritySchemeType.OpenIdConnect;

        /// <summary>
        /// Init <see cref="OpenIDConnect"/>.
        /// </summary>
        /// <param name="record">the input record.</param>
        public override void Init(IEdmRecordExpression record)
        {
            // base checked.
            base.Init(record);

            // IssuerUrl
            IssuerUrl = record.GetString("IssuerUrl");
        }
    }
}