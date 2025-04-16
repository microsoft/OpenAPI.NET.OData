// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.Interfaces;
using Microsoft.OpenApi.Models.References;
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
        /// <summary>
        /// Initializes a new instance of <see cref="EntityUpdateOperationHandler"/> class.
        /// </summary>
        /// <param name="document">The document to use to lookup references.</param>
        protected EntityUpdateOperationHandler(OpenApiDocument document):base(document)
        {
            
        }
        private UpdateRestrictionsType? _updateRestrictions;

        protected override void Initialize(ODataContext context, ODataPath path)
        {
            base.Initialize(context, path);

            if (!string.IsNullOrEmpty(TargetPath))
                _updateRestrictions = Context?.Model.GetRecord<UpdateRestrictionsType>(TargetPath, CapabilitiesConstants.UpdateRestrictions);
            if (Context is not null && EntitySet is not null)
            {
                var entityUpdateRestrictions = Context.Model.GetRecord<UpdateRestrictionsType>(EntitySet, CapabilitiesConstants.UpdateRestrictions);
                _updateRestrictions?.MergePropertiesIfNull(entityUpdateRestrictions);
                _updateRestrictions ??= entityUpdateRestrictions;
            }
        }

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            var keySegment = Path?.LastSegment as ODataKeySegment;

            // Summary and Description
            string placeHolder = "Update entity in " + EntitySet?.Name;
            if (keySegment is {IsAlternateKey: true})
            {
                placeHolder = $"{placeHolder} by {keySegment.Identifier}";
            }
            operation.Summary = _updateRestrictions?.Description ?? placeHolder;
            operation.Description = _updateRestrictions?.LongDescription;

            // OperationId
            if (Context is {Settings.EnableOperationId: true} && EntitySet?.EntityType is {} entityType)
            {
                string typeName = entityType.Name;
                string prefix = OperationType == HttpMethod.Patch ? "Update" : "Set";
                string operationName = $"{prefix}{ Utils.UpperFirstChar(typeName)}";
                if (keySegment is {IsAlternateKey: true})
                {
                    string alternateKeyName = string.Join("", keySegment.Identifier.Split(',').Select(static x => Utils.UpperFirstChar(x)));
                    operationName = $"{operationName}By{alternateKeyName}";
                }
                operation.OperationId = $"{EntitySet.Name}.{typeName}.{operationName}";
            }
        }

        /// <inheritdoc/>
        protected override void SetRequestBody(OpenApiOperation operation)
        {
            operation.RequestBody = new OpenApiRequestBody
            {
                Required = true,
                Description = "New property values",
                Content = GetContent()
            };

            base.SetRequestBody(operation);
        }

        protected Dictionary<string, OpenApiMediaType> GetContent()
        {
            var schema = GetOpenApiSchema();
            var content = new Dictionary<string, OpenApiMediaType>();

            // Add the annotated request content media types
            if (_updateRestrictions?.RequestContentTypes is {} mediaTypes)
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

            return content;
        }

        /// <inheritdoc/>
        protected override void SetResponses(OpenApiOperation operation)
        {
            if (GetOpenApiSchema() is {} schema)
                operation.AddErrorResponses(Context?.Settings ?? new(), _document, true, schema);
            base.SetResponses(operation);
        }

        protected override void SetSecurity(OpenApiOperation operation)
        {
            if (_updateRestrictions?.Permissions == null)
            {
                return;
            }

            operation.Security = Context?.CreateSecurityRequirements(_updateRestrictions.Permissions, _document).ToList();
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

        private IOpenApiSchema? GetOpenApiSchema()
        {
            if (EntitySet is null) return null;
            if (Context is {Settings.EnableDerivedTypesReferencesForRequestBody: true} &&
                EdmModelHelper.GetDerivedTypesReferenceSchema(EntitySet.EntityType, Context.Model, _document) is {} schema)
            {
                return schema;
            }

            return new OpenApiSchemaReference(EntitySet.EntityType.FullName(), _document);
        }
    }
}
