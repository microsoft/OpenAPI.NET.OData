// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Linq;
using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.Models.References;
using Microsoft.OpenApi.Models.Interfaces;

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
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(actionImport, nameof(actionImport));

            return context.CreateRequestBody(actionImport.Action);
        }

        /// <summary>
        /// Create a <see cref="OpenApiRequestBody"/> for a <see cref="IEdmAction"/>.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="action">The Edm action.</param>
        /// <returns>The created <see cref="OpenApiRequestBody"/> or null.</returns>
        public static OpenApiRequestBody CreateRequestBody(this ODataContext context, IEdmAction action)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(action, nameof(action));

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
                Type = JsonSchemaType.Object,
                Properties = new Dictionary<string, IOpenApiSchema>()
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

            requestBody.Content.Add(Constants.ApplicationJsonMediaType, new OpenApiMediaType
            {
                Schema = parametersSchema
            });

            return requestBody;
        }

        /// <summary>
        /// Create a dictionary of <see cref="OpenApiRequestBody"/> indexed by ref name.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="document">The OpenApi document to lookup references.</param>
        public static void AddRequestBodiesToDocument(this ODataContext context, OpenApiDocument document)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(document, nameof(document));

            document.AddComponent(Constants.ReferencePostRequestBodyName, CreateRefPostRequestBody());
            document.AddComponent(Constants.ReferencePutRequestBodyName, CreateRefPutRequestBody());

            // add request bodies for actions targeting multiple related paths
            foreach (IEdmAction action in context.Model.SchemaElements.OfType<IEdmAction>()
                .Where(context.Model.OperationTargetsMultiplePaths))
            {
                if (context.CreateRequestBody(action) is OpenApiRequestBody requestBody)
                    document.AddComponent($"{action.Name}RequestBody", requestBody);
            }
        }

        /// <summary>
        /// Create a <see cref="OpenApiRequestBody"/> to be reused across ref POST operations
        /// </summary>
        /// <returns>The created <see cref="OpenApiRequestBody"/></returns>
        private static OpenApiRequestBody CreateRefPostRequestBody()
        {
            var schema = new OpenApiSchemaReference(Constants.ReferenceCreateSchemaName);
            return new OpenApiRequestBody
            {
                Required = true,
                Description = "New navigation property ref value",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    {
                        Constants.ApplicationJsonMediaType, new OpenApiMediaType
                        {
                            Schema = schema
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Create a <see cref="OpenApiRequestBody"/> to be reused across ref PUT operations
        /// </summary>
        /// <returns>The created <see cref="OpenApiRequestBody"/></returns>
        private static OpenApiRequestBody CreateRefPutRequestBody()
        {
            var schema = new OpenApiSchemaReference(Constants.ReferenceUpdateSchemaName);

            return new OpenApiRequestBody
            {
                Required = true,
                Description = "New navigation property ref values",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    {
                        Constants.ApplicationJsonMediaType, new OpenApiMediaType
                        {
                            Schema = schema
                        }
                    }
                }
            };
        }
    }
}
