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
        private OperationRestrictionsType _operationRestriction;

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
            base.Initialize(context, path);

            // It's bound operation, the first segment must be the navigaiton source.
            ODataNavigationSourceSegment navigationSourceSegment = path.FirstSegment as ODataNavigationSourceSegment;
            NavigationSource = navigationSourceSegment.NavigationSource;

            OperationSegment = path.LastSegment as ODataOperationSegment;
            EdmOperation = OperationSegment.Operation;

            HasTypeCast = path.Segments.Any(s => s is ODataTypeCastSegment);

            _operationRestriction = Context.Model.GetRecord<OperationRestrictionsType>(TargetPath, CapabilitiesConstants.OperationRestrictions);
            var operationRestrictions = Context.Model.GetRecord<OperationRestrictionsType>(EdmOperation, CapabilitiesConstants.OperationRestrictions);
            _operationRestriction?.MergePropertiesIfNull(operationRestrictions);
            _operationRestriction ??= operationRestrictions;
        }

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            // Summary
            operation.Summary = "Invoke " + (EdmOperation.IsAction() ? "action " : "function ") + EdmOperation.Name;

            // Description
            operation.Description = Context.Model.GetDescriptionAnnotation(TargetPath) ?? Context.Model.GetDescriptionAnnotation(EdmOperation);

            // OperationId
            if (Context.Settings.EnableOperationId)
            {
                // When the key segment is available,
                // its EntityType name will be used
                // in the operationId to avoid potential
                // duplicates in entity vs entityset functions/actions

                List<string> identifiers = new();
                string pathHash = string.Empty;
                foreach (ODataSegment segment in Path.Segments)
                {
                    if (segment is ODataKeySegment keySegment)
                    {
                        if (!keySegment.IsAlternateKey) 
                        {
                            identifiers.Add(segment.EntityType.Name);
                            continue;
                        }

                        // We'll consider alternate keys in the operation id to eliminate potential duplicates with operation id of primary path
                        if (segment == Path.Segments.Last())
                        {
                            identifiers.Add("By" + string.Join("", keySegment.Identifier.Split(',').Select(static x => Utils.UpperFirstChar(x))));
                        }
                        else
                        {
                            identifiers.Add(keySegment.Identifier);
                        }
                    }
                    else if (segment is ODataOperationSegment opSegment)
                    {
                        if (opSegment.Operation is IEdmFunction function && Context.Model.IsOperationOverload(function))
                        {
                            // Hash the segment to avoid duplicate operationIds
                            pathHash = string.IsNullOrEmpty(pathHash)
                                ? opSegment.GetPathHash(Context.Settings)
                                : (pathHash + opSegment.GetPathHash(Context.Settings)).GetHashSHA256()[..4];
                        }

                        identifiers.Add(segment.Identifier);
                    }
                    else
                    {
                        identifiers.Add(segment.Identifier);
                    }
                }

                string operationId = string.Join(".", identifiers);

                if (!string.IsNullOrEmpty(pathHash))
                {
                    operation.OperationId = operationId + "-" + pathHash;
                }
                else
                {
                    operation.OperationId = operationId;
                }
            }

            base.SetBasicInfo(operation);
        }

        /// <inheritdoc/>
        protected override void SetTags(OpenApiOperation operation)
        {
            GenerateTagName(out string tagName);
            OpenApiTag tag = new()
            {
                Name = tagName,
            };
            tag.Extensions.Add(Constants.xMsTocType, new OpenApiString("container"));
            operation.Tags.Add(tag);

            Context.AppendTag(tag);

            base.SetTags(operation);
        }

        /// <summary>
        /// Genrates the tag name for the operation.
        /// </summary>
        /// <param name="tagName">The generated tag name.</param>
        /// <param name="skip">The number of segments to skip.</param>
        private void GenerateTagName(out string tagName, int skip = 1)
        {            
            var targetSegment = Path.Segments.Reverse().Skip(skip).FirstOrDefault();

            switch (targetSegment)
            {
                case ODataNavigationPropertySegment:
                    tagName = EdmModelHelper.GenerateNavigationPropertyPathTagName(Path, Context);
                    break;
                case ODataOperationSegment:
                case ODataOperationImportSegment:
                // Previous segmment could be a navigation property or a navigation source segment
                case ODataKeySegment:
                    skip += 1;
                    GenerateTagName(out tagName, skip);
                    break;
                // ODataNavigationSourceSegment
                default:
                    tagName = NavigationSource.Name + "." + NavigationSource.EntityType.Name;
                    break;
            }
        }

        /// <inheritdoc/>
        protected override void SetParameters(OpenApiOperation operation)
        {
            base.SetParameters(operation);

            if (EdmOperation.IsFunction())
            {
                IEdmFunction function = (IEdmFunction)EdmOperation;
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
            if (_operationRestriction == null || _operationRestriction.Permissions == null)
            {
                return;
            }

            operation.Security = Context.CreateSecurityRequirements(_operationRestriction.Permissions).ToList();
        }

        /// <inheritdoc/>
        protected override void AppendCustomParameters(OpenApiOperation operation)
        {
            if (_operationRestriction == null)
            {
                return;
            }

            if (_operationRestriction.CustomHeaders != null)
            {
                AppendCustomParameters(operation, _operationRestriction.CustomHeaders, ParameterLocation.Header);
            }

            if (_operationRestriction.CustomQueryOptions != null)
            {
                AppendCustomParameters(operation, _operationRestriction.CustomQueryOptions, ParameterLocation.Query);
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
            if (Context.Settings.ShowExternalDocs)
            {
                var externalDocs = Context.Model.GetLinkRecord(TargetPath, CustomLinkRel) ??
                    Context.Model.GetLinkRecord(EdmOperation, CustomLinkRel);

                if (externalDocs != null)
                {
                    operation.ExternalDocs = new OpenApiExternalDocs()
                    {
                        Description = CoreConstants.ExternalDocsDescription,
                        Url = externalDocs.Href
                    };
                }
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
