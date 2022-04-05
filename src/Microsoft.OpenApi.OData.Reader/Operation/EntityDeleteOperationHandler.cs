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

        /// <summary>
        /// Gets/Sets the <see cref="DeleteRestrictionsType"/>
        /// </summary>
        private DeleteRestrictionsType DeleteRestrictions { get; set; }

        protected override void Initialize(ODataContext context, ODataPath path)
        {
            base.Initialize(context, path);

            DeleteRestrictions = Context.Model.GetRecord<DeleteRestrictionsType>(EntitySet, CapabilitiesConstants.DeleteRestrictions);
        }

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            IEdmEntityType entityType = EntitySet.EntityType();

            // Description
            string placeHolder = "Delete entity from " + EntitySet.Name;
            operation.Summary = DeleteRestrictions?.Description ?? placeHolder;
            operation.Description = DeleteRestrictions?.LongDescription ?? placeHolder;

            // OperationId
            if (Context.Settings.EnableOperationId)
            {
                string typeName = entityType.Name;
                operation.OperationId = EntitySet.Name + "." + typeName + ".Delete" + Utils.UpperFirstChar(typeName);
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
            operation.AddErrorResponses(Context.Settings, true);
            base.SetResponses(operation);
        }

        protected override void SetSecurity(OpenApiOperation operation)
        {
            if (DeleteRestrictions == null || DeleteRestrictions.Permissions == null)
            {
                return;
            }

            operation.Security = Context.CreateSecurityRequirements(DeleteRestrictions.Permissions).ToList();
        }

        protected override void AppendCustomParameters(OpenApiOperation operation)
        {
            if (DeleteRestrictions == null)
            {
                return;
            }

            if (DeleteRestrictions.CustomHeaders != null)
            {
                AppendCustomParameters(operation, DeleteRestrictions.CustomHeaders, ParameterLocation.Header);
            }

            if (DeleteRestrictions.CustomQueryOptions != null)
            {
                AppendCustomParameters(operation, DeleteRestrictions.CustomQueryOptions, ParameterLocation.Query);
            }
        }
    }
}
