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

namespace Microsoft.OpenApi.OData.Operation
{
    /// <summary>
    /// Retrieve an Entity:
    /// The Path Item Object for the entity contains the keyword get with an Operation Object as value
    /// that describes the capabilities for retrieving a single entity.
    /// </summary>
    internal class EntityGetOperationHandler : EntitySetOperationHandler
    {
        /// <inheritdoc/>
        public override OperationType OperationType => OperationType.Get;

        private ReadRestrictionsType _readRestrictions;

        protected override void Initialize(ODataContext context, ODataPath path)
        {
            base.Initialize(context, path);

            _readRestrictions = Context.Model.GetRecord<ReadRestrictionsType>(TargetPath, CapabilitiesConstants.ReadRestrictions);
            var entityReadRestrictions = Context.Model.GetRecord<ReadRestrictionsType>(EntitySet, CapabilitiesConstants.ReadRestrictions);
            _readRestrictions?.MergePropertiesIfNull(entityReadRestrictions);
            _readRestrictions ??= entityReadRestrictions;
        }

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            IEdmEntityType entityType = EntitySet.EntityType;
            ODataKeySegment keySegment = Path.LastSegment as ODataKeySegment;

            // Description
            string placeHolder = "Get entity from " + EntitySet.Name + " by key";
            if (keySegment.IsAlternateKey) 
            {
                placeHolder = $"{placeHolder} ({keySegment.Identifier})";
            }
            operation.Summary = _readRestrictions?.ReadByKeyRestrictions?.Description ?? placeHolder;
            operation.Description = _readRestrictions?.ReadByKeyRestrictions?.LongDescription ?? Context.Model.GetDescriptionAnnotation(entityType);

            // OperationId
            if (Context.Settings.EnableOperationId)
            { 
                string typeName = entityType.Name;
                string operationName = $"Get{Utils.UpperFirstChar(typeName)}";
                if (keySegment.IsAlternateKey)
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

            // $select
            OpenApiParameter parameter = Context.CreateSelect(EntitySet);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }

            // $expand
            parameter = Context.CreateExpand(EntitySet);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }
        }

        /// <inheritdoc/>
        protected override void SetResponses(OpenApiOperation operation)
        {
            OpenApiSchema schema = null;
            IDictionary<string, OpenApiLink> links = null;

            if (Context.Settings.EnableDerivedTypesReferencesForResponses)
            {
                schema = EdmModelHelper.GetDerivedTypesReferenceSchema(EntitySet.EntityType, Context.Model);
            }

            if (Context.Settings.ShowLinks)
            {
                links = Context.CreateLinks(entityType: EntitySet.EntityType, entityName: EntitySet.Name,
                        entityKind: EntitySet.ContainerElementKind.ToString(), path: Path, parameters: PathParameters);
            }

            if (schema == null)
            {
                schema = new OpenApiSchema
                {
                    UnresolvedReference = true,
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.Schema,
                        Id = EntitySet.EntityType.FullName()
                    }
                };
            }

            operation.Responses = new OpenApiResponses
            {
                {
                    Context.Settings.UseSuccessStatusCodeRange ? Constants.StatusCodeClass2XX : Constants.StatusCode200,
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
            operation.AddErrorResponses(Context.Settings, false);

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

            if (readBase == null && readBase.Permissions == null)
            {
                return;
            }

            operation.Security = Context.CreateSecurityRequirements(readBase.Permissions).ToList();
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