// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.References;
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
        /// <summary>
        /// Initializes a new instance of <see cref="MediaEntityPutOperationHandler"/> class.
        /// </summary>
        /// <param name="document">The document to use to lookup references.</param>
        public MediaEntityPutOperationHandler(OpenApiDocument document):base(document)
        {
            
        }
        /// <inheritdoc/>
        public override OperationType OperationType => OperationType.Put;
        
        private UpdateRestrictionsType _updateRestrictions = null;

        protected override void Initialize(ODataContext context, ODataPath path)
        {
            base.Initialize(context, path);

            if (Property != null)
            {
                _updateRestrictions = Context.Model.GetRecord<UpdateRestrictionsType>(TargetPath, CapabilitiesConstants.UpdateRestrictions);
                if (Property is IEdmNavigationProperty)
                {
                    var navigationUpdateRestrictions = Context.Model.GetRecord<NavigationRestrictionsType>(Property, CapabilitiesConstants.NavigationRestrictions)?
                            .RestrictedProperties?.FirstOrDefault()?.UpdateRestrictions;
                    _updateRestrictions?.MergePropertiesIfNull(navigationUpdateRestrictions);
                    _updateRestrictions ??= navigationUpdateRestrictions;
                }
                else
                {
                    var propertyUpdateRestrictions = Context.Model.GetRecord<UpdateRestrictionsType>(Property, CapabilitiesConstants.UpdateRestrictions);
                    _updateRestrictions?.MergePropertiesIfNull(propertyUpdateRestrictions);
                    _updateRestrictions ??= propertyUpdateRestrictions;
                }
            }
        }

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            // Summary
            string placeholderValue = LastSegmentIsStreamPropertySegment ? Path.LastSegment.Identifier : "media content";
            operation.Summary = _updateRestrictions?.Description;
            operation.Summary ??= IsNavigationPropertyPath
                ? $"Update {placeholderValue} for the navigation property {NavigationProperty.Name} in {NavigationSourceSegment.NavigationSource.Name}"
                : $"Update {placeholderValue} for {NavigationSourceSegment.EntityType.Name} in {NavigationSourceSegment.Identifier}";

            // Description
             operation.Description = _updateRestrictions?.LongDescription ?? Context.Model.GetDescriptionAnnotation(Property);

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

                OpenApiSchema schema = new OpenApiSchemaReference(entityType.FullName(), _document);

                operation.AddErrorResponses(Context.Settings, _document, addNoContent: true, schema: schema);
            }
            else
            {
                operation.AddErrorResponses(Context.Settings, _document, true);
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

            operation.Security = Context.CreateSecurityRequirements(update.Permissions, _document).ToList();
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
