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
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Generator;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;

namespace Microsoft.OpenApi.OData.Operation
{
    /// <summary>
    /// Retrieve a navigation property from a navigation source.
    /// The Path Item Object for the entity contains the keyword get with an Operation Object as value
    /// that describes the capabilities for retrieving a navigation property form a navigation source.
    /// </summary>
    internal class NavigationPropertyGetOperationHandler : NavigationPropertyOperationHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationPropertyGetOperationHandler"/> class.
        /// </summary>
        /// <param name="document">The document to use to lookup references.</param>
        public NavigationPropertyGetOperationHandler(OpenApiDocument document) : base(document)
        {
            
        }
        /// <inheritdoc/>
        public override HttpMethod OperationType => HttpMethod.Get;

        private ReadRestrictionsType? _readRestriction;

        /// <inheritdoc/>
        protected override void Initialize(ODataContext context, ODataPath path)
        {
            base.Initialize(context, path);
            _readRestriction = GetRestrictionAnnotation(CapabilitiesConstants.ReadRestrictions) as ReadRestrictionsType;
        }

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            // Summary and Description
            string placeHolder = "Get " + NavigationProperty?.Name + " from " + NavigationSource?.Name;
            operation.Summary = (LastSegmentIsKeySegment ? _readRestriction?.ReadByKeyRestrictions?.Description : _readRestriction?.Description) ?? placeHolder;    
            operation.Description = (LastSegmentIsKeySegment ? _readRestriction?.ReadByKeyRestrictions?.LongDescription : _readRestriction?.LongDescription)
                ?? Context?.Model.GetDescriptionAnnotation(NavigationProperty);

            // OperationId
            if (Context is { Settings.EnableOperationId: true })
            {
                string prefix = "Get";
                if (!LastSegmentIsKeySegment && NavigationProperty.TargetMultiplicity() == EdmMultiplicity.Many)
                {
                    prefix = "List";
                }

                operation.OperationId = GetOperationId(prefix);
            }
            
