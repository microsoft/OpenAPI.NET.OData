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
using Microsoft.OpenApi.OData.Vocabulary.Core;

namespace Microsoft.OpenApi.OData.Operation
{
    /// <summary>
    /// Base class for operation of <see cref="IEdmOperation"/>.
    /// </summary>
    internal abstract class EdmOperationOperationHandler : OperationHandler
    {
        /// <summary>
        /// Gets the navigation source.
        /// </summary>
        protected IEdmNavigationSource NavigationSource { get; private set; }

        /// <summary>
        /// Gets the Edm operation.
        /// </summary>
        protected IEdmOperation EdmOperation { get; private set; }

        /// <summary>
        /// Gets the OData operation segment.
        /// </summary>
        protected ODataOperationSegment OperationSegment { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the path has type cast segment or not.
        /// </summary>
        protected bool HasTypeCast { get; private set; }

        /// <inheritdoc/>
        protected override void Initialize(ODataContext context, ODataPath path)
        { 
            // It's bound operation, the first segment must be the navigaiton source.
            ODataNavigationSourceSegment navigationSourceSegment = path.FirstSegment as ODataNavigationSourceSegment;
            NavigationSource = navigationSourceSegment.NavigationSource;

            OperationSegment = path.LastSegment as ODataOperationSegment;
            EdmOperation = OperationSegment.Operation;

            HasTypeCast = path.Segments.Any(s => s is ODataTypeCastSegment);

            base.Initialize(context, path);
        }

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            // Summary
            operation.Summary = "Invoke " + (EdmOperation.IsAction() ? "action " : "function ") + EdmOperation.Name;

            // Description
            operation.Description = Context.Model.GetDescriptionAnnotation(EdmOperation);

            // OperationId
            if (Context.Settings.EnableOperationId)
            {
                // When the key segment is available,
                // its EntityType name will be used
                // in the operationId to avoid potential
                // duplicates in entity vs entityset functions/actions

                List<string> identifiers = new();
                foreach (ODataSegment segment in Path.Segments)
                {
                    if (segment is not ODataKeySegment)
                    {
                        identifiers.Add(segment.Identifier);
                    }
                    else
                    {
                        identifiers.Add(segment.EntityType.Name);
                    }
                }

                string operationId = string.Join(".", identifiers);

                if (EdmOperation.IsAction())
                {
                    operation.OperationId = operationId;
                }
                else
                {
                    if (Path.LastSegment is ODataOperationSegment operationSegment &&
                        Context.Model.IsOperationOverload(operationSegment.Operation))
                    {
                        operation.OperationId = operationId + "-" + Path.LastSegment.GetPathHash(Context.Settings);
                    }
                    else
                    {
                        operation.OperationId = operationId;
                    }
                }
            }

            base.SetBasicInfo(operation);
        }

        /// <inheritdoc/>
        protected override void SetTags(OpenApiOperation operation)
        {
            string value = EdmOperation.IsAction() ? "Actions" : "Functions";
            OpenApiTag tag = new OpenApiTag
            {
                Name = NavigationSource.Name + "." + value,
            };
            tag.Extensions.Add(Constants.xMsTocType, new OpenApiString("container"));
            operation.Tags.Add(tag);

            Context.AppendTag(tag);

            base.SetTags(operation);
        }

        /// <inheritdoc/>
        protected override void SetParameters(OpenApiOperation operation)
        {
            base.SetParameters(operation);

            if (EdmOperation.IsFunction())
            {
                IEdmFunction function = (IEdmFunction)EdmOperation;

                if (OperationSegment.ParameterMappings != null)
                {
                    IList<OpenApiParameter> parameters = Context.CreateParameters(function, OperationSegment.ParameterMappings);
                    foreach (var parameter in parameters)
                    {
                        operation.Parameters.AppendParameter(parameter);
                    }
                }
                else
                {
                    IDictionary<string, string> mappings = ParameterMappings[OperationSegment];
                    IList<OpenApiParameter> parameters = Context.CreateParameters(function, mappings);
                    if (operation.Parameters == null)
                    {
                        operation.Parameters = parameters;
                    }
                    else
                    {
                        foreach (var parameter in parameters)
                        {
                            operation.Parameters.AppendParameter(parameter);
                        }
                    }
                }

                AppendSystemQueryOptions(function, operation);
            }
        }

