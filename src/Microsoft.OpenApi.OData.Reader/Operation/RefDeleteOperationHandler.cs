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
    /// Delete a navigation property ref for a navigation source.
    /// </summary>
    internal class RefDeleteOperationHandler : NavigationPropertyOperationHandler
    {
        /// <inheritdoc/>
        public override OperationType OperationType => OperationType.Delete;
        private DeleteRestrictionsType _deleteRestriction;

        /// <inheritdoc/>
        protected override void Initialize(ODataContext context, ODataPath path)
        {
            base.Initialize(context, path);
            _deleteRestriction = Restriction?.DeleteRestrictions ??
                Context.Model.GetRecord<DeleteRestrictionsType>(NavigationProperty, CapabilitiesConstants.DeleteRestrictions);
        }

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            // Summary and Description
            string placeHolder = "Delete ref of navigation property " + NavigationProperty.Name + " for " + NavigationSource.Name;
            operation.Summary = _deleteRestriction?.Description ?? placeHolder;
            operation.Description = _deleteRestriction?.LongDescription;

            // OperationId
            if (Context.Settings.EnableOperationId)
            {
                string prefix = "DeleteRef";
                operation.OperationId = GetOperationId(prefix);
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

            // for collection, we should have @id in query
            if (NavigationProperty.TargetMultiplicity() == EdmMultiplicity.Many)
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "@id",
                    In = ParameterLocation.Query,
                    Description = "Delete Uri",
                    Schema = new OpenApiSchema
                    {
                        Type = "string"
                    }
                });
            }
        }

        /// <inheritdoc/>
        protected override void SetSecurity(OpenApiOperation operation)
        {
            if (_deleteRestriction == null)
            {
                return;
            }

            operation.Security = Context.CreateSecurityRequirements(_deleteRestriction.Permissions).ToList();
        }

        /// <inheritdoc/>
        protected override void SetResponses(OpenApiOperation operation)
        {
    		operation.AddErrorResponses(Context.Settings, true);
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
