// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Generator;

namespace Microsoft.OpenApi.OData.Operation
{
    /// <summary>
    /// Create a navigation property ref for a navigation source.
    /// </summary>
    internal class RefPostOperationHandler : NavigationPropertyOperationHandler
    {
        /// <inheritdoc/>
        public override OperationType OperationType => OperationType.Post;

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            // Summary and Description
            string placeHolder = "Create new navigation property ref to " + NavigationProperty.Name + " for " + NavigationSource.Name;
            operation.Summary = Restriction?.InsertRestrictions?.Description ?? placeHolder;
            operation.Description = Restriction?.InsertRestrictions?.LongDescription;

            // OperationId
            if (Context.Settings.EnableOperationId)
            {
                string prefix = "CreateRef";
                operation.OperationId = GetOperationId(prefix);
            }            
        }

        /// <inheritdoc/>
        protected override void SetRequestBody(OpenApiOperation operation)
        {
            operation.RequestBody = new OpenApiRequestBody
            {
                UnresolvedReference = true,
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.RequestBody,
                    Id = Constants.ReferenceRequestBodyName
                }
            };

            base.SetRequestBody(operation);
        }

        /// <inheritdoc/>
        protected override void SetResponses(OpenApiOperation operation)
        {
            operation.Responses = new OpenApiResponses
            {
                {
                    Context.Settings.UseSuccessStatusCodeRange ? Constants.StatusCodeClass2XX : Constants.StatusCode204,
                    new OpenApiResponse { Description = "Success" } 
                }
            };

    		operation.AddErrorResponses(Context.Settings, false);

            base.SetResponses(operation);
        }

        protected override void SetSecurity(OpenApiOperation operation)
        {
            if (Restriction == null || Restriction.InsertRestrictions == null)
            {
                return;
            }

            operation.Security = Context.CreateSecurityRequirements(Restriction.InsertRestrictions.Permissions).ToList();
        }

        protected override void AppendCustomParameters(OpenApiOperation operation)
        {
            if (Restriction == null || Restriction.InsertRestrictions == null)
            {
                return;
            }

            if (Restriction.InsertRestrictions.CustomHeaders != null)
            {
                AppendCustomParameters(operation, Restriction.InsertRestrictions.CustomHeaders, ParameterLocation.Header);
            }

            if (Restriction.InsertRestrictions.CustomQueryOptions != null)
            {
                AppendCustomParameters(operation, Restriction.InsertRestrictions.CustomQueryOptions, ParameterLocation.Query);
            }
        }
    }
}
