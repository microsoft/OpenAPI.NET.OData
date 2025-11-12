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

namespace Microsoft.OpenApi.OData.Operation
{
    /// <summary>
    /// Update a Singleton
    /// The Path Item Object for the entity set contains the keyword patch with an Operation Object as value
    /// that describes the capabilities for updating the singleton, unless the singleton is read-only.
    /// </summary>
    internal class SingletonPatchOperationHandler : SingletonOperationHandler
    {
        /// <summary>
        /// Initializes a new instance of <see cref="SingletonPatchOperationHandler"/> class.
        /// </summary>
        /// <param name="document">The document to use to lookup references.</param>
        public SingletonPatchOperationHandler(OpenApiDocument document) : base(document)
        {
            
        }
        /// <inheritdoc/>
        public override HttpMethod OperationType => HttpMethod.Patch;

        private UpdateRestrictionsType? _updateRestrictions;

        protected override void Initialize(ODataContext context, ODataPath path)
        {
            base.Initialize(context, path);

            if (!string.IsNullOrEmpty(TargetPath))
                _updateRestrictions = Context?.Model.GetRecord<UpdateRestrictionsType>(TargetPath, CapabilitiesConstants.UpdateRestrictions);

            if (Context is not null && Singleton is not null)
            {
                var singletonUpdateRestrictions = Context.Model.GetRecord<UpdateRestrictionsType>(Singleton, CapabilitiesConstants.UpdateRestrictions);
                _updateRestrictions?.MergePropertiesIfNull(singletonUpdateRestrictions);
                _updateRestrictions ??= singletonUpdateRestrictions;
            }
        }

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            // Summary and Descriptions
            string placeHolder = "Update " + Singleton?.Name;
            operation.Summary = _updateRestrictions?.Description ?? placeHolder;
            operation.Description = _updateRestrictions?.LongDescription;

            // OperationId
            if (Context is {Settings.EnableOperationId: true} && Singleton is not null)
            {
                string typeName = Singleton.EntityType.Name;
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
                Content = new Dictionary<string, IOpenApiMediaType>
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
            if (Context is not null && GetOpenApiSchema() is {} schema)
                operation.AddErrorResponses(Context.Settings, _document, true, schema);
            base.SetResponses(operation);
        }

        /// <inheritdoc/>
        protected override void SetSecurity(OpenApiOperation operation)
        {
            if (_updateRestrictions?.Permissions == null)
            {
                return;
            }

            operation.Security = Context?.CreateSecurityRequirements(_updateRestrictions.Permissions, _document).ToList();
        }

        /// <inheritdoc/>
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
            if (Singleton is null) return null;
            return Context is {Settings.EnableDerivedTypesReferencesForRequestBody: true} ?
                EdmModelHelper.GetDerivedTypesReferenceSchema(Singleton.EntityType, Context.Model, _document) :
                new OpenApiSchemaReference(Singleton.EntityType.FullName(), _document);
        }
    }
}
