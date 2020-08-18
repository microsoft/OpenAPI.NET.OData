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
    /// Update a media content for an Entity
    /// </summary>
    internal class MediaEntityPutOperationHandler : EntitySetOperationHandler
    {
        /// <inheritdoc/>
        public override OperationType OperationType => OperationType.Put;

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            string typeName = EntitySet.EntityType().Name;

            // Summary
            operation.Summary = $"Update media content for {typeName} in {EntitySet.Name}";

            // OperationId
            if (Context.Settings.EnableOperationId)
            {
                string identifier = Path.LastSegment.Kind == ODataSegmentKind.StreamContent ? "Content" : Path.LastSegment.Identifier;
                operation.OperationId = EntitySet.Name + "." + typeName + ".Update" + Utils.UpperFirstChar(identifier);
            }

            base.SetBasicInfo(operation);
        }

        /// <inheritdoc/>
        protected override void SetRequestBody(OpenApiOperation operation)
        {
            OpenApiSchema schema = null;

            if (Context.Settings.EnableDerivedTypesReferencesForRequestBody)
            {
                schema = EdmModelHelper.GetDerivedTypesReferenceSchema(EntitySet.EntityType(), Context.Model);
            }

            if (schema == null)
            {
                schema = new OpenApiSchema
                {
                    Type = "string",
                    Format = "binary"
                };
            }

            operation.RequestBody = new OpenApiRequestBody
            {
                Required = true,
                Description = "New media content.",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    {
                        Constants.ApplicationOctetStreamMediaType, new OpenApiMediaType
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
            UpdateRestrictionsType update = Context.Model.GetRecord<UpdateRestrictionsType>(EntitySet, CapabilitiesConstants.UpdateRestrictions);
            if (update == null || update.Permissions == null)
            {
                return;
            }

            operation.Security = Context.CreateSecurityRequirements(update.Permissions).ToList();
        }
    }
}
