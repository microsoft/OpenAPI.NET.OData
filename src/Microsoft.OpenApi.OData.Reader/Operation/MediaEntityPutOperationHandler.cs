// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
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
    internal class MediaEntityPutOperationHandler : MediaEntityOperationalHandler
    {
        /// <inheritdoc/>
        public override OperationType OperationType => OperationType.Put;
        private IEdmProperty _property = null;
        private UpdateRestrictionsType _updateRestrictions = null;

        protected override void Initialize(ODataContext context, ODataPath path)
        {
            base.Initialize(context, path);
            (_, _property) = GetStreamElements();
            
            if (_property != null) 
            {
                if (_property is IEdmNavigationProperty)
                {
                    _updateRestrictions = Context.Model.GetRecord<NavigationRestrictionsType>(_property, CapabilitiesConstants.NavigationRestrictions)?
                        .RestrictedProperties?.FirstOrDefault()?.UpdateRestrictions;
                }
                else
                {
                    _updateRestrictions = Context.Model.GetRecord<UpdateRestrictionsType>(_property, CapabilitiesConstants.UpdateRestrictions);
                }
            }            
        }

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            // Summary
            string placeholderValue = LastSegmentIsStreamPropertySegment ? Path.LastSegment.Identifier : "media content";
            operation.Summary = IsNavigationPropertyPath
                ? $"Update {placeholderValue} for the navigation property {NavigationProperty.Name} in {NavigationSource.Name}"
                : $"Update {placeholderValue} for {NavigationSourceSegment.EntityType.Name} in {NavigationSourceSegment.Identifier}";

            // Description
            if (LastSegmentIsStreamPropertySegment)
            {
                string description;

                if (_property is IEdmNavigationProperty)
                {

                    description = _updateRestrictions?.Description ?? Context.Model.GetDescriptionAnnotation(_property);
                }
                else
                {
                    // Structural property
                    description = Context.Model.GetDescriptionAnnotation(_property);
                }

                operation.Description = description;
            }

            // OperationId
            if (Context.Settings.EnableOperationId)
            {
                string identifier = LastSegmentIsStreamPropertySegment ? Path.LastSegment.Identifier : "Content";
                operation.OperationId = GetOperationId("Update", identifier);
            }
        }

        /// <inheritdoc/>
        protected override void SetRequestBody(OpenApiOperation operation)
        {
            operation.RequestBody = new OpenApiRequestBody
            {
                Required = true,
                Description = "New media content.",
                Content = GetContentDescription()
            };

            base.SetRequestBody(operation);
        }

        /// <inheritdoc/>
        protected override void SetResponses(OpenApiOperation operation)
        {
            if (LastSegmentIsStreamPropertySegment && Path.LastSegment.Identifier.Equals(Constants.Content, StringComparison.OrdinalIgnoreCase))
            {
                // Get the entity type declaring this stream property.
                (var entityType, _) = GetStreamElements();

                OpenApiSchema schema = new()
                {
                    UnresolvedReference = true,
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.Schema,
                        Id = entityType.FullName()
                    }
                };

                operation.AddErrorResponses(Context.Settings, addNoContent: true, schema: schema);
            }
            else
            {
                operation.AddErrorResponses(Context.Settings, true);
            }
            
            base.SetResponses(operation);
        }

        /// <inheritdoc/>
        protected override void SetSecurity(OpenApiOperation operation)
        {
            IEdmVocabularyAnnotatable annotatableNavigationSource = (IEdmVocabularyAnnotatable)NavigationSourceSegment.NavigationSource;
            UpdateRestrictionsType update = Context.Model.GetRecord<UpdateRestrictionsType>(annotatableNavigationSource, CapabilitiesConstants.UpdateRestrictions);
            if (update == null || update.Permissions == null)
            {
                return;
            }

            operation.Security = Context.CreateSecurityRequirements(update.Permissions).ToList();
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
    }
}
