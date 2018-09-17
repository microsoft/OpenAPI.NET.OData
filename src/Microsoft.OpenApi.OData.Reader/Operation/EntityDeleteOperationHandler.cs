// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Generator;

namespace Microsoft.OpenApi.OData.Operation
{
    /// <summary>
    /// Delete an Entity
    /// The Path Item Object for the entity set contains the keyword delete with an Operation Object as value
    /// that describes the capabilities for deleting the entity.
    /// </summary>
    internal class EntityDeleteOperationHandler : EntitySetOperationHandler
    {
        /// <inheritdoc/>
        public override OperationType OperationType => OperationType.Delete;

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            // Summary
            operation.Summary = "Delete entity from " + EntitySet.Name;
            // override the summary using the request.Description.
            var request = Context.FindRequest(EntitySet, OperationType.ToString());
            if (request != null && request.Description != null)
            {
                operation.Summary = request.Description;
            }

            // OperationId
            if (Context.Settings.EnableOperationId)
            {
                string typeName = EntitySet.EntityType().Name;
                operation.OperationId = EntitySet.Name + "." + typeName + ".Delete" + Utils.UpperFirstChar(typeName);
            }
        }

        /// <inheritdoc/>
        protected override void SetParameters(OpenApiOperation operation)
        {
            base.SetParameters(operation);

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "If-Match",
                In = ParameterLocation.Header,
                Description = "ETag",
                Schema = new OpenApiSchema
                {
                    Type = "string"
                }
            });
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
