// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Linq;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.References;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Generator;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;

namespace Microsoft.OpenApi.OData.Operation
{
    /// <summary>
    /// Update a navigation property ref for a navigation source.
    /// </summary>
    internal class RefPutOperationHandler : NavigationPropertyOperationHandler
    {
        /// <summary>
        /// Initializes a new instance of <see cref="RefPutOperationHandler"/> class.
        /// </summary>
        /// <param name="document">The document to use to lookup references.</param>
        public RefPutOperationHandler(OpenApiDocument document) : base(document)
        {
            
        }
        /// <inheritdoc/>
        public override OperationType OperationType => OperationType.Patch;
        private UpdateRestrictionsType _updateRestriction;

        /// <inheritdoc/>
        protected override void Initialize(ODataContext context, ODataPath path)
        {
            base.Initialize(context, path);
            _updateRestriction = GetRestrictionAnnotation(CapabilitiesConstants.UpdateRestrictions) as UpdateRestrictionsType;
        }

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            // Summary and Description
            string placeHolder = "Update the ref of navigation property " + NavigationProperty.Name + " in " + NavigationSource.Name;
            operation.Summary = _updateRestriction?.Description ?? placeHolder;
            operation.Description = _updateRestriction?.LongDescription;

            // OperationId
            if (Context.Settings.EnableOperationId)
            {
                string prefix = "UpdateRef";
                operation.OperationId = GetOperationId(prefix);
            }
        }

        /// <inheritdoc/>
        protected override void SetRequestBody(OpenApiOperation operation)
        {
            operation.RequestBody = new OpenApiRequestBodyReference(Constants.ReferencePutRequestBodyName, null);

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

            operation.AddErrorResponses(Context.Settings, false);
            base.SetResponses(operation);
        }

        protected override void SetSecurity(OpenApiOperation operation)
        {
            if (_updateRestriction == null)
            {
                return;
            }

            operation.Security = Context.CreateSecurityRequirements(_updateRestriction.Permissions).ToList();
        }

        protected override void AppendCustomParameters(OpenApiOperation operation)
        {
            if (_updateRestriction == null)
            {
                return;
            }

            if (_updateRestriction.CustomHeaders != null)
            {
                AppendCustomParameters(operation, _updateRestriction.CustomHeaders, ParameterLocation.Header);
            }

            if (_updateRestriction.CustomQueryOptions != null)
            {
                AppendCustomParameters(operation, _updateRestriction.CustomQueryOptions, ParameterLocation.Query);
            }
        }
    }
}
