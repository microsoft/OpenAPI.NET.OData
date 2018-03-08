// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;

namespace Microsoft.OpenApi.OData.Authorizations
{
    /// <summary>
    /// OAuth2 type kind.
    /// </summary>
    internal enum OAuth2Type
    {
        /// <summary>
        /// ClientCredentials
        /// </summary>
        ClientCredentials,

        /// <summary>
        /// Implicit
        /// </summary>
        Implicit,

        /// <summary>
        /// Pasword
        /// </summary>
        Pasword,

        /// <summary>
        /// AuthCode
        /// </summary>
        AuthCode
    }

    /// <summary>
    /// Abstract complex type Org.OData.Core.V1.OAuthAuthorization
    /// </summary>
    internal abstract class OAuthAuthorization : Authorization
    {
        /// <summary>
        /// Available scopes.
        /// </summary>
        public IList<AuthorizationScope> Scopes { get; set; }

        /// <summary>
        /// Refresh Url
        /// </summary>
        public string RefreshUrl { get; set; }

        /// <summary>
        /// Gets the security scheme type.
        /// </summary>
        public override SecuritySchemeType SchemeType => SecuritySchemeType.OAuth2;

        /// <summary>
        /// Gets the OAuth2 type.
        /// </summary>
        public abstract OAuth2Type OAuth2Type { get; }

        /// <summary>
        /// Init <see cref="OAuthAuthorization"/>.
        /// </summary>
        /// <param name="record">the input record.</param>
        public override void Init(IEdmRecordExpression record)
        {
            // base checked.
            base.Init(record);

            // Scopes
            Scopes = record.GetCollection<AuthorizationScope>("Scopes", (s, r) => s.Init(r as IEdmRecordExpression));

            // RefreshUrl
            RefreshUrl = record.GetString("RefreshUrl");
        }
    }
}