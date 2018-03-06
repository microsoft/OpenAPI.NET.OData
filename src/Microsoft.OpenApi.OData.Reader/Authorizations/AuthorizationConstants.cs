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
    }
}