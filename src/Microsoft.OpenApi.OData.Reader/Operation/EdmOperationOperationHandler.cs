// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Microsoft.OData.Edm;
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
        /// Initializes a new instance of the <see cref="EdmOperationOperationHandler"/> class.
        /// </summary>
        /// <param name="document">The document to use to lookup references.</param>
        protected EdmOperationOperationHandler(OpenApiDocument document) : base(document)
        {
            
        }
        private OperationRestrictionsType? _operationRestriction;

        /// <summary>
        /// Gets the navigation source.
        /// </summary>
        protected IEdmNavigationSource? NavigationSource { get; private set; }

        /// <summary>
        /// Gets the Edm operation.
        /// </summary>
        protected IEdmOperation? EdmOperation { get; private set; }

        /// <summary>
        /// Gets the OData operation segment.
        /// </summary>
        protected ODataOperationSegment? OperationSegment { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the path has type cast segment or not.
        /// </summary>
        protected bool HasTypeCast { get; private set; }

        /// <inheritdoc/>
        protected override void Initialize(ODataContext context, ODataPath path)
        {
            base.Initialize(context, path);

            // It's bound operation, the first segment must be the navigaiton source.
            if (path.FirstSegment is ODataNavigationSourceSegment navigationSourceSegment)
                NavigationSource = navigationSourceSegment.NavigationSource;

            if (path.LastSegment is ODataOperationSegment opSegment)
            {
                OperationSegment = opSegment;
                EdmOperation = opSegment.Operation;
            }

            HasTypeCast = path.Segments.Any(s => s is ODataTypeCastSegment);

            if (!string.IsNullOrEmpty(TargetPath))
                _operationRestriction = Context?.Model.GetRecord<OperationRestrictionsType>(TargetPath, CapabilitiesConstants.OperationRestrictions);
            if (Context is not null && EdmOperation is not null &&
                Context.Model.GetRecord<OperationRestrictionsType>(EdmOperation, CapabilitiesConstants.OperationRestrictions) is { } operationRestrictions)
            {
                _operationRestriction?.MergePropertiesIfNull(operationRestrictions);
                _operationRestriction ??= operationRestrictions;
            }
        }

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            // Summary
            operation.Summary = "Invoke " + (EdmOperation?.IsAction() ?? false ? "action " : "function ") + EdmOperation?.Name;

            // Description
            operation.Description = (string.IsNullOrEmpty(TargetPath) ? null :Context?.Model.GetDescriptionAnnotation(TargetPath)) ?? Context?.Model.GetDescriptionAnnotation(EdmOperation);

            // OperationId
            if (Context is {Settings.EnableOperationId: true} && Path is not null)
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
                        if (!keySegment.IsAlternateKey && segment is {EntityType: not null}) 
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
                    else if (segment is ODataOperationSegment {Identifier: not null} opSegment)
                    {
                        if (opSegment.Operation is IEdmFunction function && Context.Model.IsOperationOverload(function))
                        {
                            // Hash the segment to avoid duplicate operationIds
                            pathHash = string.IsNullOrEmpty(pathHash)
                                ? opSegment.GetPathHash(Context.Settings)
                                : (pathHash + opSegment.GetPathHash(Context.Settings)).GetHashSHA256()[..4];
                        }

                        identifiers.Add(opSegment.Identifier);
                    }
                    else if (!string.IsNullOrEmpty(segment.Identifier))
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
            tag.Extensions ??= new Dictionary<string, IOpenApiExtension>();
            tag.Extensions.Add(Constants.xMsTocType, new JsonNodeExtension("container"));
            operation.Tags ??= new HashSet<OpenApiTagReference>();
            operation.Tags.Add(new OpenApiTagReference(tag.Name, _document));

            Context?.AppendTag(tag);

            base.SetTags(operation);
        }

        /// <summary>
        /// Genrates the tag name for the operation. Adds Action or Function name to the tag name if the operation is an action or function.
        /// </summary>
        /// <param name="tagName">The generated tag name.</param>
        /// <param name="skip">The number of segments to skip.</param>
        private void GenerateTagName(out string tagName, int skip = 1)
        {
            if (Path is null) throw new InvalidOperationException("Path is null.");
            var targetSegment = Path.Segments.Reverse().Skip(skip).FirstOrDefault();

            switch (targetSegment)
            {
                case ODataNavigationPropertySegment when Context is not null:
                    tagName = EdmModelHelper.GenerateNavigationPropertyPathTagName(Path, Context);
                    break;
                case ODataOperationImportSegment:
                // Previous segmment could be a navigation property or a navigation source segment
                case ODataKeySegment:
                    skip += 1;
                    GenerateTagName(out tagName, skip);
                    break;
                default:
                    tagName = NavigationSource?.Name + "." + NavigationSource?.EntityType.Name;
                    if (EdmOperation.IsAction())
                    {
                        tagName += ".Actions";
                    }
                    else if (EdmOperation.IsFunction())
                    {
                        tagName += ".Functions";
                    }
                    break;
            }
        }

        /// <inheritdoc/>
        protected override void SetParameters(OpenApiOperation operation)
        {
            base.SetParameters(operation);

            if (EdmOperation.IsFunction() && EdmOperation is IEdmFunction function)
            {
                AppendSystemQueryOptions(function, operation);
            }
        }

        /// <inheritdoc/>
        protected override void SetResponses(OpenApiOperation operation) 
        {
            if (EdmOperation is not null && Context is not null)
                operation.Responses = Context.CreateResponses(EdmOperation, _document);
            base.SetResponses(operation);
        }

        /// <inheritdoc/>
        protected override void SetSecurity(OpenApiOperation operation)
        {
            if (_operationRestriction == null || _operationRestriction.Permissions == null)
            {
                return;
            }

            operation.Security = Context?.CreateSecurityRequirements(_operationRestriction.Permissions, _document).ToList();
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
            if (function.GetReturn()?.Type is {} functionReturnType && functionReturnType.IsCollection() && Context is not null)
            {
                operation.Parameters ??= [];
                // $top
                if (Context.CreateTop(function, _document) is {} topParameter)
                {
                    operation.Parameters.AppendParameter(topParameter);
                }

                // $skip
                if (Context.CreateSkip(function, _document) is {} skipParameter)
                {
                    operation.Parameters.AppendParameter(skipParameter);
                }

                // $search
                if (Context.CreateSearch(function, _document) is {} searchParameter)
                {
                    operation.Parameters.AppendParameter(searchParameter);
                }

                // $filter
                if (Context.CreateFilter(function, _document) is {} filterParameter)
                {
                    operation.Parameters.AppendParameter(filterParameter);
                }

                // $count
                if (Context.CreateCount(function, _document) is {} countParameter)
                {
                    operation.Parameters.AppendParameter(countParameter);
                }

                if (functionReturnType?.Definition?.AsElementType() is IEdmEntityType entityType)
                {
                    // $select
                    if (Context.CreateSelect(function, entityType) is {} selectParameter)
                    {
                        operation.Parameters.AppendParameter(selectParameter);
                    }

                    // $orderby
                    if (Context.CreateOrderBy(function, entityType) is {} orderbyParameter)
                    {
                        operation.Parameters.AppendParameter(orderbyParameter);
                    }

                    // $expand
                    if (Context.CreateExpand(function, entityType) is {} expandParameter)
                    {
                        operation.Parameters.AppendParameter(expandParameter);
                    }
                }
            }
        }

        /// <inheritdoc/>
        protected override void SetCustomLinkRelType()
        {
            if (Context is {Settings.CustomHttpMethodLinkRelMapping: not null} && EdmOperation != null)
            {
                LinkRelKey key = EdmOperation.IsAction() ? LinkRelKey.Action : LinkRelKey.Function;
                Context.Settings.CustomHttpMethodLinkRelMapping.TryGetValue(key, out var linkRelValue);
                CustomLinkRel =  linkRelValue;
            }
        }
    
        /// <inheritdoc/>
        protected override void SetExternalDocs(OpenApiOperation operation)
        {
            if (Context is { Settings.ShowExternalDocs: true })
            {
                var externalDocs = (string.IsNullOrEmpty(TargetPath), string.IsNullOrEmpty(CustomLinkRel))  switch
                {
                    (false, false) => Context.Model.GetLinkRecord(TargetPath!, CustomLinkRel!),
                    (true, false) when EdmOperation is not null => Context.Model.GetLinkRecord(EdmOperation, CustomLinkRel!),
                    (_, _) => null,
                };

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
            if (Context is { Settings.EnablePagination: true } && EdmOperation?.GetReturn()?.Type?.TypeKind() == EdmTypeKind.Collection)
            {
                var extension = new JsonObject
                {
                    { "nextLinkName", "@odata.nextLink"},
                    { "operationName", Context.Settings.PageableOperationName}
                };
                
                operation.Extensions ??= new Dictionary<string, IOpenApiExtension>();
                operation.Extensions.Add(Constants.xMsPageable, new JsonNodeExtension(extension));
            }
            base.SetExtensions(operation);
        }
    }
}
