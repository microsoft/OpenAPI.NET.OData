// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Linq;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Generator;

namespace Microsoft.OpenApi.OData.Operation
{
    /// <summary>
    /// Delete a navigation for a navigation source.
    /// The Path Item Object for the navigation property contains the keyword delete with an Operation Object as value
    /// that describes the capabilities for delete a navigation for a navigation source.
    /// </summary>
    internal class NavigationPropertyDeleteOperationHandler : NavigationPropertyOperationHandler
    {
        /// <inheritdoc/>
        public override OperationType OperationType => OperationType.Delete;

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            // Summary
            operation.Summary = "Delete navigation property " + NavigationProperty.Name + " for " + NavigationSource.Name;

            // OperationId
            if (Context.Settings.EnableOperationId)
            {
                string prefix = "Delete";
                operation.OperationId = GetOperationId(prefix);
            }

            base.SetBasicInfo(operation);
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
        protected override void SetSecurity(OpenApiOperation operation)
        {
            if (Restriction == null || Restriction.DeleteRestrictions == null)
            {
                return;
            }

            operation.Security = Context.CreateSecurityRequirements(Restriction.DeleteRestrictions.Permissions).ToList();
        }

        /// <inheritdoc/>
        protected override void SetResponses(OpenApiOperation operation)
        {
            operation.AddErrorResponses(Context.Settings, true);
            base.SetResponses(operation);
        }

        protected override void AppendCustomParameters(OpenApiOperation operation)
        {
            if (Restriction == null || Restriction.DeleteRestrictions == null)
            {
                return;
            }

            if (Restriction.DeleteRestrictions.CustomHeaders != null)
            {
                AppendCustomParameters(operation, Restriction.DeleteRestrictions.CustomHeaders, ParameterLocation.Header);
            }

            if (Restriction.DeleteRestrictions.CustomQueryOptions != null)
            {
                AppendCustomParameters(operation, Restriction.DeleteRestrictions.CustomQueryOptions, ParameterLocation.Query);
            }
        }
    }
}
