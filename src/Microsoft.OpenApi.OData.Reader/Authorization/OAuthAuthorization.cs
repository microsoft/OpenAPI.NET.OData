// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.OpenApi.OData.Authorization
{
    /// <summary>
    /// Constant values for Authorization Vocabulary
    /// </summary>
    internal abstract class OAuthAuthorization : Authorization
    {
        public IList<AuthorizationScope> Scopes { get; set; }

        public string RefreshUrl { get; set; }
    }

    internal class OAuth2ClientCredentials : OAuthAuthorization
    {
        public string TokenUrl { get; set; }
    }

    internal class OAuth2Implicit : OAuthAuthorization
    {
        public string AuthorizationUrl { get; set; }
    }

    internal class OAuth2Password : OAuthAuthorization
    {
        public string TokenUrl { get; set; }
    }

    internal class OAuth2AuthCode : OAuthAuthorization
    {
        public string AuthorizationUrl { get; set; }

        public string TokenUrl { get; set; }
    }
}