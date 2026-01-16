// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Generator;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;
using Microsoft.OpenApi.OData.Vocabulary.Core;

namespace Microsoft.OpenApi.OData.Operation
{
    /// <summary>
    /// Create an Entity:
    /// The Path Item Object for the entity set contains the keyword "post" with an Operation Object as value
    /// that describes the capabilities for creating new entities.
    /// </summary>
    internal class EntitySetPostOperationHandler : EntitySetOperationHandler
    {
        /// <summary>
        /// Initializes a new instance of <see cref="EntitySetPostOperationHandler"/> class.
        /// </summary>
        /// <param name="document">The document to use to lookup references.</param>
        public EntitySetPostOperationHandler(OpenApiDocument document) : base(document)
        {
            
        }
        /// <inheritdoc/>
        public override HttpMethod OperationType => HttpMethod.Post;
               
        private InsertRestrictionsType? _insertRestrictions;

        protected override void Initialize(ODataContext context, ODataPath path)
        {
            base.Initialize(context, path);

            if (!string.IsNullOrEmpty(TargetPath))
                _insertRestrictions = Context?.Model.GetRecord<InsertRestrictionsType>(TargetPath, CapabilitiesConstants.InsertRestrictions);
            if (Context is not null && EntitySet is not null)
            {
                var entityInsertRestrictions = Context.Model.GetRecord<InsertRestrictionsType>(EntitySet, CapabilitiesConstants.InsertRestrictions);
                _insertRestrictions?.MergePropertiesIfNull(entityInsertRestrictions);
                _insertRestrictions ??= entityInsertRestrictions;
            }
        }

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            // Summary and Description
            string placeHolder = "Add new entity to " + EntitySet?.Name;
            operation.Summary = _insertRestrictions?.Description ?? placeHolder;
            operation.Description = _insertRestrictions?.LongDescription;

            // OperationId
            if (Context is {Settings.EnableOperationId: true} && EntitySet is not null)
            {
                string typeName = EntitySet.EntityType.Name;
                operation.OperationId = EntitySet.Name + "." + typeName + ".Create" + Utils.UpperFirstChar(typeName);
            }
        }

        /// <inheritdoc/>
        protected override void SetRequestBody(OpenApiOperation operation)
        {
            // The requestBody field contains a Request Body Object for the request body
            // that references the schema of the entity set's entity type in the global schemas.
            operation.RequestBody = new OpenApiRequestBody
            {
                Required = EntitySet?.EntityType?.ShouldRequestBodyBeRequired(
                    isUpdateOperation: false,
                    Context?.Model) ?? true,
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
                    Context?.Settings.UseSuccessStatusCodeRange ?? false ? Constants.StatusCodeClass2XX : Constants.StatusCode201,
                    new OpenApiResponse
                    {
                        Description = "Created entity",
                        Content = GetContentDescription()
                    }
                }
            };

            if (Context is not null)
                operation.AddErrorResponses(Context.Settings, _document, false);

            base.SetResponses(operation);
        }

        protected override void SetSecurity(OpenApiOperation operation)
        {
            if (_insertRestrictions?.Permissions == null)
            {
                return;
            }

            operation.Security = Context?.CreateSecurityRequirements(_insertRestrictions.Permissions, _document).ToList();
        }

        protected override void AppendCustomParameters(OpenApiOperation operation)
        {
            if (_insertRestrictions == null)
            {
                return;
            }

            if (_insertRestrictions.CustomQueryOptions != null)
            {
                AppendCustomParameters(operation, _insertRestrictions.CustomQueryOptions, ParameterLocation.Query);
            }

            if (_insertRestrictions.CustomHeaders != null)
            {
                AppendCustomParameters(operation, _insertRestrictions.CustomHeaders, ParameterLocation.Header);
            }
        }

        /// <summary>
        /// Get the entity content description.
        /// </summary>
        /// <returns>The entity content description.</returns>
        private Dictionary<string, OpenApiMediaType> GetContentDescription()
        {
            var schema = GetEntitySchema();
            var content = new Dictionary<string, OpenApiMediaType>();

            if (EntitySet is {EntityType.HasStream: true})
            {
                if (Context?.Model.GetCollection(EntitySet.EntityType,
                    CoreConstants.AcceptableMediaTypes) is {} mediaTypes)
                {
                    foreach (string item in mediaTypes)
                    {
                        content.Add(item, new());
                    }
                }
                else
                {
                    // Default stream content type
                    content.Add(Constants.ApplicationOctetStreamMediaType, new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = JsonSchemaType.String,
                            Format = "binary"
                        }
                    });
                }
            }
            else
            {
                // Add the annotated request content media types
                if (_insertRestrictions?.RequestContentTypes is {} mediaTypes)
                {
                    foreach (string mediaType in mediaTypes)
                    {
                        content.Add(mediaType, new OpenApiMediaType
                        {
                            Schema = schema
                        });
                    }
                }
                else
                {
                    // Default content type
                    content.Add(Constants.ApplicationJsonMediaType, new OpenApiMediaType
                    {
                        Schema = schema
                    });
                }                
            }            

            return content;
        }

        /// <summary>
        /// Get the entity schema.
        /// </summary>
        /// <returns>The entity schema.</returns>
        private IOpenApiSchema? GetEntitySchema()
        {
            if (EntitySet is null) return null;
            return Context?.Settings.EnableDerivedTypesReferencesForRequestBody ?? false ?
                EdmModelHelper.GetDerivedTypesReferenceSchema(EntitySet.EntityType, Context.Model, _document) :
                new OpenApiSchemaReference(EntitySet.EntityType.FullName(), _document);
        }
    }
}
