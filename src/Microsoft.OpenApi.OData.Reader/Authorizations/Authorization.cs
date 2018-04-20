// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.Exceptions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Properties;

namespace Microsoft.OpenApi.OData.Authorizations
{
    /// <summary>
    /// Abstract complex type Org.OData.Core.V1.Authorization
    /// </summary>
    internal abstract class Authorization
    {
        /// <summary>
        /// Name that can be used to reference the authorization flow.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the authorization method.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets the security scheme type.
        /// </summary>
        public abstract SecuritySchemeType SchemeType { get; }

        /// <summary>
        /// Init the <see cref="Authorization"/>.
        /// </summary>
        /// <param name="record">The corresponding record.</param>
        public virtual void Init(IEdmRecordExpression record)
        {
            Utils.CheckArgumentNull(record, nameof(record));

            // Name.
            Name = record.GetString("Name");

            // Description.
            Description = record.GetString("Description");
        }

        /// <summary>
        /// Create the corresponding Authorization object.
        /// </summary>
        /// <param name="record">The input record.</param>
        /// <returns>The created <see cref="Authorization"/> object.</returns>
        public static Authorization CreateAuthorization(IEdmRecordExpression record)
        {
            if (record == null || record.DeclaredType == null)
            {
                return null;
            }

            IEdmComplexType complexType = record.DeclaredType.Definition as IEdmComplexType;
            if (complexType == null)
            {
                return null;
            }

            Authorization auth = null;
            switch (complexType.FullTypeName())
            {
                case AuthorizationConstants.OpenIDConnect: // OpenIDConnect
                    auth = new OpenIDConnect();
                    break;

                case AuthorizationConstants.Http: // Http
                    auth = new Http();
                    break;

                case AuthorizationConstants.ApiKey: // ApiKey
                    auth = new ApiKey();
                    break;

                case AuthorizationConstants.OAuth2ClientCredentials: // OAuth2ClientCredentials
                    auth = new OAuth2ClientCredentials();
                    break;

                case AuthorizationConstants.OAuth2Implicit: // OAuth2Implicit
                    auth = new OAuth2Implicit();
                    break;

                case AuthorizationConstants.OAuth2Password: // OAuth2Password
                    auth = new OAuth2Password();
                    break;

                case AuthorizationConstants.OAuth2AuthCode: // OAuth2AuthCode
                    auth = new OAuth2AuthCode();
                    break;

                case AuthorizationConstants.OAuthAuthorization: // OAuthAuthorization
                default:
                    throw new OpenApiException(String.Format(SRResource.AuthorizationRecordTypeNameNotCorrect, complexType.FullTypeName()));
            }

            if (auth != null)
            {
                auth.Init(record);
            }

            return auth;
        }
    }
}