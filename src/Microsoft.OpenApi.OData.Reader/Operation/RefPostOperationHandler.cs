// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Linq;
using System.Net.Http;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Generator;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;

namespace Microsoft.OpenApi.OData.Operation
{
    /// <summary>
    /// Create a navigation property ref for a navigation source.
    /// </summary>
    internal class RefPostOperationHandler : NavigationPropertyOperationHandler
    {
        /// <summary>
        /// Initializes a new instance of <see cref="RefPostOperationHandler"/> class.
        /// </summary>
        /// <param name="document">The document to use to lookup references.</param>
        public RefPostOperationHandler(OpenApiDocument document) : base(document)
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
            string placeHolder = "Create new navigation property ref to " + NavigationProperty?.Name + " for " + NavigationSource?.Name;
            operation.Summary = _insertRestriction?.Description ?? placeHolder;
            operation.Description = _insertRestriction?.LongDescription;

            // OperationId
            if (Context is {Settings.EnableOperationId: true})
            {
                string prefix = "CreateRef";
                operation.OperationId = GetOperationId(prefix);
            }            
        }

        /// <inheritdoc/>
        protected override void SetRequestBody(OpenApiOperation operation)
        {
            operation.RequestBody = new OpenApiRequestBodyReference(Constants.ReferencePostRequestBodyName, _document);

            base.SetRequestBody(operation);
        }

        /// <inheritdoc/>
        protected override void SetResponses(OpenApiOperation operation)
        {
            operation.Responses = new OpenApiResponses
            {
                {
                    Constants.StatusCode204,
                    new OpenApiResponse { Description = "Success" } 
                }
            };

    		if (Context is not null)
                operation.AddErrorResponses(Context.Settings, _document, false);

            base.SetResponses(operation);
        }

        protected override void SetSecurity(OpenApiOperation operation)
        {
            if (_insertRestriction?.Permissions is null)
            {
                return;
            }

            operation.Security = Context?.CreateSecurityRequirements(_insertRestriction.Permissions, _document).ToList();
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
