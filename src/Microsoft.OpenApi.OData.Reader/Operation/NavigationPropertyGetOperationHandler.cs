// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Nodes;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.Interfaces;
using Microsoft.OpenApi.Models.References;
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

        private ReadRestrictionsType _readRestriction;

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
            string placeHolder = "Get " + NavigationProperty.Name + " from " + NavigationSource.Name;
            operation.Summary = (LastSegmentIsKeySegment ? _readRestriction?.ReadByKeyRestrictions?.Description : _readRestriction?.Description) ?? placeHolder;    
            operation.Description = (LastSegmentIsKeySegment ? _readRestriction?.ReadByKeyRestrictions?.LongDescription : _readRestriction?.LongDescription)
                ?? Context.Model.GetDescriptionAnnotation(NavigationProperty);

            // OperationId
            if (Context.Settings.EnableOperationId)
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
            if (Context.Settings.EnablePagination)
            {
                if (!LastSegmentIsKeySegment && NavigationProperty.TargetMultiplicity() == EdmMultiplicity.Many)
                {
                    JsonObject extension = new JsonObject
                    {
                        { "nextLinkName", "@odata.nextLink"},
                        { "operationName", Context.Settings.PageableOperationName}
                    };

                    operation.Extensions.Add(Constants.xMsPageable, new OpenApiAny(extension));
                }
            }

            base.SetExtensions(operation);
        }

        /// <inheritdoc/>
        protected override void SetResponses(OpenApiOperation operation)
        {
            IDictionary<string, IOpenApiLink> links = null;
            if (Context.Settings.ShowLinks)
            {
                string operationId = GetOperationId();

                links = Context.CreateLinks(entityType: NavigationProperty.ToEntityType(), entityName: NavigationProperty.Name,
                        entityKind: NavigationProperty.PropertyKind.ToString(), path: Path, parameters: PathParameters,
                        navPropOperationId: operationId);
            }

            if (!LastSegmentIsKeySegment && NavigationProperty.TargetMultiplicity() == EdmMultiplicity.Many)
            {
                operation.Responses = new OpenApiResponses
                {
                    {
                        Context.Settings.UseSuccessStatusCodeRange ? Constants.StatusCodeClass2XX : Constants.StatusCode200,
                        new OpenApiResponseReference($"{NavigationProperty.ToEntityType().FullName()}{Constants.CollectionSchemaSuffix}", _document)
                    }
                };
            }
            else
            {
                IOpenApiSchema schema = null;
                var entityType = NavigationProperty.ToEntityType();

                if (Context.Settings.EnableDerivedTypesReferencesForResponses)
                {
                    schema = EdmModelHelper.GetDerivedTypesReferenceSchema(entityType, Context.Model, _document);
                }

                schema ??= new OpenApiSchemaReference(entityType.FullName(), _document);

                operation.Responses = new OpenApiResponses
                {
                    {
                        Context.Settings.UseSuccessStatusCodeRange ? Constants.StatusCodeClass2XX : Constants.StatusCode200,
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

            operation.AddErrorResponses(Context.Settings, _document, false);

            base.SetResponses(operation);
        }

        /// <inheritdoc/>
        protected override void SetParameters(OpenApiOperation operation)
        {
            base.SetParameters(operation);

            OpenApiParameter selectParameter = Context.CreateSelect(TargetPath, NavigationProperty.ToEntityType()) 
                ?? Context.CreateSelect(NavigationProperty);

            OpenApiParameter expandParameter = Context.CreateExpand(TargetPath, NavigationProperty.ToEntityType()) 
                ?? Context.CreateExpand(NavigationProperty);

            if (!LastSegmentIsKeySegment && NavigationProperty.TargetMultiplicity() == EdmMultiplicity.Many)
            {
                // Need to verify that TopSupported or others should be applied to navigation source.
                // So, how about for the navigation property.
                var parameter = Context.CreateTop(TargetPath, _document) ?? Context.CreateTop(NavigationProperty, _document);
                if (parameter != null)
                {
                    operation.Parameters.Add(parameter);
                }

                parameter = Context.CreateSkip(TargetPath, _document) ?? Context.CreateSkip(NavigationProperty, _document);
                if (parameter != null)
                {
                    operation.Parameters.Add(parameter);
                }

                parameter = Context.CreateSearch(TargetPath, _document) ?? Context.CreateSearch(NavigationProperty, _document);
                if (parameter != null)
                {
                    operation.Parameters.Add(parameter);
                }

                parameter = Context.CreateFilter(TargetPath, _document) ?? Context.CreateFilter(NavigationProperty, _document);
                if (parameter != null)
                {
                    operation.Parameters.Add(parameter);
                }

                parameter = Context.CreateCount(TargetPath, _document) ?? Context.CreateCount(NavigationProperty, _document);
                if (parameter != null)
                {
                    operation.Parameters.Add(parameter);
                }

                parameter = Context.CreateOrderBy(TargetPath, NavigationProperty.ToEntityType()) ?? Context.CreateOrderBy(NavigationProperty);
                if (parameter != null)
                {
                    operation.Parameters.Add(parameter);
                }

                if (selectParameter != null)
                {
                    operation.Parameters.Add(selectParameter);
                }

                if (expandParameter != null)
                {
                    operation.Parameters.Add(expandParameter);
                }
            }
            else
            {
                if (selectParameter != null)
                {
                    operation.Parameters.Add(selectParameter);
                }

                if (expandParameter != null)
                {
                    operation.Parameters.Add(expandParameter);
                }
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

            operation.Security = Context.CreateSecurityRequirements(readBase.Permissions, _document).ToList();
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
