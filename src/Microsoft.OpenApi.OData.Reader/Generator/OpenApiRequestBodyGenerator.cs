// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Linq;
using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;

namespace Microsoft.OpenApi.OData.Generator
{
    /// <summary>
    /// Extension methods to create <see cref="OpenApiRequestBody"/> for Edm elements.
    /// </summary>
    internal static class OpenApiRequestBodyGenerator
    {
        /// <summary>
        /// Create a <see cref="OpenApiRequestBody"/> for a <see cref="IEdmActionImport"/>.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="actionImport">The Edm action import.</param>
        /// <returns>The created <see cref="OpenApiRequestBody"/> or null.</returns>
        public static OpenApiRequestBody CreateRequestBody(this ODataContext context, IEdmActionImport actionImport)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            if (actionImport == null)
            {
                throw Error.ArgumentNull(nameof(actionImport));
            }

            return context.CreateRequestBody(actionImport.Action);
        }

        /// <summary>
        /// Create a <see cref="OpenApiRequestBody"/> for a <see cref="IEdmAction"/>.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="actionImport">The Edm action.</param>
        /// <returns>The created <see cref="OpenApiRequestBody"/> or null.</returns>
        public static OpenApiRequestBody CreateRequestBody(this ODataContext context, IEdmAction action)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            if (action == null)
            {
                throw Error.ArgumentNull(nameof(action));
            }

            // return null for empty action parameters
            int skip = 0;
            if (action.IsBound)
            {
                if (action.Parameters.Count() <= 1)
                {
                    return null;
                }
                skip = 1; //skip the binding parameter
            }
            else
            {
                if (!action.Parameters.Any())
                {
                    return null;
                }
            }

            OpenApiSchema parametersSchema = new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>()
            };

            foreach (var parameter in action.Parameters.Skip(skip))
            {
                parametersSchema.Properties.Add(parameter.Name, context.CreateEdmTypeSchema(parameter.Type));
            }

            OpenApiRequestBody requestBody = new OpenApiRequestBody
            {
                Description = "Action parameters",
                Required = true,
                Content = new Dictionary<string, OpenApiMediaType>()
            };

            requestBody.Content.Add("application/json", new OpenApiMediaType
            {
                Schema = parametersSchema
            });

            return requestBody;
        }
    }
}
