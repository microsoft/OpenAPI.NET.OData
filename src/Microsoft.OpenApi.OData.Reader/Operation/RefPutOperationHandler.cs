// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Generator;

namespace Microsoft.OpenApi.OData.Operation
{
    /// <summary>
    /// Update a navigation property ref for a navigation source.
    /// </summary>
    internal class RefPutOperationHandler : NavigationPropertyOperationHandler
    {
        /// <inheritdoc/>
        public override OperationType OperationType => OperationType.Patch;

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            // Summary
            operation.Summary = "Update the ref of navigation property " + NavigationProperty.Name + " in " + NavigationSource.Name;

            // OperationId
            if (Context.Settings.EnableOperationId)
            {
                string prefix = "UpdateRef";
                operation.OperationId = GetOperationId(prefix);
            }

            base.SetBasicInfo(operation);
        }

        /// <inheritdoc/>
        protected override void SetRequestBody(OpenApiOperation operation)
        {
            operation.RequestBody = new OpenApiRequestBody
            {
                Required = true,
                Description = "New navigation property ref values",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    {
                        Constants.ApplicationJsonMediaType, new OpenApiMediaType
                        {
                            Schema = new()
                            {
                                Reference = new OpenApiReference {
                                    Id = Constants.ReferenceUpdateSchemaName,
                                    Type = ReferenceType.Schema
                                },
                            }
                        }
                    }
                }
            };

            base.SetRequestBody(operation);
        }

        /// <inheritdoc/>
        protected override void SetResponses(OpenApiOperation operation)
        {
    		operation.AddErrorResponses(Context.Settings, true);
            base.SetResponses(operation);
        }

        protected override void SetSecurity(OpenApiOperation operation)
        {
            if (Restriction == null || Restriction.UpdateRestrictions == null)
            {
                return;
            }

            operation.Security = Context.CreateSecurityRequirements(Restriction.UpdateRestrictions.Permissions).ToList();
        }

        protected override void AppendCustomParameters(OpenApiOperation operation)
        {
            if (Restriction == null || Restriction.UpdateRestrictions == null)
            {
                return;
            }

            if (Restriction.UpdateRestrictions.CustomHeaders != null)
            {
                AppendCustomParameters(operation, Restriction.UpdateRestrictions.CustomHeaders, ParameterLocation.Header);
            }

            if (Restriction.UpdateRestrictions.CustomQueryOptions != null)
            {
                AppendCustomParameters(operation, Restriction.UpdateRestrictions.CustomQueryOptions, ParameterLocation.Query);
            }
        }
    }
}
