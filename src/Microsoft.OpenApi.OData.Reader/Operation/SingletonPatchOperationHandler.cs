﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Generator;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;

namespace Microsoft.OpenApi.OData.Operation
{
    /// <summary>
    /// Update a Singleton
    /// The Path Item Object for the entity set contains the keyword patch with an Operation Object as value
    /// that describes the capabilities for updating the singleton, unless the singleton is read-only.
    /// </summary>
    internal class SingletonPatchOperationHandler : SingletonOperationHandler
    {
       /// <inheritdoc/>
        public override OperationType OperationType => OperationType.Patch;

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            // Summary, this summary maybe update in the base function call.
            operation.Summary = "Update " + Singleton.Name;

            // OperationId
            if (Context.Settings.EnableOperationId)
            {
                string typeName = Singleton.EntityType().Name;
                operation.OperationId = Singleton.Name + "." + typeName + ".Update" + Utils.UpperFirstChar(typeName);
            }

            base.SetBasicInfo(operation);
        }

        /// <inheritdoc/>
        protected override void SetRequestBody(OpenApiOperation operation)
        {
            OpenApiSchema schema = null;

            if (Context.Settings.EnableDerivedTypesReferencesForRequestBody)
            {
                schema = EdmModelHelper.GetDerivedTypesReferenceSchema(Singleton.EntityType(), Context.Model);
            }

            if (schema == null)
            {
                schema = new OpenApiSchema
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.Schema,
                        Id = Singleton.EntityType().FullName()
                    }
                };
            }

            operation.RequestBody = new OpenApiRequestBody
            {
                Required = true,
                Description = "New property values",
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

            base.SetRequestBody(operation);
        }

        /// <inheritdoc/>
        protected override void SetResponses(OpenApiOperation operation)
        {
            operation.Responses = new OpenApiResponses
            {
                { Constants.StatusCode204, Constants.StatusCode204.GetResponse() },
                { Constants.StatusCodeDefault, Constants.StatusCodeDefault.GetResponse() }
            };

            base.SetResponses(operation);
        }

        /// <inheritdoc/>
        protected override void SetSecurity(OpenApiOperation operation)
        {
            UpdateRestrictionsType update = Context.Model.GetRecord<UpdateRestrictionsType>(Singleton, CapabilitiesConstants.UpdateRestrictions);
            if (update == null || update.Permissions == null)
            {
                return;
            }

            operation.Security = Context.CreateSecurityRequirements(update.Permissions).ToList();
        }

        /// <inheritdoc/>
        protected override void AppendCustomParameters(OpenApiOperation operation)
        {
            UpdateRestrictionsType update = Context.Model.GetRecord<UpdateRestrictionsType>(Singleton, CapabilitiesConstants.UpdateRestrictions);
            if (update == null)
            {
                return;
            }

            if (update.CustomHeaders != null)
            {
                AppendCustomParameters(operation, update.CustomHeaders, ParameterLocation.Header);
            }

            if (update.CustomQueryOptions != null)
            {
                AppendCustomParameters(operation, update.CustomQueryOptions, ParameterLocation.Query);
            }
        }
    }
}
