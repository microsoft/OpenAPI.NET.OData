// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Vocabulary.Authorization;

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
        /// <param name="document">The Open API document.</param>
        public static void AddSecuritySchemesToDocument(this ODataContext context, OpenApiDocument document)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(document, nameof(document));

            if (context.Model == null || context.Model.EntityContainer == null || context.Model.GetAuthorizations(context.EntityContainer) is not {} authorizations)
            {
                return;
            }

            foreach (var authorization in authorizations)
            {
                if (string.IsNullOrEmpty(authorization.Name))
                {
                    continue;
                }
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

                document.AddComponent(authorization.Name, scheme);
            }
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

            if (!string.IsNullOrEmpty(openIdConnect.IssuerUrl))
                scheme.OpenIdConnectUrl = new Uri(openIdConnect.IssuerUrl);
        }

        private static void AppendOAuth2(OpenApiSecurityScheme scheme, OAuthAuthorization oAuth2)
        {
            Debug.Assert(scheme != null);
            Debug.Assert(oAuth2 != null);

            scheme.Flows = new OpenApiOAuthFlows();
            OpenApiOAuthFlow? flow = null;
            switch (oAuth2.OAuth2Type)
            {
                case OAuth2Type.AuthCode when oAuth2 is OAuth2AuthCode {AuthorizationUrl: not null, TokenUrl: not null} authCode:
                    // AuthCode
                    flow = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri(authCode.AuthorizationUrl),
                        TokenUrl = new Uri(authCode.TokenUrl)
                    };
                    scheme.Flows.AuthorizationCode = flow;
                    break;

                case OAuth2Type.Password when oAuth2 is OAuth2Password { TokenUrl: not null} password:
                    // Password
                    flow = new OpenApiOAuthFlow
                    {
                        TokenUrl = new Uri(password.TokenUrl)
                    };
                    scheme.Flows.Password = flow;
                    break;

                case OAuth2Type.Implicit when oAuth2 is OAuth2Implicit {AuthorizationUrl: not null} @implicit:
                    // Implicit
                    flow = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri(@implicit.AuthorizationUrl)
                    };
                    scheme.Flows.Implicit = flow;
                    break;

                case OAuth2Type.ClientCredentials when oAuth2 is OAuth2ClientCredentials {TokenUrl: not null} credentials:
                    // ClientCredentials
                    flow = new OpenApiOAuthFlow
                    {
                        TokenUrl = new Uri(credentials.TokenUrl)
                    };
                    scheme.Flows.ClientCredentials = flow;
                    break;
            }

            Debug.Assert(flow != null);
            if (!string.IsNullOrEmpty(oAuth2.RefreshUrl))
                flow.RefreshUrl = new Uri(oAuth2.RefreshUrl);

            if (oAuth2.Scopes != null)
            {
                flow.Scopes = oAuth2.Scopes
                                    .Where(static x => !string.IsNullOrEmpty(x.Scope) && !string.IsNullOrEmpty(x.Description))
                                    .ToDictionary(s => s.Scope!, s => s.Description!);
            }
        }
    }
}
