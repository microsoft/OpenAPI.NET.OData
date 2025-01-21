// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Generator;
using System.Collections.Generic;

namespace Microsoft.OpenApi.OData.Common;

/// <summary>
/// Extensions methods for the OpenApiOperation class.
/// </summary>
public static class OpenApiOperationExtensions
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
    public static void AddErrorResponses(this OpenApiOperation operation, OpenApiConvertSettings settings, OpenApiDocument document, bool addNoContent = false, OpenApiSchema schema = null)
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
                OpenApiResponse response = null;
                if (schema != null)
                {
                    response = new()
                    {
                        Description = Constants.Success,
                        Content = new Dictionary<string, OpenApiMediaType>
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
                operation.Responses.Add(Constants.StatusCodeClass2XX, response ?? Constants.StatusCodeClass2XX.GetResponse(document));
            }
            else
            {
                operation.Responses.Add(Constants.StatusCode204, Constants.StatusCode204.GetResponse(document));
            }
        }

        if (settings.ErrorResponsesAsDefault)
        {
            operation.Responses.Add(Constants.StatusCodeDefault, Constants.StatusCodeDefault.GetResponse(document));
        }
        else
        {
			operation.Responses.Add(Constants.StatusCodeClass4XX, Constants.StatusCodeClass4XX.GetResponse(document));
			operation.Responses.Add(Constants.StatusCodeClass5XX, Constants.StatusCodeClass5XX.GetResponse(document));
        }
    }
}