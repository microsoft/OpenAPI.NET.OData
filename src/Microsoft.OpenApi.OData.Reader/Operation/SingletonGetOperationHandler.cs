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
    /// Retrieve a Singleton
    /// The Path Item Object for the entity set contains the keyword get with an Operation Object as value
    /// that describes the capabilities for retrieving the singleton.
    /// </summary>
    internal class SingletonGetOperationHandler : SingletonOperationHandler
    {
        /// <inheritdoc/>
        public override OperationType OperationType => OperationType.Get;

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            // Summary
            operation.Summary = "Get " + Singleton.Name;
            var request = Context.FindRequest(Singleton, OperationType.ToString());
            if (request != null && request.Description != null)
            {
                operation.Summary = request.Description;
            }

            // OperationId
            if (Context.Settings.OperationId)
            {
                string typeName = Singleton.EntityType().Name;
                operation.OperationId = Singleton.Name + "." + typeName + ".Get" + Utils.UpperFirstChar(typeName);
            }
        }

        /// <inheritdoc/>
        protected override void SetParameters(OpenApiOperation operation)
        {
            operation.Parameters = new List<OpenApiParameter>();
            IEdmEntityType entityType = Singleton.EntityType();

            // $select
            OpenApiParameter parameter = Context.CreateSelect(Singleton);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }

            // $expand
            parameter = Context.CreateExpand(Singleton);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }

            var request = Context.FindRequest(Singleton, OperationType.ToString());
            AppendCustomParameters(operation, request);
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
                        Description = "Retrieved entity",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            {
                                Constants.ApplicationJsonMediaType,
                                new OpenApiMediaType
                                {
                                    Schema = new OpenApiSchema
                                    {
                                        Reference = new OpenApiReference
                                        {
                                            Type = ReferenceType.Schema,
                                            Id = Singleton.EntityType().FullName()
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
            };
            operation.Responses.Add(Constants.StatusCodeDefault, Constants.StatusCodeDefault.GetResponse());
        }
    }
}
