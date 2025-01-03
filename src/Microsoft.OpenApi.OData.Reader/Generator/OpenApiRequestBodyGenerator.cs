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
        /// <param name="document">The OpenApi document to lookup references.</param>
        /// <returns>The created <see cref="OpenApiRequestBody"/> or null.</returns>
        public static OpenApiRequestBody CreateRequestBody(this ODataContext context, IEdmActionImport actionImport, OpenApiDocument document)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(actionImport, nameof(actionImport));
            Utils.CheckArgumentNull(document, nameof(document));

            return context.CreateRequestBody(actionImport.Action, document);
        }

        /// <summary>
        /// Create a <see cref="OpenApiRequestBody"/> for a <see cref="IEdmAction"/>.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="action">The Edm action.</param>
        /// <param name="document">The OpenApi document to lookup references.</param>
        /// <returns>The created <see cref="OpenApiRequestBody"/> or null.</returns>
        public static OpenApiRequestBody CreateRequestBody(this ODataContext context, IEdmAction action, OpenApiDocument document)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(action, nameof(action));
            Utils.CheckArgumentNull(document, nameof(document));

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
                Properties = new Dictionary<string, OpenApiSchema>()
            };

            foreach (var parameter in action.Parameters.Skip(skip))
            {
                parametersSchema.Properties.Add(parameter.Name, context.CreateEdmTypeSchema(parameter.Type, document));
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
        /// <returns>The created dictionary of <see cref="OpenApiRequestBody"/> indexed by ref name</returns>
        public static IDictionary<string, OpenApiRequestBody> CreateRequestBodies(this ODataContext context, OpenApiDocument document)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(document, nameof(document));

            Dictionary<string, OpenApiRequestBody> requestBodies = new()
            {
                {
                    Constants.ReferencePostRequestBodyName,
                    CreateRefPostRequestBody(document)
                },
                {
                    Constants.ReferencePutRequestBodyName,
                    CreateRefPutRequestBody(document)
                }
            };

            // add request bodies for actions targeting multiple related paths
            foreach (IEdmAction action in context.Model.SchemaElements.OfType<IEdmAction>()
                .Where(action => context.Model.OperationTargetsMultiplePaths(action)))
            {
                OpenApiRequestBody requestBody = context.CreateRequestBody(action, document);
                if (requestBody != null)
                    requestBodies.Add($"{action.Name}RequestBody", requestBody);
            }
      
            return requestBodies;
        }

        /// <summary>
        /// Create a <see cref="OpenApiRequestBody"/> to be reused across ref POST operations
        /// </summary>
        /// <returns>The created <see cref="OpenApiRequestBody"/></returns>
        /// <param name="document">The OpenApi document to lookup references.</param>
        private static OpenApiRequestBody CreateRefPostRequestBody(OpenApiDocument document)
        {
            OpenApiSchema schema = new OpenApiSchemaReference(Constants.ReferenceCreateSchemaName, document);
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
        /// <param name="document">The OpenApi document to lookup references.</param>
        private static OpenApiRequestBody CreateRefPutRequestBody(OpenApiDocument document)
        {
            OpenApiSchema schema = new OpenApiSchemaReference(Constants.ReferenceUpdateSchemaName, document);

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
