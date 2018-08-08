// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.OpenApi.OData.Authorizations
{
    /// <summary>
    /// Constant values for Authorization Vocabulary
    /// </summary>
    internal class AuthorizationConstants
    {
        /// <summary>
        /// The namespace of Authorization annotation.
        /// </summary>
        public const string Namespace = "Org.OData.Authorization.V1";

        /// <summary>
        /// Term Org.OData.Authorization.V1.Authorizations
        /// </summary>
        public const string Authorizations = Namespace + ".Authorizations";

        /// <summary>
        /// Term Org.OData.Authorization.V1.SecuritySchemes
        /// </summary>
        public const string SecuritySchemes = Namespace + ".SecuritySchemes";

        /// <summary>
        /// Complex type: Org.OData.Authorization.V1.OpenIDConnect
        /// </summary>
        public const string OpenIDConnect = Namespace + ".OpenIDConnect";

        /// <summary>
        /// Complex type: Org.OData.Authorization.V1.Http
        /// </summary>
        public const string Http = Namespace + ".Http";

        /// <summary>
        /// Complex type: Org.OData.Authorization.V1.ApiKey
        /// </summary>
        public const string ApiKey = Namespace + ".ApiKey";

        /// <summary>
        /// Complex type: Org.OData.Authorization.V1.OAuth2ClientCredentials
        /// </summary>
        public const string OAuth2ClientCredentials = Namespace + ".OAuth2ClientCredentials";

        /// <summary>
        /// Complex type: Org.OData.Authorization.V1.OAuth2Implicit
        /// </summary>
        public const string OAuth2Implicit = Namespace + ".OAuth2Implicit";

        /// <summary>
        /// Complex type: Org.OData.Authorization.V1.OAuth2Password
        /// </summary>
        public const string OAuth2Password = Namespace + ".OAuth2Password";

        /// <summary>
        /// Complex type: Org.OData.Authorization.V1.OAuth2AuthCode
        /// </summary>
        public const string OAuth2AuthCode = Namespace + ".OAuth2AuthCode";

        /// <summary>
        /// Complex type: Org.OData.Authorization.V1.OAuthAuthorization
        /// </summary>
        public const string OAuthAuthorization = Namespace + ".OAuthAuthorization";
    }
}