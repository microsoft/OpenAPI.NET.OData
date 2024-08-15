// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

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
    /// Delete an Entity
    /// The Path Item Object for the entity set contains the keyword delete with an Operation Object as value
    /// that describes the capabilities for deleting the entity.
    /// </summary>
    internal class EntityDeleteOperationHandler : EntitySetOperationHandler
    {
        /// <inheritdoc/>
        public override OperationType OperationType => OperationType.Delete;

        private DeleteRestrictionsType _deleteRestrictions;

        protected override void Initialize(ODataContext context, ODataPath path)
        {
            base.Initialize(context, path);

            _deleteRestrictions = Context.Model.GetRecord<DeleteRestrictionsType>(TargetPath, CapabilitiesConstants.DeleteRestrictions);
            var entityDeleteRestrictions = Context.Model.GetRecord<DeleteRestrictionsType>(EntitySet, CapabilitiesConstants.DeleteRestrictions);
            _deleteRestrictions?.MergePropertiesIfNull(entityDeleteRestrictions);
            _deleteRestrictions ??= entityDeleteRestrictions;
        }

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            IEdmEntityType entityType = EntitySet.EntityType;
            ODataKeySegment keySegment = Path.LastSegment as ODataKeySegment;

            // Description
            string placeHolder = $"Delete entity from {EntitySet.Name}";
            if (keySegment.IsAlternateKey)
            {
                placeHolder = $"{placeHolder} by {keySegment.Identifier}";
            }
            operation.Summary = _deleteRestrictions?.Description ?? placeHolder;
            operation.Description = _deleteRestrictions?.LongDescription;

            // OperationId
            if (Context.Settings.EnableOperationId)
            {
                string typeName = entityType.Name;
                string operationName =$"Delete{Utils.UpperFirstChar(typeName)}";
                if (keySegment.IsAlternateKey)
                {
                    string alternateKeyName = string.Join("", keySegment.Identifier.Split(',').Select(static x => Utils.UpperFirstChar(x)));
                    operationName = $"{operationName}By{alternateKeyName}";
                }
                operation.OperationId =  $"{EntitySet.Name}.{typeName}.{operationName}";          
            }
        }

        /// <inheritdoc/>
        protected override void SetParameters(OpenApiOperation operation)
        {
            base.SetParameters(operation);

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "If-Match",
                In = ParameterLocation.Header,
                Description = "ETag",
                Schema = new OpenApiSchema
                {
                    Type = "string"
                }
            });
        }

        /// <inheritdoc/>
        protected override void SetResponses(OpenApiOperation operation)
        {
            // Response for Delete methods should be 204 No Content
            OpenApiConvertSettings settings = Context.Settings.Clone();
            settings.UseSuccessStatusCodeRange = false;
            
            operation.AddErrorResponses(settings, true);
            base.SetResponses(operation);
        }

        protected override void SetSecurity(OpenApiOperation operation)
        {
            if (_deleteRestrictions == null || _deleteRestrictions.Permissions == null)
            {
                return;
            }

            operation.Security = Context.CreateSecurityRequirements(_deleteRestrictions.Permissions).ToList();
        }

        protected override void AppendCustomParameters(OpenApiOperation operation)
        {
            if (_deleteRestrictions == null)
            {
                return;
            }

            if (_deleteRestrictions.CustomHeaders != null)
            {
                AppendCustomParameters(operation, _deleteRestrictions.CustomHeaders, ParameterLocation.Header);
            }

            if (_deleteRestrictions.CustomQueryOptions != null)
            {
                AppendCustomParameters(operation, _deleteRestrictions.CustomQueryOptions, ParameterLocation.Query);
            }
        }
    }
}
