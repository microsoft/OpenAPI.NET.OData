﻿using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Generator;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;
using System.Linq;
using System.Net.Http;

namespace Microsoft.OpenApi.OData.Operation
{
    internal class MediaEntityDeleteOperationHandler : MediaEntityOperationalHandler
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MediaEntityDeleteOperationHandler"/> class.
        /// </summary>
        /// <param name="document">The document to use to lookup references.</param>
        public MediaEntityDeleteOperationHandler(OpenApiDocument document) : base(document)
        {
            
        }
        /// <inheritdoc/>
        public override HttpMethod OperationType => HttpMethod.Delete;

        private DeleteRestrictionsType? _deleteRestrictions;

        protected override void Initialize(ODataContext context, ODataPath path)
        {
            base.Initialize(context, path);

            if (Property != null)
            {
                if (!string.IsNullOrEmpty(TargetPath))
                    _deleteRestrictions = Context?.Model.GetRecord<DeleteRestrictionsType>(TargetPath, CapabilitiesConstants.DeleteRestrictions);
                if (Property is IEdmNavigationProperty)
                {
                    var navigationDeleteRestrictions = Context?.Model.GetRecord<NavigationRestrictionsType>(Property, CapabilitiesConstants.NavigationRestrictions)?
                            .RestrictedProperties?.FirstOrDefault()?.DeleteRestrictions;
                    _deleteRestrictions?.MergePropertiesIfNull(navigationDeleteRestrictions);
                    _deleteRestrictions ??= navigationDeleteRestrictions;
                }
                else
                {
                    var propertyDeleteRestrictions = Context?.Model.GetRecord<DeleteRestrictionsType>(Property, CapabilitiesConstants.DeleteRestrictions);
                    _deleteRestrictions?.MergePropertiesIfNull(propertyDeleteRestrictions);
                    _deleteRestrictions ??= propertyDeleteRestrictions;
                }
            }
        }

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            // Summary
            string placeholderValue = LastSegmentIsStreamPropertySegment && Path is {LastSegment.Identifier: not null} ? Path.LastSegment.Identifier : "media content";
            operation.Summary = _deleteRestrictions?.Description;
            operation.Summary ??= IsNavigationPropertyPath
                ? $"Delete {placeholderValue} for the navigation property {NavigationProperty?.Name} in {NavigationSourceSegment?.NavigationSource.Name}"
                : $"Delete {placeholderValue} for {NavigationSourceSegment?.EntityType.Name} in {NavigationSourceSegment?.Identifier}";

            // Description
            operation.Description = _deleteRestrictions?.LongDescription ?? Context?.Model.GetDescriptionAnnotation(Property);

            // OperationId
            if (Context is {Settings.EnableOperationId: true})
            {
                string identifier = LastSegmentIsStreamPropertySegment && Path is {LastSegment.Identifier: not null} ? Path.LastSegment.Identifier : "Content";
                operation.OperationId = GetOperationId("Delete", identifier);
            }

            base.SetBasicInfo(operation);
        }

        /// <inheritdoc/>
        protected override void SetParameters(OpenApiOperation operation)
        {
            base.SetParameters(operation);

            operation.Parameters ??= [];
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "If-Match",
                In = ParameterLocation.Header,
                Description = "ETag",
                Schema = new OpenApiSchema
                {
                    Type = JsonSchemaType.String
                }
            });
        }

        /// <inheritdoc/>
        protected override void SetResponses(OpenApiOperation operation)
        {
            // Response for Delete methods should be 204 No Content
            OpenApiConvertSettings settings = Context?.Settings.Clone() ?? new();
            settings.UseSuccessStatusCodeRange = false;

            operation.AddErrorResponses(settings, _document, true);
            base.SetResponses(operation);
        }

        protected override void SetSecurity(OpenApiOperation operation)
        {
            if (_deleteRestrictions == null || _deleteRestrictions.Permissions == null)
            {
                return;
            }

            operation.Security = Context?.CreateSecurityRequirements(_deleteRestrictions.Permissions, _document).ToList() ?? [];
        }

        /// <inheritdoc/>
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
