// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OpenApi.OData.Generator;
using System.Collections.Generic;

namespace Microsoft.OpenApi.OData.Common;

/// <summary>
/// Extensions methods for the OpenApiOperation class.
/// </summary>
internal static class OpenApiOperationExtensions
{
    /// <summary>
    /// Adds a default response to the operation or 4XX/5XX responses for the errors depending on the settings.
    /// Also adds a 204 no content response when requested.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="settings">The settings.</param>
    /// <param name="addNoContent">Optional: Whether to add a 204 no content response.</param>
    /// <param name="schema">Optional: The OpenAPI schema of the response.</param>
    /// <param name="document">The OpenAPI document to lookup references.</param>
    public static void AddErrorResponses(this OpenApiOperation operation, OpenApiConvertSettings settings, OpenApiDocument document, bool addNoContent = false, IOpenApiSchema? schema = null)
    {
        Utils.CheckArgumentNull(operation, nameof(operation));
        Utils.CheckArgumentNull(settings, nameof(settings));
        Utils.CheckArgumentNull(document, nameof(document));
        
		if (operation.Responses == null)
		{
			operation.Responses = new();
		}

        if (addNoContent)
        {
            if (settings.UseSuccessStatusCodeRange)
            {
                OpenApiResponse? response = null;
                if (schema != null)
                {
                    response = new()
                    {
                        Description = Constants.Success,
                        Content = new Dictionary<string, IOpenApiMediaType>
                        {
                            {
                                Constants.ApplicationJsonMediaType,
                                new OpenApiMediaType
                                {
                                    Schema = schema
                                }
                            }
                        }
                    };
                }
                if ((response ?? Constants.StatusCodeClass2XX.GetResponse(document)) is {} x2xxResponse)
                    operation.Responses.Add(Constants.StatusCodeClass2XX, x2xxResponse);
            }
            else if (Constants.StatusCode204.GetResponse(document) is {} x204Response)
            {
                operation.Responses.Add(Constants.StatusCode204, x204Response);
            }
        }

        if (settings.ErrorResponsesAsDefault && Constants.StatusCodeDefault.GetResponse(document) is {} defaultResponse)
        {
            operation.Responses.Add(Constants.StatusCodeDefault, defaultResponse);
        }
        else
        {
            if (Constants.StatusCodeClass4XX.GetResponse(document) is {} x4xxResponse)
			    operation.Responses.Add(Constants.StatusCodeClass4XX, x4xxResponse);
            if (Constants.StatusCodeClass5XX.GetResponse(document) is {} x5xxResponse)
			    operation.Responses.Add(Constants.StatusCodeClass5XX, x5xxResponse);
        }
    }
}