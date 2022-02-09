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
    /// Create an Entity:
    /// The Path Item Object for the entity set contains the keyword "post" with an Operation Object as value
    /// that describes the capabilities for creating new entities.
    /// </summary>
    internal class EntitySetPostOperationHandler : EntitySetOperationHandler
    {
        /// <inheritdoc/>
        public override OperationType OperationType => OperationType.Post;

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            // Summary
            operation.Summary = "Add new entity to " + EntitySet.Name;

            // OperationId
            if (Context.Settings.EnableOperationId)
            {
                string typeName = EntitySet.EntityType().Name;
                operation.OperationId = EntitySet.Name + "." + typeName + ".Create" + Utils.UpperFirstChar(typeName);
            }

            base.SetBasicInfo(operation);
        }

        /// <inheritdoc/>
        protected override void SetRequestBody(OpenApiOperation operation)
        {
            // The requestBody field contains a Request Body Object for the request body
            // that references the schema of the entity set’s entity type in the global schemas.
            operation.RequestBody = new OpenApiRequestBody
            {
                Required = true,
                Description = "New entity",
                Content = GetContentDescription()
            };

            base.SetRequestBody(operation);
        }

        /// <inheritdoc/>
        protected override void SetResponses(OpenApiOperation operation)
        {
            operation.Responses = new OpenApiResponses
            {
                {
                    Constants.StatusCode201,
                    new OpenApiResponse
                    {
                        Description = "Created entity",
                        Content = GetContentDescription()
                    }
                }
            };

            operation.AddErrorResponses(Context.Settings, false);

            base.SetResponses(operation);
        }

        protected override void SetSecurity(OpenApiOperation operation)
        {
            InsertRestrictionsType insert = Context.Model.GetRecord<InsertRestrictionsType>(EntitySet, CapabilitiesConstants.InsertRestrictions);
            if (insert == null || insert.Permissions == null)
            {
                return;
            }

            operation.Security = Context.CreateSecurityRequirements(insert.Permissions).ToList();
        }

        protected override void AppendCustomParameters(OpenApiOperation operation)
        {
            InsertRestrictionsType insert = Context.Model.GetRecord<InsertRestrictionsType>(EntitySet, CapabilitiesConstants.InsertRestrictions);
            if (insert == null)
            {
                return;
            }

            if (insert.CustomQueryOptions != null)
            {
                AppendCustomParameters(operation, insert.CustomQueryOptions, ParameterLocation.Query);
            }

            if (insert.CustomHeaders != null)
            {
                AppendCustomParameters(operation, insert.CustomHeaders, ParameterLocation.Header);
            }
        }

        /// <summary>
        /// Get the entity content description.
        /// </summary>
        /// <returns>The entity content description.</returns>
        private IDictionary<string, OpenApiMediaType> GetContentDescription()
        {
            OpenApiSchema schema = GetEntitySchema();
            var content = new Dictionary<string, OpenApiMediaType>();

            if (EntitySet.EntityType().HasStream)
            {
                IEnumerable<string> mediaTypes = Context.Model.GetCollection(EntitySet.EntityType(),
                    CapabilitiesConstants.AcceptableMediaTypes);

                if (mediaTypes != null)
                {
                    foreach (string item in mediaTypes)
                    {
                        content.Add(item, null);
                    }
                }
                else
                {
                    // Default content type
                    content.Add(Constants.ApplicationOctetStreamMediaType, new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "string",
                            Format = "binary"
                        }
                    });
                }
            }

            content.Add(Constants.ApplicationJsonMediaType, new OpenApiMediaType
            {
                Schema = schema
            });

            return content;
        }

        /// <summary>
        /// Get the entity schema.
        /// </summary>
        /// <returns>The entity schema.</returns>
        private OpenApiSchema GetEntitySchema()
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
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.Schema,
                        Id = EntitySet.EntityType().FullName()
                    }
                };
            }

            return schema;
        }
    }
}
