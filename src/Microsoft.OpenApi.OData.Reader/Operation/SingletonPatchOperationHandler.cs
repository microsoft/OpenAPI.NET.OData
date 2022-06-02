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

        /// <summary>
        /// Gets/Sets the <see cref="UpdateRestrictionsType"/>
        /// </summary>
        private UpdateRestrictionsType UpdateRestrictions { get; set; }

        protected override void Initialize(ODataContext context, ODataPath path)
        {
            base.Initialize(context, path);

            UpdateRestrictions = Context.Model.GetRecord<UpdateRestrictionsType>(Singleton, CapabilitiesConstants.UpdateRestrictions);
        }

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            // Summary and Descriptions
            string placeHolder = "Update " + Singleton.Name;
            operation.Summary = UpdateRestrictions?.Description ?? placeHolder;
            operation.Description = UpdateRestrictions?.LongDescription;

            // OperationId
            if (Context.Settings.EnableOperationId)
            {
                string typeName = Singleton.EntityType().Name;
                operation.OperationId = Singleton.Name + "." + typeName + ".Update" + Utils.UpperFirstChar(typeName);
            }
        }

        /// <inheritdoc/>
        protected override void SetRequestBody(OpenApiOperation operation)
        {          
            operation.RequestBody = new OpenApiRequestBody
            {
                Required = true,
                Description = "New property values",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    {
                        Constants.ApplicationJsonMediaType, new OpenApiMediaType
                        {
                            Schema = GetOpenApiSchema()
                        }
                    }
                }
            };

            base.SetRequestBody(operation);
        }

        /// <inheritdoc/>
        protected override void SetResponses(OpenApiOperation operation)
        {
            operation.AddErrorResponses(Context.Settings, true, GetOpenApiSchema());
            base.SetResponses(operation);
        }

        /// <inheritdoc/>
        protected override void SetSecurity(OpenApiOperation operation)
        {
            if (UpdateRestrictions?.Permissions == null)
            {
                return;
            }

            operation.Security = Context.CreateSecurityRequirements(UpdateRestrictions.Permissions).ToList();
        }

        /// <inheritdoc/>
        protected override void AppendCustomParameters(OpenApiOperation operation)
        {
            if (UpdateRestrictions == null)
            {
                return;
            }

            if (UpdateRestrictions.CustomHeaders != null)
            {
                AppendCustomParameters(operation, UpdateRestrictions.CustomHeaders, ParameterLocation.Header);
            }

            if (UpdateRestrictions.CustomQueryOptions != null)
            {
                AppendCustomParameters(operation, UpdateRestrictions.CustomQueryOptions, ParameterLocation.Query);
            }
        }

        private OpenApiSchema GetOpenApiSchema()
        {
            if (Context.Settings.EnableDerivedTypesReferencesForRequestBody)
            {
                return EdmModelHelper.GetDerivedTypesReferenceSchema(Singleton.EntityType(), Context.Model);
            }

            return new OpenApiSchema
            {
                UnresolvedReference = true,
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.Schema,
                    Id = Singleton.EntityType().FullName()
                }
            };
        }
    }
}