            base.SetBasicInfo(operation);
        }

        protected override void SetExtensions(OpenApiOperation operation)
        {
            if (Context is { Settings.EnablePagination: true } && !LastSegmentIsKeySegment && NavigationProperty.TargetMultiplicity() == EdmMultiplicity.Many)
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

        /// <inheritdoc/>
        protected override void SetResponses(OpenApiOperation operation)
        {
            Dictionary<string, IOpenApiLink>? links = null;
            if (Context is { Settings.ShowLinks: true } && NavigationProperty is not null && Path is not null)
            {
                var operationId = GetOperationId();

                links = Context.CreateLinks(entityType: NavigationProperty.ToEntityType(), entityName: NavigationProperty.Name,
                        entityKind: NavigationProperty.PropertyKind.ToString(), path: Path, parameters: PathParameters,
                        navPropOperationId: operationId);
            }

            if (!LastSegmentIsKeySegment && NavigationProperty.TargetMultiplicity() == EdmMultiplicity.Many)
            {
                operation.Responses = new OpenApiResponses
                {
                    {
                        Context?.Settings.UseSuccessStatusCodeRange ?? false ? Constants.StatusCodeClass2XX : Constants.StatusCode200,
                        new OpenApiResponseReference($"{NavigationProperty.ToEntityType().FullName()}{Constants.CollectionSchemaSuffix}", _document)
                    }
                };
            }
            else
            {
                IOpenApiSchema? schema = null;
                var entityType = NavigationProperty.ToEntityType();

                if (Context is { Settings.EnableDerivedTypesReferencesForResponses: true })
                {
                    schema = EdmModelHelper.GetDerivedTypesReferenceSchema(entityType, Context.Model, _document);
                }

                schema ??= new OpenApiSchemaReference(entityType.FullName(), _document);

                operation.Responses = new OpenApiResponses
                {
                    {
                        Context?.Settings.UseSuccessStatusCodeRange ?? false ? Constants.StatusCodeClass2XX : Constants.StatusCode200,
                        new OpenApiResponse
                        {
                            Description = "Retrieved navigation property",
                            Content = new Dictionary<string, OpenApiMediaType>
                            {
                                {
                                    Constants.ApplicationJsonMediaType,
                                    new OpenApiMediaType
                                    {
                                        Schema = schema
                                    }
                                }
                            },
                            Links = links
                        }
                    }
                };
            }

            if (Context is not null)
                operation.AddErrorResponses(Context.Settings, _document, false);

            base.SetResponses(operation);
        }

        /// <inheritdoc/>
        protected override void SetParameters(OpenApiOperation operation)
        {
            base.SetParameters(operation);

            if (Context is null)
            {
                return;
            }

            var (selectParameter, expandParameter) = (string.IsNullOrEmpty(TargetPath), NavigationProperty) switch
            { 
                (false, not null) when NavigationProperty.ToEntityType() is {} entityType =>
                    (Context.CreateSelect(TargetPath!, entityType) ?? Context.CreateSelect(NavigationProperty), 
                    Context.CreateExpand(TargetPath!, entityType) ?? Context.CreateExpand(NavigationProperty)),
                (true, not null) => (Context.CreateSelect(NavigationProperty), Context.CreateExpand(NavigationProperty)),
                _ => (null, null),
            };

            var parametersToAdd = new List<IOpenApiParameter>();
            if (!LastSegmentIsKeySegment && NavigationProperty?.TargetMultiplicity() == EdmMultiplicity.Many)
            {
                // Need to verify that TopSupported or others should be applied to navigation source.
                // So, how about for the navigation property.
                AddParameterIfExists(parametersToAdd, Context.CreateTop, Context.CreateTop);
                AddParameterIfExists(parametersToAdd, Context.CreateSkip, Context.CreateSkip);
                AddParameterIfExists(parametersToAdd, Context.CreateSearch, Context.CreateSearch);
                AddParameterIfExists(parametersToAdd, Context.CreateFilter, Context.CreateFilter);
                AddParameterIfExists(parametersToAdd, Context.CreateCount, Context.CreateCount);

                var orderByParameter = (string.IsNullOrEmpty(TargetPath), NavigationProperty) switch
                { 
                    (false, not null) when NavigationProperty.ToEntityType() is {} entityType =>
                        Context.CreateOrderBy(TargetPath!, entityType),
                    (true, not null) => Context.CreateOrderBy(NavigationProperty),
                    _ => null,
                };
                if (orderByParameter != null)
                {
                    parametersToAdd.Add(orderByParameter);
                }
            }

            if (selectParameter != null)
            {
                parametersToAdd.Add(selectParameter);
            }

            if (expandParameter != null)
            {
                parametersToAdd.Add(expandParameter);
            }

            if (parametersToAdd.Count > 0)
            {
                if (operation.Parameters is null) operation.Parameters = parametersToAdd;
                else parametersToAdd.ForEach(p => operation.Parameters.Add(p));
            }
        }
        private void AddParameterIfExists(List<IOpenApiParameter> parameters,
                                            Func<string, OpenApiDocument, IOpenApiParameter?> createParameterFromPath,
                                            Func<IEdmNavigationProperty, OpenApiDocument, IOpenApiParameter?> createParameterFromProperty)
        {
            if (!string.IsNullOrEmpty(TargetPath) && createParameterFromPath(TargetPath, _document) is {} parameterFromPath)
            {
                parameters.Add(parameterFromPath);
            }
            else if (NavigationProperty is not null && createParameterFromProperty(NavigationProperty, _document) is {} parameterFromProperty)
            {
                parameters.Add(parameterFromProperty);
            }
        }

        protected override void SetSecurity(OpenApiOperation operation)
        {
            if (_readRestriction == null)
            {
                return;
            }

            ReadRestrictionsBase readBase = _readRestriction;
            if (LastSegmentIsKeySegment && _readRestriction.ReadByKeyRestrictions != null)
            {
                readBase = _readRestriction.ReadByKeyRestrictions;
            }

            if (readBase.Permissions is not null)
                operation.Security = Context?.CreateSecurityRequirements(readBase.Permissions, _document).ToList();
        }

        protected override void AppendCustomParameters(OpenApiOperation operation)
        {
            if (_readRestriction == null)
            {
                return;
            }

            ReadRestrictionsBase readBase = _readRestriction;
            if (LastSegmentIsKeySegment && _readRestriction.ReadByKeyRestrictions != null)
            {
                readBase = _readRestriction.ReadByKeyRestrictions;
            }

            if (readBase.CustomHeaders != null)
            {
                AppendCustomParameters(operation, readBase.CustomHeaders, ParameterLocation.Header);
            }

            if (readBase.CustomQueryOptions != null)
            {
                AppendCustomParameters(operation, readBase.CustomQueryOptions, ParameterLocation.Query);
            }
        }
    }
}
