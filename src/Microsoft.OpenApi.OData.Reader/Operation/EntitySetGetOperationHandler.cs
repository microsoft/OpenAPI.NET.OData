// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Generator;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;

namespace Microsoft.OpenApi.OData.Operation
{
    /// <summary>
    /// Query a Collection of Entities, it's a "Get" operation for <see cref="IEdmEntitySet"/>
    /// The Path Item Object for the entity set contains the keyword get with an Operation Object as value
    /// that describes the capabilities for querying the entity set.
    /// For example: "/users"
    /// </summary>
    internal class EntitySetGetOperationHandler : EntitySetOperationHandler
    {
        /// <inheritdoc/>
        public override OperationType OperationType => OperationType.Get;

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            // Summary
            operation.Summary = "Get entities from " + EntitySet.Name;

            // OperationId
            if (Context.Settings.EnableOperationId)
            {
                string typeName = EntitySet.EntityType().Name;
                operation.OperationId = EntitySet.Name + "." + typeName + ".List" + Utils.UpperFirstChar(typeName);
            }

            base.SetBasicInfo(operation);
        }

        protected override void SetExtensions(OpenApiOperation operation)
        {
            if (Context.Settings.EnablePagination)
            {
                OpenApiObject extension = new OpenApiObject
                {
                    { "nextLinkName", new OpenApiString("@odata.nextLink")},
                    { "operationName", new OpenApiString(Context.Settings.PageableOperationName)}
                };
                operation.Extensions.Add(Constants.xMsPageable, extension);

                base.SetExtensions(operation);
            }
        }

        /// <inheritdoc/>
        protected override void SetParameters(OpenApiOperation operation)
        {
            base.SetParameters(operation);

            // The parameters array contains Parameter Objects for all system query options allowed for this collection,
            // and it does not list system query options not allowed for this collection, see terms
            // Capabilities.TopSupported, Capabilities.SkipSupported, Capabilities.SearchRestrictions,
            // Capabilities.FilterRestrictions, and Capabilities.CountRestrictions
            // $top
            OpenApiParameter parameter = Context.CreateTop(EntitySet);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }

            // $skip
            parameter = Context.CreateSkip(EntitySet);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }

            // $search
            parameter = Context.CreateSearch(EntitySet);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }

            // $filter
            parameter = Context.CreateFilter(EntitySet);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }

            // $count
            parameter = Context.CreateCount(EntitySet);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }

            // the syntax of the system query options $expand, $select, and $orderby is too flexible
            // to be formally described with OpenAPI Specification means, yet the typical use cases
            // of just providing a comma-separated list of properties can be expressed via an array-valued
            // parameter with an enum constraint
            // $order
            parameter = Context.CreateOrderBy(EntitySet);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }

            // $select
            parameter = Context.CreateSelect(EntitySet);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }

            // $expand
            parameter = Context.CreateExpand(EntitySet);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }
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
                        Reference = new OpenApiReference()
                        {
                            Type = ReferenceType.Response,
                            Id = $"{EntitySet.EntityType().FullName()}{Constants.CollectionSchemaSuffix}"
                        },
                    }
                }
            };

            operation.AddErrorResponses(Context.Settings, false);

            base.SetResponses(operation);
        }

        protected override void SetSecurity(OpenApiOperation operation)
        {
            ReadRestrictionsType read = Context.Model.GetRecord<ReadRestrictionsType>(EntitySet, CapabilitiesConstants.ReadRestrictions);
            if (read == null || read.Permissions == null)
            {
                return;
            }

            operation.Security = Context.CreateSecurityRequirements(read.Permissions).ToList();
        }

        protected override void AppendCustomParameters(OpenApiOperation operation)
        {
            ReadRestrictionsType read = Context.Model.GetRecord<ReadRestrictionsType>(EntitySet, CapabilitiesConstants.ReadRestrictions);
            if (read == null)
            {
                return;
            }

            if (read.CustomHeaders != null)
            {
                AppendCustomParameters(operation, read.CustomHeaders, ParameterLocation.Header);
            }

            if (read.CustomQueryOptions != null)
            {
                AppendCustomParameters(operation, read.CustomQueryOptions, ParameterLocation.Query);
            }
        }
    }
}
