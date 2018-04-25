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
    /// Create a navigation for a navigation source.
    /// The Path Item Object for the entity set contains the keyword delete with an Operation Object as value
    /// that describes the capabilities for create a navigation for a navigation source.
    /// </summary>
    internal class NavigationPropertyPostOperationHandler : NavigationPropertyOperationHandler
    {
        /// <inheritdoc/>
        public override OperationType OperationType => OperationType.Post;

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            // Summary
            operation.Summary = "Create new navigation property to " + NavigationProperty.Name + " for " + NavigationSource.Name;

            // OperationId
            if (Context.Settings.OperationId)
            {
                string prefix = "Create";
                string key = NavigationSource.Name + "." + NavigationProperty.Name + "-" + prefix + Utils.UpperFirstChar(NavigationProperty.ToEntityType().Name);
                int index = Context.GetIndex(key);
                operation.OperationId = NavigationSource.Name + "." + NavigationProperty.Name + index + "-" + prefix + Utils.UpperFirstChar(NavigationProperty.ToEntityType().Name);
            }
        }

        /// <inheritdoc/>
        protected override void SetRequestBody(OpenApiOperation operation)
        {
            operation.RequestBody = new OpenApiRequestBody
            {
                Required = true,
                Description = "New navigation property",
                Content = new Dictionary<string, OpenApiMediaType>
                    {
                        {
                            Constants.ApplicationJsonMediaType, new OpenApiMediaType
                            {
                                Schema = new OpenApiSchema
                                {
                                    Reference = new OpenApiReference
                                    {
                                        Type = ReferenceType.Schema,
                                        Id = NavigationSource.EntityType().FullName()
                                    }
                                }
                            }
                        }
                    }
            };
        }

        /// <inheritdoc/>
        protected override void SetResponses(OpenApiOperation operation)
        {
            operation.Responses = new OpenApiResponses
            {
                {
                    Constants.StatusCode201,
                    new OpenApiResponse
                    {
                        Description = "Created navigation property.",
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
                                            Id = NavigationSource.EntityType().FullName()
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
