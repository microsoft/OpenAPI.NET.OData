// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Generator;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;
using System.Linq;

namespace Microsoft.OpenApi.OData.Operation
{
    /// <summary>
    /// Retrieve a media content for an Entity
    /// </summary>
    internal class MediaEntityGetOperationHandler : MediaEntityOperationalHandler
    {
        /// <inheritdoc/>
        public override OperationType OperationType => OperationType.Get;
        private IEdmProperty _property = null;
        private ReadRestrictionsType _readRestrictions = null;

        protected override void Initialize(ODataContext context, ODataPath path)
        {
            base.Initialize(context, path);
            (_, _property) = GetStreamElements();

            if (_property != null)
            {
                if (_property is IEdmNavigationProperty)
                {
                    _readRestrictions = Context.Model.GetRecord<NavigationRestrictionsType>(_property, CapabilitiesConstants.NavigationRestrictions)?
                        .RestrictedProperties?.FirstOrDefault()?.ReadRestrictions;
                }
                else
                {
                    _readRestrictions = Context.Model.GetRecord<ReadRestrictionsType>(_property, CapabilitiesConstants.ReadRestrictions);
                }
            }            
        }

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            // Summary
            string placeholderValue = LastSegmentIsStreamPropertySegment ? Path.LastSegment.Identifier : "media content";
            operation.Summary = IsNavigationPropertyPath
                ? $"Get {placeholderValue} for the navigation property {NavigationProperty.Name} from {NavigationSource.Name}"
                : $"Get {placeholderValue} for {NavigationSourceSegment.EntityType.Name} from {NavigationSourceSegment.Identifier}";

            // Description
            if (LastSegmentIsStreamPropertySegment)
            {
                string description;

                if (_property is IEdmNavigationProperty)
                {
                    description = LastSegmentIsKeySegment
                        ? _readRestrictions?.ReadByKeyRestrictions?.Description
                        : _readRestrictions?.Description
                        ?? Context.Model.GetDescriptionAnnotation(_property);
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
                operation.OperationId = GetOperationId("Get", identifier);
            }
        }

        /// <inheritdoc/>
        protected override void SetResponses(OpenApiOperation operation)
        {
            operation.Responses = new OpenApiResponses
            {
                {
                    Context.Settings.UseSuccessStatusCodeRange ? Constants.StatusCodeClass2XX : Constants.StatusCode200,
                    new OpenApiResponse
                    {
                        Description = "Retrieved media content",
                        Content = GetContentDescription()
                    }
                }
            };
            operation.AddErrorResponses(Context.Settings, false);

            base.SetResponses(operation);
        }

        /// <inheritdoc/>
        protected override void SetSecurity(OpenApiOperation operation)
        {
            IEdmVocabularyAnnotatable annotatableNavigationSource = (IEdmVocabularyAnnotatable)NavigationSourceSegment.NavigationSource;
            ReadRestrictionsType read = Context.Model.GetRecord<ReadRestrictionsType>(annotatableNavigationSource, CapabilitiesConstants.ReadRestrictions);
            if (read == null)
            {
                return;
            }

            ReadRestrictionsBase readBase = read;
            if (read.ReadByKeyRestrictions != null)
            {
                readBase = read.ReadByKeyRestrictions;
            }

            if (readBase == null && readBase.Permissions == null)
            {
                return;
            }

            operation.Security = Context.CreateSecurityRequirements(readBase.Permissions).ToList();
        }

        /// <inheritdoc/>
        protected override void AppendCustomParameters(OpenApiOperation operation)
        {
            if (_readRestrictions == null)
            {
                return;
            }

            if (_readRestrictions.CustomHeaders != null)
            {
                AppendCustomParameters(operation, _readRestrictions.CustomHeaders, ParameterLocation.Header);
            }

            if (_readRestrictions.CustomQueryOptions != null)
            {
                AppendCustomParameters(operation, _readRestrictions.CustomQueryOptions, ParameterLocation.Query);
            }
        }
    }
}
