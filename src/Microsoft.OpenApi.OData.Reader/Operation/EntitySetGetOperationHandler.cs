// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Generator;

namespace Microsoft.OpenApi.OData.Operation
{
    /// <summary>
    /// Query a Collection of Entities, it's a "Get" operation for <see cref="IEdmEntitySet"/>
    /// The Path Item Object for the entity set contains the keyword get with an Operation Object as value
    /// that describes the capabilities for querying the entity set.
    /// For example: "/users"
    /// </summary>
    internal class EntitySetGetOperationHandler : EntitySetOperationHandler
    {
        /// <inheritdoc/>
        public override OperationType OperationType => OperationType.Get;

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            // Summary
            operation.Summary = "Get entities from " + EntitySet.Name;
            var request = Context.FindRequest(EntitySet, OperationType.ToString());
            if (request != null && request.Description != null)
            {
                operation.Summary = request.Description;
            }

            // OperationId
            if (Context.Settings.EnableOperationId)
            {
                string typeName = EntitySet.EntityType().Name;
                operation.OperationId = EntitySet.Name + "." + typeName + ".List" + Utils.UpperFirstChar(typeName);
            }
        }

        /// <inheritdoc/>
        protected override void SetParameters(OpenApiOperation operation)
        {
            base.SetParameters(operation);

            // The parameters array contains Parameter Objects for all system query options allowed for this collection,
            // and it does not list system query options not allowed for this collection, see terms
            // Capabilities.TopSupported, Capabilities.SkipSupported, Capabilities.SearchRestrictions,
            // Capabilities.FilterRestrictions, and Capabilities.CountRestrictions
            // $top
            OpenApiParameter parameter = Context.CreateTop(EntitySet);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }

            // $skip
            parameter = Context.CreateSkip(EntitySet);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }

            // $search
            parameter = Context.CreateSearch(EntitySet);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }

            // $filter
            parameter = Context.CreateFilter(EntitySet);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }

            // $count
            parameter = Context.CreateCount(EntitySet);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }

            // the syntax of the system query options $expand, $select, and $orderby is too flexible
            // to be formally described with OpenAPI Specification means, yet the typical use cases
            // of just providing a comma-separated list of properties can be expressed via an array-valued
            // parameter with an enum constraint
            // $order
            parameter = Context.CreateOrderBy(EntitySet);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }

            // $select
            parameter = Context.CreateSelect(EntitySet);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }

            // $expand
            parameter = Context.CreateExpand(EntitySet);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }
        }

        /// <inheritdoc/>
        protected override void SetResponses(OpenApiOperation operation)
        {
            operation.Responses = new OpenApiResponses
            {
                {
                    Constants.StatusCode200,
                    new OpenApiResponse
                    {
                        Description = "Retrieved entities",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            {
                                Constants.ApplicationJsonMediaType,
                                new OpenApiMediaType
                                {
                                    Schema = new OpenApiSchema
                                    {
                                        Title = "Collection of " + EntitySet.EntityType().Name,
                                        Type = "object",
                                        Properties = new Dictionary<string, OpenApiSchema>
                                        {
                                            {
                                                "value",
                                                new OpenApiSchema
                                                {
                                                    Type = "array",
                                                    Items = new OpenApiSchema
                                                    {
                                                        Reference = new OpenApiReference
                                                        {
                                                            Type = ReferenceType.Schema,
                                                            Id = EntitySet.EntityType().FullName()
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            operation.Responses.Add(Constants.StatusCodeDefault, Constants.StatusCodeDefault.GetResponse());
        }
    }
}
