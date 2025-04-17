// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Generator;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.Models.References;
using Microsoft.OpenApi.Models.Interfaces;
using System.Net.Http;

namespace Microsoft.OpenApi.OData.Operation
{
    /// <summary>
    /// Retrieve an Entity:
    /// The Path Item Object for the entity contains the keyword get with an Operation Object as value
    /// that describes the capabilities for retrieving a single entity.
    /// </summary>
    internal class EntityGetOperationHandler : EntitySetOperationHandler
    {
        /// <summary>
        /// Initializes a new instance of <see cref="EntityGetOperationHandler"/> class.
        /// </summary>
        /// <param name="document">The document to use to lookup references.</param>
        public EntityGetOperationHandler(OpenApiDocument document) : base(document)
        {
            
        }
        /// <inheritdoc/>
        public override HttpMethod OperationType => HttpMethod.Get;

        private ReadRestrictionsType? _readRestrictions;

        protected override void Initialize(ODataContext context, ODataPath path)
        {
            base.Initialize(context, path);

            if (!string.IsNullOrEmpty(TargetPath))
                _readRestrictions = Context?.Model.GetRecord<ReadRestrictionsType>(TargetPath, CapabilitiesConstants.ReadRestrictions);
            if (Context is not null && EntitySet is not null)
            {
                var entityReadRestrictions = Context.Model.GetRecord<ReadRestrictionsType>(EntitySet, CapabilitiesConstants.ReadRestrictions);
                _readRestrictions?.MergePropertiesIfNull(entityReadRestrictions);
                _readRestrictions ??= entityReadRestrictions;
            }
        }

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            var keySegment = Path?.LastSegment as ODataKeySegment;

            // Description
            string placeHolder = "Get entity from " + EntitySet?.Name + " by key";
            if (keySegment?.IsAlternateKey ?? false) 
            {
                placeHolder = $"{placeHolder} ({keySegment.Identifier})";
            }
            operation.Summary = _readRestrictions?.ReadByKeyRestrictions?.Description ?? placeHolder;
            operation.Description = _readRestrictions?.ReadByKeyRestrictions?.LongDescription ?? 
                                    (EntitySet is null ? null : Context?.Model.GetDescriptionAnnotation(EntitySet.EntityType));

            // OperationId
            if (Context is {Settings.EnableOperationId: true} && EntitySet?.EntityType.Name is string entityTypeName)
            { 
                string typeName = entityTypeName;
                string operationName = $"Get{Utils.UpperFirstChar(typeName)}";
                if (keySegment?.IsAlternateKey ?? false)
                {
                    string alternateKeyName = string.Join("", keySegment.Identifier.Split(',').Select(static x => Utils.UpperFirstChar(x)));
                    operationName = $"{operationName}By{alternateKeyName}";
                }              
                operation.OperationId = $"{EntitySet.Name}.{typeName}.{operationName}";
            }
        }

        /// <inheritdoc/>
        protected override void SetParameters(OpenApiOperation operation)
        {
            base.SetParameters(operation);

            if (Context is null || EntitySet is null) return;

            // $select
            if (Context.CreateSelect(EntitySet) is {} sParameter)
            {
                operation.Parameters ??= [];
                operation.Parameters.Add(sParameter);
            }

            // $expand
            if (Context.CreateExpand(EntitySet) is {} eParameter)
            {
                operation.Parameters ??= [];
                operation.Parameters.Add(eParameter);
            }
        }

        /// <inheritdoc/>
        protected override void SetResponses(OpenApiOperation operation)
        {
            IOpenApiSchema? schema = null;
            Dictionary<string, IOpenApiLink>? links = null;

            if (EntitySet is not null)
            {
                if (Context is {Settings.EnableDerivedTypesReferencesForResponses: true})
                {
                    schema = EdmModelHelper.GetDerivedTypesReferenceSchema(EntitySet.EntityType, Context.Model, _document);
                }

                if (Context is {Settings.ShowLinks: true} && Path is not null)
                {
                    links = Context.CreateLinks(entityType: EntitySet.EntityType, entityName: EntitySet.Name,
                            entityKind: EntitySet.ContainerElementKind.ToString(), path: Path, parameters: PathParameters);
                }

                schema ??= new OpenApiSchemaReference(EntitySet.EntityType.FullName(), _document);
            }

            operation.Responses = new OpenApiResponses
            {
                {
                    Context?.Settings.UseSuccessStatusCodeRange ?? false ? Constants.StatusCodeClass2XX : Constants.StatusCode200,
                    new OpenApiResponse
                    {
                        Description = "Retrieved entity",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            {
                                Constants.ApplicationJsonMediaType,
                                new OpenApiMediaType
                                {
                                    Schema = schema
                                }
                            }
                        },
                        Links = links
                    }
                }
            };
            if (Context is not null)
                operation.AddErrorResponses(Context.Settings, _document, false);

            base.SetResponses(operation);
        }

        protected override void SetSecurity(OpenApiOperation operation)
        {
            if (_readRestrictions == null)
            {
                return;
            }

            ReadRestrictionsBase readBase = _readRestrictions;
            if (_readRestrictions.ReadByKeyRestrictions != null)
            {
                readBase = _readRestrictions.ReadByKeyRestrictions;
            }

            if (readBase == null || readBase.Permissions == null)
            {
                return;
            }

            operation.Security = Context?.CreateSecurityRequirements(readBase.Permissions, _document).ToList();
        }

        protected override void AppendCustomParameters(OpenApiOperation operation)
        {
            if (_readRestrictions == null)
            {
                return;
            }

            ReadRestrictionsBase readBase = _readRestrictions;
            if (_readRestrictions.ReadByKeyRestrictions != null)
            {
                readBase = _readRestrictions.ReadByKeyRestrictions;
            }

            if (readBase.CustomHeaders != null)
            {
                AppendCustomParameters(operation, readBase.CustomHeaders, ParameterLocation.Header);
            }

            if (readBase.CustomQueryOptions != null)
            {
                AppendCustomParameters(operation, readBase.CustomQueryOptions, ParameterLocation.Query);
            }
        }
    }
}