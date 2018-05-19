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
    /// Update a navigation property for a navigation source.
    /// The Path Item Object for the entity set contains the keyword patch with an Operation Object as value
    /// that describes the capabilities for updating the navigation property for a navigation source.
    /// </summary>
    internal class NavigationPropertyPatchOperationHandler : NavigationPropertyOperationHandler
    {
        /// <inheritdoc/>
        public override OperationType OperationType => OperationType.Patch;

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            // Summary
            operation.Summary = "Update the navigation property " + NavigationProperty.Name + " in " + NavigationSource.Name;

            // OperationId
            if (Context.Settings.OperationId)
            {
                string prefix = "Update";
                //string key = NavigationSource.Name + "." + NavigationProperty.Name + "-" + prefix + Utils.UpperFirstChar(NavigationProperty.ToEntityType().Name);
               // int index = Context.GetIndex(key);
               // operation.OperationId = NavigationSource.Name + "." + NavigationProperty.Name + index + "-" + prefix + Utils.UpperFirstChar(NavigationProperty.ToEntityType().Name);

                operation.OperationId = GetOperationId(prefix);
            }
        }

        /// <inheritdoc/>
        protected override void SetParameters(OpenApiOperation operation)
        {
            operation.Parameters = Context.CreateKeyParameters(NavigationSource.EntityType());
        }

        /// <inheritdoc/>
        protected override void SetRequestBody(OpenApiOperation operation)
        {
            operation.RequestBody = new OpenApiRequestBody
            {
                Required = true,
                Description = "New navigation property values",
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
                                    Id = NavigationProperty.ToEntityType().FullName()
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
                { Constants.StatusCode204, Constants.StatusCode204.GetResponse() },
                { Constants.StatusCodeDefault, Constants.StatusCodeDefault.GetResponse() }
            };
        }
    }
}
