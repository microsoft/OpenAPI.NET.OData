// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Generator;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.OpenApi.OData.Operation
{
    /// <summary>
    /// Retrieve a media content for an Entity
    /// </summary>
    internal class MediaEntityGetOperationHandler : EntitySetOperationHandler
    {
        /// <inheritdoc/>
        public override OperationType OperationType => OperationType.Get;

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            string typeName = EntitySet.EntityType().Name;

            // Summary
            operation.Summary = $"Get media content for {typeName} from {EntitySet.Name}";

            // OperationId
            if (Context.Settings.EnableOperationId)
            {
                string identifier = Path.LastSegment.Kind == ODataSegmentKind.StreamContent ? "Content" : Path.LastSegment.Identifier;
                operation.OperationId = EntitySet.Name + "." + typeName + ".Get" + Utils.UpperFirstChar(identifier);
            }

            base.SetBasicInfo(operation);
        }

        /// <inheritdoc/>
        protected override void SetResponses(OpenApiOperation operation)
        {
            OpenApiSchema schema = null;

            if (Context.Settings.EnableDerivedTypesReferencesForResponses)
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

            operation.Responses = new OpenApiResponses
            {
                {
                    Constants.StatusCode200,
                    new OpenApiResponse
                    {
                        Description = "Retrieved media content",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            {
                                Constants.ApplicationOctetStreamMediaType,
                                new OpenApiMediaType
                                {
                                    Schema = schema
                                }
                            }
                        }
                    }
                }
            };
            operation.Responses.Add(Constants.StatusCodeDefault, Constants.StatusCodeDefault.GetResponse());

            base.SetResponses(operation);
        }
        /// <inheritdoc/>
        protected override void SetSecurity(OpenApiOperation operation)
        {
            ReadRestrictionsType read = Context.Model.GetRecord<ReadRestrictionsType>(EntitySet, CapabilitiesConstants.ReadRestrictions);
            if (read == null)
            {
                return;
            }

            ReadRestrictionsBase readBase = read;
            if (read.ReadByKeyRestrictions != null)
            {
                readBase = read.ReadByKeyRestrictions;
            }

            if (readBase == null && readBase.Permissions == null)
            {
                return;
            }

            operation.Security = Context.CreateSecurityRequirements(readBase.Permissions).ToList();
        }
    }
}
