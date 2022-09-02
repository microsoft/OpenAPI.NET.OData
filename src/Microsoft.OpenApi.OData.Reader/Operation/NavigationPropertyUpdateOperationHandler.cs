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
    /// Update a navigation property for a navigation source.
    /// The Path Item Object for the entity set contains the keyword patch / put with an Operation Object as value
    /// that describes the capabilities for updating the navigation property for a navigation source.
    /// </summary>
    internal abstract class NavigationPropertyUpdateOperationHandler : NavigationPropertyOperationHandler
    {
        private UpdateRestrictionsType _updateRestriction;

        /// <inheritdoc/>
        protected override void Initialize(ODataContext context, ODataPath path)
        {
            base.Initialize(context, path);
            _updateRestriction = GetRestrictionAnnotation(CapabilitiesConstants.UpdateRestrictions) as UpdateRestrictionsType;
        }

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            // Summary and Description
            string placeHolder = "Update the navigation property " + NavigationProperty.Name + " in " + NavigationSource.Name;
            operation.Summary = _updateRestriction?.Description ?? placeHolder;
            operation.Description = _updateRestriction?.LongDescription;

            // OperationId
            if (Context.Settings.EnableOperationId)
            {
                string prefix = "Update";
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
                Description = "New navigation property values",
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

        protected override void SetSecurity(OpenApiOperation operation)
        {
            if (_updateRestriction == null)
            {
                return;
            }

            operation.Security = Context.CreateSecurityRequirements(_updateRestriction.Permissions).ToList();
        }

        protected override void AppendCustomParameters(OpenApiOperation operation)
        {
            if (_updateRestriction == null)
            {
                return;
            }

            if (_updateRestriction.CustomHeaders != null)
            {
                AppendCustomParameters(operation, _updateRestriction.CustomHeaders, ParameterLocation.Header);
            }

            if (_updateRestriction.CustomQueryOptions != null)
            {
                AppendCustomParameters(operation, _updateRestriction.CustomQueryOptions, ParameterLocation.Query);
            }
        }

        private OpenApiSchema GetOpenApiSchema()
        {           
            if (Context.Settings.EnableDerivedTypesReferencesForRequestBody)
            {
                return EdmModelHelper.GetDerivedTypesReferenceSchema(NavigationProperty.ToEntityType(), Context.Model);
            }

            return new OpenApiSchema
            {
                UnresolvedReference = true,
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.Schema,
                    Id = NavigationProperty.ToEntityType().FullName()
                }
            };
        }
    }
}
