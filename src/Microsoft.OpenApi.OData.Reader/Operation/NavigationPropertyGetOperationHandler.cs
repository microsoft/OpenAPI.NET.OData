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
    /// Retrieve a navigation property from a navigation source.
    /// The Path Item Object for the entity contains the keyword get with an Operation Object as value
    /// that describes the capabilities for retrieving a navigation property form a navigation source.
    /// </summary>
    internal class NavigationPropertyGetOperationHandler : NavigationPropertyOperationHandler
    {
        /// <inheritdoc/>
        public override OperationType OperationType => OperationType.Get;

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            // Summary
            operation.Summary = "Get " + NavigationProperty.Name + " from " + NavigationSource.Name;

            // OperationId
            if (Context.Settings.OperationId)
            {
                string prefix = "Get";
                if (!LastSegmentIsKeySegment && NavigationProperty.TargetMultiplicity() == EdmMultiplicity.Many)
                {
                    prefix = "List";
                }

                operation.OperationId = GetOperationId(prefix);
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
                        Description = "Retrieved navigation property",
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
                                            Id = NavigationProperty.ToEntityType().FullName()
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

        /// <inheritdoc/>
        protected override void SetParameters(OpenApiOperation operation)
        {
            base.SetParameters(operation);

            if (operation.Parameters == null)
            {
                operation.Parameters = new List<OpenApiParameter>();
            }

            if (!LastSegmentIsKeySegment && NavigationProperty.TargetMultiplicity() == EdmMultiplicity.Many)
            {
                // Need to verify that TopSupported or others should be applyed to navigaiton source.
                // So, how about for the navigation property.
                OpenApiParameter parameter = Context.CreateTop(NavigationProperty);
                if (parameter != null)
                {
                    operation.Parameters.Add(parameter);
                }

                parameter = Context.CreateSkip(NavigationProperty);
                if (parameter != null)
                {
                    operation.Parameters.Add(parameter);
                }

                parameter = Context.CreateSearch(NavigationProperty);
                if (parameter != null)
                {
                    operation.Parameters.Add(parameter);
                }

                parameter = Context.CreateFilter(NavigationProperty);
                if (parameter != null)
                {
                    operation.Parameters.Add(parameter);
                }

                parameter = Context.CreateCount(NavigationProperty);
                if (parameter != null)
                {
                    operation.Parameters.Add(parameter);
                }

                parameter = Context.CreateOrderBy(NavigationProperty);
                if (parameter != null)
                {
                    operation.Parameters.Add(parameter);
                }

                parameter = Context.CreateSelect(NavigationProperty);
                if (parameter != null)
                {
                    operation.Parameters.Add(parameter);
                }

                parameter = Context.CreateExpand(NavigationProperty);
                if (parameter != null)
                {
                    operation.Parameters.Add(parameter);
                }
            }
            else
            {
                OpenApiParameter parameter = Context.CreateSelect(NavigationProperty);
                if (parameter != null)
                {
                    operation.Parameters.Add(parameter);
                }

                parameter = Context.CreateExpand(NavigationProperty);
                if (parameter != null)
                {
                    operation.Parameters.Add(parameter);
                }
            }
        }
    }
}
