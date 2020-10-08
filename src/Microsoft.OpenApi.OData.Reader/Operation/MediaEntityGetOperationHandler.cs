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

namespace Microsoft.OpenApi.OData.Operation
{
    /// <summary>
    /// Retrieve a media content for an Entity
    /// </summary>
    internal class MediaEntityGetOperationHandler : MediaEntityOperationalHandler
    {
        /// <inheritdoc/>
        public override OperationType OperationType => OperationType.Get;

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            // Summary
            if (IsNavigationPropertyPath)
            {
                operation.Summary = $"Get media content for the navigation property {NavigationProperty.Name} from {NavigationSource.Name}";
            }
            else
            {
                string typeName = EntitySet.EntityType().Name;
                operation.Summary = $"Get media content for {typeName} from {EntitySet.Name}";
            }

            // OperationId
            if (Context.Settings.EnableOperationId)
            {
                string identifier = Path.LastSegment.Kind == ODataSegmentKind.StreamContent ? "Content" : Path.LastSegment.Identifier;
                operation.OperationId = GetOperationId("Get", identifier);
            }

            base.SetBasicInfo(operation);
        }

        /// <inheritdoc/>
        protected override void SetResponses(OpenApiOperation operation)
        {
            operation.Responses = new OpenApiResponses
            {
                {
                    Constants.StatusCode200,
                    new OpenApiResponse
                    {
                        Description = "Retrieved media content",
                        Content = GetContentDescription()
                    }
                }
            };
            operation.Responses.Add(Constants.StatusCodeDefault, Constants.StatusCodeDefault.GetResponse());

            base.SetResponses(operation);
        }
        /// <inheritdoc/>
        protected override void SetSecurity(OpenApiOperation operation)
        {
            ReadRestrictionsType read = EntitySet != null
                ? Context.Model.GetRecord<ReadRestrictionsType>(EntitySet, CapabilitiesConstants.ReadRestrictions)
                : Context.Model.GetRecord<ReadRestrictionsType>(Singleton, CapabilitiesConstants.ReadRestrictions);
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
    }
}
