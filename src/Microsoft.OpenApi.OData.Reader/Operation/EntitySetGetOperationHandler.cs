// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Nodes;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.References;
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
        /// <summary>
        /// Initializes a new instance of <see cref="EntitySetGetOperationHandler"/> class.
        /// </summary>
        /// <param name="document">The document to use to lookup references.</param>
        public EntitySetGetOperationHandler(OpenApiDocument document) : base(document)
        {
            
        }
        /// <inheritdoc/>
        public override HttpMethod OperationType => HttpMethod.Get;

        private ReadRestrictionsType? _readRestrictions;

        protected override void Initialize(ODataContext context, ODataPath path)
        {
            base.Initialize(context, path);

            if (!string.IsNullOrEmpty(TargetPath))
                _readRestrictions = Context?.Model.GetRecord<ReadRestrictionsType>(TargetPath, CapabilitiesConstants.ReadRestrictions);
            if (Context is not null)
            {
                var entityReadRestrictions = Context.Model.GetRecord<ReadRestrictionsType>(EntitySet, CapabilitiesConstants.ReadRestrictions);
                _readRestrictions?.MergePropertiesIfNull(entityReadRestrictions);
                _readRestrictions ??= entityReadRestrictions;
            }
        }

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            // Summary and Descriptions
            string placeHolder = "Get entities from " + EntitySet.Name;
            operation.Summary = _readRestrictions?.Description ?? placeHolder;
            operation.Description = _readRestrictions?.LongDescription ?? Context?.Model.GetDescriptionAnnotation(EntitySet);

            // OperationId
            if (Context is {Settings.EnableOperationId: true})
            {
                string typeName = EntitySet.EntityType.Name;
                operation.OperationId = EntitySet.Name + "." + typeName + ".List" + Utils.UpperFirstChar(typeName);
            }
        }

        protected override void SetExtensions(OpenApiOperation operation)
        {
            if (Context is {Settings.EnablePagination: true})
            {
                var extension = new JsonObject
                {
                    { "nextLinkName", "@odata.nextLink"},
                    { "operationName", Context.Settings.PageableOperationName}
                };
                operation.Extensions ??= new Dictionary<string, IOpenApiExtension>();
                operation.Extensions.Add(Constants.xMsPageable, new OpenApiAny(extension));

                base.SetExtensions(operation);
            }
        }

        /// <inheritdoc/>
        protected override void SetParameters(OpenApiOperation operation)
        {
            base.SetParameters(operation);
            if (Context is null) return;

            // The parameters array contains Parameter Objects for all system query options allowed for this collection,
            // and it does not list system query options not allowed for this collection, see terms
            // Capabilities.TopSupported, Capabilities.SkipSupported, Capabilities.SearchRestrictions,
            // Capabilities.FilterRestrictions, and Capabilities.CountRestrictions
            // $top
            operation.Parameters ??= [];
            var parameter = (TargetPath is null ? null : Context.CreateTop(TargetPath, _document)) ?? Context.CreateTop(EntitySet, _document);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }

            // $skip
            parameter = (TargetPath is null ? null : Context.CreateSkip(TargetPath, _document)) ?? Context.CreateSkip(EntitySet, _document);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }

            // $search
            parameter = (TargetPath is null ? null : Context.CreateSearch(TargetPath, _document)) ?? Context.CreateSearch(EntitySet, _document);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }

            // $filter
            parameter = (TargetPath is null ? null : Context.CreateFilter(TargetPath, _document)) ?? Context.CreateFilter(EntitySet, _document);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }

            // $count
            parameter = (TargetPath is null ? null : Context.CreateCount(TargetPath, _document)) ?? Context.CreateCount(EntitySet, _document);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }

            // the syntax of the system query options $expand, $select, and $orderby is too flexible
            // to be formally described with OpenAPI Specification means, yet the typical use cases
            // of just providing a comma-separated list of properties can be expressed via an array-valued
            // parameter with an enum constraint
            // $order
            parameter = (TargetPath is null ? null : Context.CreateOrderBy(TargetPath, EntitySet.EntityType)) ?? Context.CreateOrderBy(EntitySet);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }

            // $select
            parameter = (TargetPath is null ? null : Context.CreateSelect(TargetPath, EntitySet.EntityType)) ?? Context.CreateSelect(EntitySet);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }

            // $expand
            parameter = (TargetPath is null ? null : Context.CreateExpand(TargetPath, EntitySet.EntityType)) ?? Context.CreateExpand(EntitySet);
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
                    Context?.Settings.UseSuccessStatusCodeRange ?? false ? Constants.StatusCodeClass2XX : Constants.StatusCode200,
                    new OpenApiResponseReference($"{EntitySet.EntityType.FullName()}{Constants.CollectionSchemaSuffix}", _document)
                }
            };

            if (Context is not null)
                operation.AddErrorResponses(Context.Settings, _document, false);

            base.SetResponses(operation);
        }

        protected override void SetSecurity(OpenApiOperation operation)
        {
            if (_readRestrictions?.Permissions == null)
            {
                return;
            }

            operation.Security = Context?.CreateSecurityRequirements(_readRestrictions.Permissions, _document).ToList();
        }

        protected override void AppendCustomParameters(OpenApiOperation operation)
        {
            if (_readRestrictions == null)
            {
                return;
            }

            if (_readRestrictions.CustomHeaders != null)
            {
                AppendCustomParameters(operation, _readRestrictions.CustomHeaders, ParameterLocation.Header);
            }

            if (_readRestrictions.CustomQueryOptions != null)
            {
                AppendCustomParameters(operation, _readRestrictions.CustomQueryOptions, ParameterLocation.Query);
            }
        }
    }
}
