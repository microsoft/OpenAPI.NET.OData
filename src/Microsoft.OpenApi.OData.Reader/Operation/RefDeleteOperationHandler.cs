// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Generator;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;
using System.Linq;
using System.Net.Http;

namespace Microsoft.OpenApi.OData.Operation
{
    /// <summary>
    /// Delete a navigation property ref for a navigation source.
    /// </summary>
    internal class RefDeleteOperationHandler : NavigationPropertyOperationHandler
    {
        /// <summary>
        /// Initializes a new instance of <see cref="RefDeleteOperationHandler"/> class.
        /// </summary>
        /// <param name="document">The document to use to lookup references.</param>
        public RefDeleteOperationHandler(OpenApiDocument document) : base(document)
        {
            
        }
        /// <inheritdoc/>
        public override HttpMethod OperationType => HttpMethod.Delete;
        private DeleteRestrictionsType? _deleteRestriction;

        /// <inheritdoc/>
        protected override void Initialize(ODataContext context, ODataPath path)
        {
            base.Initialize(context, path);
            _deleteRestriction = GetRestrictionAnnotation(CapabilitiesConstants.DeleteRestrictions) as DeleteRestrictionsType;
        }

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            // Summary and Description
            string placeHolder = "Delete ref of navigation property " + NavigationProperty?.Name + " for " + NavigationSource?.Name;
            operation.Summary = _deleteRestriction?.Description ?? placeHolder;
            operation.Description = _deleteRestriction?.LongDescription;

            // OperationId
            if (Context is {Settings.EnableOperationId: true})
            {
                string prefix = "DeleteRef";
                var segments = GetOperationId()?.Split('.').ToList() ?? [];
                
                if (SecondLastSegmentIsKeySegment)
                {
                    segments[segments.Count - 1] = Utils.ToFirstCharacterLowerCase(segments[segments.Count - 1]);
                    var lastSegment = prefix + Utils.UpperFirstChar(NavigationProperty.ToEntityType().Name);
                    segments.Add(lastSegment);
                    operation.OperationId = string.Join(".", segments);
                }
                else
                {
                    var lastSegment = segments.LastOrDefault();
                    segments[segments.Count - 1] = prefix + lastSegment;
                    operation.OperationId = string.Join(".", segments);
                }
            }            
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

            // for collection, we should have @id in query
            if (NavigationProperty.TargetMultiplicity() == EdmMultiplicity.Many &&
                Path?.Segments.Reverse().Skip(1).First() is ODataNavigationPropertySegment)
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "@id",
                    In = ParameterLocation.Query,
                    Description = "The delete Uri",
                    Required = true,
                    Schema = new OpenApiSchema
                    {
                        Type = JsonSchemaType.String
                    }
                });
            }
        }

        /// <inheritdoc/>
        protected override void SetSecurity(OpenApiOperation operation)
        {
            if (_deleteRestriction?.Permissions == null)
            {
                return;
            }

            operation.Security = Context?.CreateSecurityRequirements(_deleteRestriction.Permissions, _document).ToList();
        }

        /// <inheritdoc/>
        protected override void SetResponses(OpenApiOperation operation)
        {
            // Response for Delete methods should be 204 No Content
            var settings = Context?.Settings.Clone() ?? new();
            settings.UseSuccessStatusCodeRange = false;
            
            operation.AddErrorResponses(settings, _document, true);
            base.SetResponses(operation);
        }

        protected override void AppendCustomParameters(OpenApiOperation operation)
        {
            if (_deleteRestriction == null)
            {
                return;
            }

            if (_deleteRestriction.CustomHeaders != null)
            {
                AppendCustomParameters(operation, _deleteRestriction.CustomHeaders, ParameterLocation.Header);
            }

            if (_deleteRestriction.CustomQueryOptions != null)
            {
                AppendCustomParameters(operation, _deleteRestriction.CustomQueryOptions, ParameterLocation.Query);
            }
        }
    }
}
