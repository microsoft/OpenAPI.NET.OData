// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Authorizations;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.Generator
{
    /// <summary>
    /// Extension methods to create <see cref="OpenApiSecurityScheme"/> by <see cref="ODataContext"/>.
    /// </summary>
    internal static class OpenApiSecuritySchemeGenerator
    {
        /// <summary>
        /// Create the dictionary of <see cref="OpenApiSecurityScheme"/> object.
        /// The name of each pair is the name of authorization. The value of each pair is a <see cref="OpenApiSecurityScheme"/>.
        /// </summary>
        /// <param name="context">The OData to Open API context.</param>
        /// <returns>The string/security scheme dictionary.</returns>
        public static IDictionary<string, OpenApiSecurityScheme> CreateSecuritySchemes(this ODataContext context)
        {
            Utils.CheckArgumentNull(context, nameof(context));

            if (context.Model == null || context.Model.EntityContainer == null)
            {
                return null;
            }

            IDictionary<string, OpenApiSecurityScheme> securitySchemes = new Dictionary<string, OpenApiSecurityScheme>();
            var authorizations = context.GetAuthorizations(context.EntityContainer);
            foreach (var authorization in authorizations)
            {
                OpenApiSecurityScheme scheme = new OpenApiSecurityScheme
                {
                    Type = authorization.SchemeType,
                    Description = authorization.Description
                };

                switch (authorization.SchemeType)
                {
                    case SecuritySchemeType.ApiKey: // ApiKey
                        AppendApiKey(scheme, (ApiKey)authorization);
                        break;

                    case SecuritySchemeType.Http: // Http
                        AppendHttp(scheme, (Http)authorization);
                        break;

                    case SecuritySchemeType.OpenIdConnect: // OpenIdConnect
                        AppendOpenIdConnect(scheme, (OpenIDConnect)authorization);
                        break;

                    case SecuritySchemeType.OAuth2: // OAuth2
                        AppendOAuth2(scheme, (OAuthAuthorization)authorization);
                        break;
                }

                securitySchemes[authorization.Name] = scheme;
            }

            return securitySchemes;
        }

        private static void AppendApiKey(OpenApiSecurityScheme scheme, ApiKey apiKey)
        {
            Debug.Assert(scheme != null);
            Debug.Assert(apiKey != null);

            scheme.Name = apiKey.KeyName;

            Debug.Assert(apiKey.Location != null);
            switch(apiKey.Location.Value)
            {
                case KeyLocation.Cookie:
                    scheme.In = ParameterLocation.Cookie;
                    break;

                case KeyLocation.Header:
                    scheme.In = ParameterLocation.Header;
                    break;

                case KeyLocation.QueryOption:
                    scheme.In = ParameterLocation.Query;
                    break;
            }
        }

        private static void AppendHttp(OpenApiSecurityScheme scheme, Http http)
        {
            Debug.Assert(scheme != null);
            Debug.Assert(http != null);

            scheme.Scheme = http.Scheme;
            scheme.BearerFormat = http.BearerFormat;
        }

        private static void AppendOpenIdConnect(OpenApiSecurityScheme scheme, OpenIDConnect openIdConnect)
        {
            Debug.Assert(scheme != null);
            Debug.Assert(openIdConnect != null);

            scheme.OpenIdConnectUrl = new Uri(openIdConnect.IssuerUrl);
        }

        private static void AppendOAuth2(OpenApiSecurityScheme scheme, OAuthAuthorization oAuth2)
        {
            Debug.Assert(scheme != null);
            Debug.Assert(oAuth2 != null);

            scheme.Flows = new OpenApiOAuthFlows();
            OpenApiOAuthFlow flow = null;
            switch (oAuth2.OAuth2Type)
            {
                case OAuth2Type.AuthCode: // AuthCode
                    OAuth2AuthCode authCode = (OAuth2AuthCode)oAuth2;
                    flow = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri(authCode.AuthorizationUrl),
                        TokenUrl = new Uri(authCode.TokenUrl)
                    };
                    scheme.Flows.AuthorizationCode = flow;
                    break;

                case OAuth2Type.Pasword: // Password
                    OAuth2Password password = (OAuth2Password)oAuth2;
                    flow = new OpenApiOAuthFlow
                    {
                        TokenUrl = new Uri(password.TokenUrl)
                    };
                    scheme.Flows.Password = flow;
                    break;

                case OAuth2Type.Implicit: // Implicit
                    OAuth2Implicit @implicit = (OAuth2Implicit)oAuth2;
                    flow = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri(@implicit.AuthorizationUrl)
                    };
                    scheme.Flows.Implicit = flow;
                    break;

                case OAuth2Type.ClientCredentials: // ClientCredentials
                    OAuth2ClientCredentials credentials = (OAuth2ClientCredentials)oAuth2;
                    flow = new OpenApiOAuthFlow
                    {
                        TokenUrl = new Uri(credentials.TokenUrl)
                    };
                    scheme.Flows.ClientCredentials = flow;
                    break;
            }

            Debug.Assert(flow != null);
            flow.RefreshUrl = new Uri(oAuth2.RefreshUrl);

            if (oAuth2.Scopes != null)
            {
                flow.Scopes = oAuth2.Scopes.ToDictionary(s => s.Scope, s => s.Description);
            }
        }
    }
}