        /// <inheritdoc/>
        protected override void SetResponses(OpenApiOperation operation) 
        {
            operation.Responses = Context.CreateResponses(EdmOperation);
            base.SetResponses(operation);
        }

        /// <inheritdoc/>
        protected override void SetSecurity(OpenApiOperation operation)
        {
            OperationRestrictionsType restriction = Context.Model.GetRecord<OperationRestrictionsType>(EdmOperation, CapabilitiesConstants.OperationRestrictions);
            if (restriction == null || restriction.Permissions == null)
            {
                return;
            }

            operation.Security = Context.CreateSecurityRequirements(restriction.Permissions).ToList();
        }

        /// <inheritdoc/>
        protected override void AppendCustomParameters(OpenApiOperation operation)
        {
            OperationRestrictionsType restriction = Context.Model.GetRecord<OperationRestrictionsType>(EdmOperation, CapabilitiesConstants.OperationRestrictions);
            if (restriction == null)
            {
                return;
            }

            if (restriction.CustomHeaders != null)
            {
                AppendCustomParameters(operation, restriction.CustomHeaders, ParameterLocation.Header);
            }

            if (restriction.CustomQueryOptions != null)
            {
                AppendCustomParameters(operation, restriction.CustomQueryOptions, ParameterLocation.Query);
            }
        }

        private void AppendSystemQueryOptions(IEdmFunction function, OpenApiOperation operation)
        {
            if (function.ReturnType.IsCollection())
            {
                // $top
                if (Context.CreateTop(function) is OpenApiParameter topParameter)
                {
                    operation.Parameters.AppendParameter(topParameter);
                }

                // $skip
                if (Context.CreateSkip(function) is OpenApiParameter skipParameter)
                {
                    operation.Parameters.AppendParameter(skipParameter);
                }

                // $search
                if (Context.CreateSearch(function) is OpenApiParameter searchParameter)
                {
                    operation.Parameters.AppendParameter(searchParameter);
                }

                // $filter
                if (Context.CreateFilter(function) is OpenApiParameter filterParameter)
                {
                    operation.Parameters.AppendParameter(filterParameter);
                }

                // $count
                if (Context.CreateCount(function) is OpenApiParameter countParameter)
                {
                    operation.Parameters.AppendParameter(countParameter);
                }

                if (function.ReturnType?.Definition?.AsElementType() is IEdmEntityType entityType)
                {
                    // $select
                    if (Context.CreateSelect(function, entityType) is OpenApiParameter selectParameter)
                    {
                        operation.Parameters.AppendParameter(selectParameter);
                    }

                    // $orderby
                    if (Context.CreateOrderBy(function, entityType) is OpenApiParameter orderbyParameter)
                    {
                        operation.Parameters.AppendParameter(orderbyParameter);
                    }

                    // $expand
                    if (Context.CreateExpand(function, entityType) is OpenApiParameter expandParameter)
                    {
                        operation.Parameters.AppendParameter(expandParameter);
                    }
                }
            }
        }

        /// <inheritdoc/>
        protected override void SetCustomLinkRelType()
        {
            if (Context.Settings.CustomHttpMethodLinkRelMapping != null && EdmOperation != null)
            {
                LinkRelKey key = EdmOperation.IsAction() ? LinkRelKey.Action : LinkRelKey.Function;
                Context.Settings.CustomHttpMethodLinkRelMapping.TryGetValue(key, out string linkRelValue);
                CustomLinkRel =  linkRelValue;
            }
        }
    
        /// <inheritdoc/>
        protected override void SetExternalDocs(OpenApiOperation operation)
        {
            if (Context.Settings.ShowExternalDocs && Context.Model.GetLinkRecord(EdmOperation, CustomLinkRel) is Link externalDocs)
            {
                operation.ExternalDocs = new OpenApiExternalDocs()
                {
                    Description = CoreConstants.ExternalDocsDescription,
                    Url = externalDocs.Href
                };
            }
        }

        // <inheritdoc/>
        protected override void SetExtensions(OpenApiOperation operation)
        {
            if (Context.Settings.EnablePagination && EdmOperation.ReturnType?.TypeKind() == EdmTypeKind.Collection)
            {
                OpenApiObject extension = new OpenApiObject
                {
                    { "nextLinkName", new OpenApiString("@odata.nextLink")},
                    { "operationName", new OpenApiString(Context.Settings.PageableOperationName)}
                };

                operation.Extensions.Add(Constants.xMsPageable, extension);
            }
            base.SetExtensions(operation);
        }
    }
}
