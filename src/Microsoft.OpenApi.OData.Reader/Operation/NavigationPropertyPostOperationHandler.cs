// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

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
    /// Create a navigation for a navigation source.
    /// The Path Item Object for the entity set contains the keyword delete with an Operation Object as value
    /// that describes the capabilities for create a navigation for a navigation source.
    /// </summary>
    internal class NavigationPropertyPostOperationHandler : NavigationPropertyOperationHandler
    {
        /// <summary>
        /// Initializes a new instance of <see cref="NavigationPropertyPostOperationHandler"/> class.
        /// </summary>
        /// <param name="document">The document to use to lookup references.</param>
        public NavigationPropertyPostOperationHandler(OpenApiDocument document):base(document)
        {
            
        }
        /// <inheritdoc/>
        public override HttpMethod OperationType => HttpMethod.Post;

        private InsertRestrictionsType? _insertRestriction;

        /// <inheritdoc/>
        protected override void Initialize(ODataContext context, ODataPath path)
        {
            base.Initialize(context, path);
            _insertRestriction = GetRestrictionAnnotation(CapabilitiesConstants.InsertRestrictions) as InsertRestrictionsType;
        }

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            // Summary and Description
            string placeHolder = "Create new navigation property to " + NavigationProperty?.Name + " for " + NavigationSource?.Name;
            operation.Summary = _insertRestriction?.Description ?? placeHolder;
            operation.Description = _insertRestriction?.LongDescription;

            // OperationId
            if (Context is {Settings.EnableOperationId: true})
            {
                string prefix = "Create";
                operation.OperationId = GetOperationId(prefix);
            }

            base.SetBasicInfo(operation);
        }

        /// <inheritdoc/>
        protected override void SetRequestBody(OpenApiOperation operation)
        {
            var schema = Context is {Settings.EnableDerivedTypesReferencesForRequestBody: true} ?
                EdmModelHelper.GetDerivedTypesReferenceSchema(NavigationProperty.ToEntityType(), Context.Model, _document) :
                null;

            operation.RequestBody = new OpenApiRequestBody
            {
                Required = NavigationProperty?.ToEntityType() != null
                    ? OpenApiRequestBodyGenerator.DetermineIfRequestBodyRequired(
                        NavigationProperty.ToEntityType(),
                        isUpdateOperation: false,
                        Context?.Model)
                    : true,
                Description = "New navigation property",
                Content = GetContent(schema, _insertRestriction?.RequestContentTypes)
            };

            base.SetRequestBody(operation);
        }

        /// <inheritdoc/>
        protected override void SetResponses(OpenApiOperation operation)
        {
            var schema = Context is {Settings.EnableDerivedTypesReferencesForResponses: true} ? 
                EdmModelHelper.GetDerivedTypesReferenceSchema(NavigationProperty.ToEntityType(), Context.Model, _document) :
                null;

            operation.Responses = new OpenApiResponses
            {
                {
                    Context?.Settings.UseSuccessStatusCodeRange ?? false ? Constants.StatusCodeClass2XX : Constants.StatusCode201,
                    new OpenApiResponse
                    {
                        Description = "Created navigation property.",
                        Content = GetContent(schema, _insertRestriction?.ResponseContentTypes)
                    }
                }
            };
            if (Context is not null)
                operation.AddErrorResponses(Context.Settings, _document, false);

            base.SetResponses(operation);
        }

        protected override void SetSecurity(OpenApiOperation operation)
        {
            if (_insertRestriction == null)
            {
                return;
            }

            operation.Security = Context?.CreateSecurityRequirements(_insertRestriction.Permissions ?? [], _document).ToList() ?? [];
        }

        protected override void AppendCustomParameters(OpenApiOperation operation)
        {
            if (_insertRestriction == null)
            {
                return;
            }

            if (_insertRestriction.CustomHeaders != null)
            {
                AppendCustomParameters(operation, _insertRestriction.CustomHeaders, ParameterLocation.Header);
            }

            if (_insertRestriction.CustomQueryOptions != null)
            {
                AppendCustomParameters(operation, _insertRestriction.CustomQueryOptions, ParameterLocation.Query);
            }
        }
    }
}
