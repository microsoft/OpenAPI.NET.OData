// ------------------------------------------------------------
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
    /// Base class for entity set update (patch or put) operations.
    /// </summary>
    internal abstract class EntityUpdateOperationHandler : EntitySetOperationHandler
    {
        private UpdateRestrictionsType _updateRestrictions;

        protected override void Initialize(ODataContext context, ODataPath path)
        {
            base.Initialize(context, path);

            _updateRestrictions = Context.Model.GetRecord<UpdateRestrictionsType>(EntitySet, CapabilitiesConstants.UpdateRestrictions);
        }

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            IEdmEntityType entityType = EntitySet.EntityType();
            ODataKeySegment keySegment = Path.LastSegment as ODataKeySegment;

            // Summary and Description
            string placeHolder = $"Update entity in {EntitySet.Name} by key ({keySegment.Identifier})";
            operation.Summary = _updateRestrictions?.Description ?? placeHolder;
            operation.Description = _updateRestrictions?.LongDescription;

            // OperationId
            if (Context.Settings.EnableOperationId)
            {
                string typeName = entityType.Name;
                string keyName = string.Join("", keySegment.Identifier.Split(',').Select(x => Utils.UpperFirstChar(x)));
                operation.OperationId = $"{EntitySet.Name}.{typeName}.Update{Utils.UpperFirstChar(typeName)}By{keyName}";
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

        protected override void SetSecurity(OpenApiOperation operation)
        {
            if (_updateRestrictions?.Permissions == null)
            {
                return;
            }

            operation.Security = Context.CreateSecurityRequirements(_updateRestrictions.Permissions).ToList();
        }

        protected override void AppendCustomParameters(OpenApiOperation operation)
        {
            if (_updateRestrictions == null)
            {
                return;
            }

            if (_updateRestrictions.CustomHeaders != null)
            {
                AppendCustomParameters(operation, _updateRestrictions.CustomHeaders, ParameterLocation.Header);
            }

            if (_updateRestrictions.CustomQueryOptions != null)
            {
                AppendCustomParameters(operation, _updateRestrictions.CustomQueryOptions, ParameterLocation.Query);
            }
        }

        private OpenApiSchema GetOpenApiSchema()
        {
            if (Context.Settings.EnableDerivedTypesReferencesForRequestBody)
            {
                return EdmModelHelper.GetDerivedTypesReferenceSchema(EntitySet.EntityType(), Context.Model);
            }

            return new OpenApiSchema
            {
                UnresolvedReference = true,
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.Schema,
                    Id = EntitySet.EntityType().FullName()
                }
            };
        }
    }
}
