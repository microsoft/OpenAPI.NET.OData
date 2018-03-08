// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;

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
            if (record== null || record.DeclaredType == null)
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
                case "Org.OData.Authorization.V1.OpenIDConnect":
                    auth = new OpenIDConnect();
                    break;

                case "Org.OData.Authorization.V1.Http":
                    auth = new Http();
                    break;

                case "Org.OData.Authorization.V1.OAuthAuthorization":
                    //auth = new OAuthAuthorization();
                    break;

                case "Org.OData.Authorization.V1.OAuth2ClientCredentials":
                    auth = new OAuth2ClientCredentials();
                    break;

                case "Org.OData.Authorization.V1.OAuth2Implicit":
                    auth = new OAuth2Implicit();
                    break;

                case "Org.OData.Authorization.V1.OAuth2Password":
                    auth = new OAuth2Password();
                    break;

                case "Org.OData.Authorization.V1.OAuth2AuthCode":
                    auth = new OAuth2AuthCode();
                    break;

                case "Org.OData.Authorization.V1.ApiKey":
                    auth = new ApiKey();
                    break;

                default:
                    break;
            }

            if (auth != null)
            {
                auth.Init(record);
            }

            return auth;
        }
    }
}