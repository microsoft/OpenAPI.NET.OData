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
    /// Create a navigation for a navigation source.
    /// The Path Item Object for the entity set contains the keyword delete with an Operation Object as value
    /// that describes the capabilities for create a navigation for a navigation source.
    /// </summary>
    internal class NavigationPropertyPostOperationHandler : NavigationPropertyOperationHandler
    {
        /// <inheritdoc/>
        public override OperationType OperationType => OperationType.Post;

        private InsertRestrictionsType _insertRestriction;

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
            string placeHolder = "Create new navigation property to " + NavigationProperty.Name + " for " + NavigationSource.Name;
            operation.Summary = _insertRestriction?.Description ?? placeHolder;
            operation.Description = _insertRestriction?.LongDescription;

            // OperationId
            if (Context.Settings.EnableOperationId)
            {
                string prefix = "Create";
                operation.OperationId = GetOperationId(prefix);
            }

            base.SetBasicInfo(operation);
        }

        /// <inheritdoc/>
        protected override void SetRequestBody(OpenApiOperation operation)
        {
            OpenApiSchema schema = null;

            if (Context.Settings.EnableDerivedTypesReferencesForRequestBody)
            {
                schema = EdmModelHelper.GetDerivedTypesReferenceSchema(NavigationProperty.ToEntityType(), Context.Model);
            }

            operation.RequestBody = new OpenApiRequestBody
            {
                Required = true,
                Description = "New navigation property",
                Content = GetContent(schema, _insertRestriction?.RequestContentTypes)
            };

            base.SetRequestBody(operation);
        }

        /// <inheritdoc/>
        protected override void SetResponses(OpenApiOperation operation)
        {
            OpenApiSchema schema = null;

            if (Context.Settings.EnableDerivedTypesReferencesForResponses)
            {
                schema = EdmModelHelper.GetDerivedTypesReferenceSchema(NavigationProperty.ToEntityType(), Context.Model);
            }

            operation.Responses = new OpenApiResponses
            {
                {
                    Context.Settings.UseSuccessStatusCodeRange ? Constants.StatusCodeClass2XX : Constants.StatusCode201,
                    new OpenApiResponse
                    {
                        Description = "Created navigation property.",
                        Content = GetContent(schema, _insertRestriction?.ResponseContentTypes)
                    }
                }
            };
            operation.AddErrorResponses(Context.Settings, false);

            base.SetResponses(operation);
        }

        protected override void SetSecurity(OpenApiOperation operation)
        {
            if (_insertRestriction == null)
            {
                return;
            }

            operation.Security = Context.CreateSecurityRequirements(_insertRestriction.Permissions).ToList();
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
