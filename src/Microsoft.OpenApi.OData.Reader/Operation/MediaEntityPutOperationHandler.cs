// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

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

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            // Summary
            if (IsNavigationPropertyPath)
            {
                operation.Summary = $"Update media content for the navigation property {NavigationProperty.Name} in {NavigationSource.Name}";
            }
            else
            {
                IEdmEntityType entityType = EntitySet.EntityType();
                operation.Summary = $"Update media content for {entityType.Name} in {EntitySet.Name}";
            }

            // Description
            IEdmVocabularyAnnotatable annotatableElement = GetAnnotatableElement();
            if (annotatableElement != null)
            {
                operation.Description = Context.Model.GetDescriptionAnnotation(annotatableElement);
            }

            // OperationId
            if (Context.Settings.EnableOperationId)
            {
                string identifier = Path.LastSegment.Kind == ODataSegmentKind.StreamContent ? "Content" : Path.LastSegment.Identifier;
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
            operation.Responses = new OpenApiResponses
            {
                { Constants.StatusCode204, Constants.StatusCode204.GetResponse() },
                { Constants.StatusCodeDefault, Constants.StatusCodeDefault.GetResponse() }
            };

            base.SetResponses(operation);
        }

        /// <inheritdoc/>
        protected override void SetSecurity(OpenApiOperation operation)
        {
            UpdateRestrictionsType update = EntitySet != null
                ? Context.Model.GetRecord<UpdateRestrictionsType>(EntitySet, CapabilitiesConstants.UpdateRestrictions)
                : Context.Model.GetRecord<UpdateRestrictionsType>(Singleton, CapabilitiesConstants.UpdateRestrictions);
            if (update == null || update.Permissions == null)
            {
                return;
            }

            operation.Security = Context.CreateSecurityRequirements(update.Permissions).ToList();
        }
    }
}
