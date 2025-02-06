﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;
using System.Diagnostics;
using System;
using System.Text.Json.Nodes;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models.References;
using Microsoft.OpenApi.Models.Interfaces;

namespace Microsoft.OpenApi.OData.Generator
{
    /// <summary>
    /// Extension methods to create <see cref="OpenApiParameter"/> by Edm model.
    /// </summary>
    internal static class OpenApiParameterGenerator
    {
        /// <summary>
        /// 4.6.2 Field parameters in components
        /// Create a map of <see cref="OpenApiParameter"/> object.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="document">The Open API document.</param>
        public static void AddParametersToDocument(this ODataContext context, OpenApiDocument document)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(document, nameof(document));

            // It allows defining query options and headers that can be reused across operations of the service.
            // The value of parameters is a map of Parameter Objects.
            document.AddComponent("top", CreateTop(context.Settings.TopExample));
            document.AddComponent("skip", CreateSkip());
            document.AddComponent("count", CreateCount());
            document.AddComponent("filter", CreateFilter());
            document.AddComponent("search", CreateSearch());
        }

        /// <summary>
        /// Create the list of <see cref="OpenApiParameter"/> for a <see cref="IEdmFunctionImport"/>.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="functionImport">The Edm function import.</param>
        /// <param name="document">The Open API document to lookup references.</param>
        /// <returns>The created list of <see cref="OpenApiParameter"/>.</returns>
        public static IList<IOpenApiParameter> CreateParameters(this ODataContext context, IEdmFunctionImport functionImport, OpenApiDocument document)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(functionImport, nameof(functionImport));
            Utils.CheckArgumentNull(document, nameof(document));

            return context.CreateParameters(functionImport.Function, document);
        }

        /// <summary>
        /// Create the list of <see cref="OpenApiParameter"/> for a <see cref="IEdmFunction"/>.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="function">The Edm function.</param>
        /// <param name="document">The Open API document to lookup references.</param>
        /// <param name="parameterNameMapping">The parameter name mapping.</param>
        /// <returns>The created list of <see cref="OpenApiParameter"/>.</returns>
        public static IList<IOpenApiParameter> CreateParameters(this ODataContext context, IEdmFunction function,
            OpenApiDocument document,
            IDictionary<string, string> parameterNameMapping = null)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(function, nameof(function));
            Utils.CheckArgumentNull(document, nameof(document));

            var parameters = new List<IOpenApiParameter>();            
            int skip = function.IsBound ? 1 : 0;
            foreach (IEdmOperationParameter edmParameter in function.Parameters.Skip(skip))
            {
                if (parameterNameMapping != null && !parameterNameMapping.ContainsKey(edmParameter.Name))
                {
                    continue;
                }

                OpenApiParameter parameter;
                bool isOptionalParameter = edmParameter is IEdmOptionalParameter;
                if (edmParameter.Type.IsStructured() ||
                    edmParameter.Type.IsCollection())
                {
                    parameter = new OpenApiParameter
                    {
                        Name = parameterNameMapping == null ? edmParameter.Name : parameterNameMapping[edmParameter.Name],
                        In = ParameterLocation.Path,
                        Required = true,

                        // Create schema in the Content property to indicate that the parameters are serialized as JSON
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            {
                                Constants.ApplicationJsonMediaType,
                                new OpenApiMediaType
                                {
                                    Schema = new OpenApiSchema
                                    {
                                        Type = JsonSchemaType.Array,
                                        Items = new OpenApiSchema
                                        {
                                            Type = JsonSchemaType.String
                                        },

                                        // These Parameter Objects optionally can contain the field description,
                                        // whose value is the value of the unqualified annotation Core.Description of the parameter.
                                        Description = context.Model.GetDescriptionAnnotation(edmParameter)
                                    }
                                }
                            }
                        },

                        // The parameter description describes the format this URL-encoded JSON object or array, and/or reference to [OData-URL].
                        Description = "The URL-encoded JSON " + (edmParameter.Type.IsStructured() ? "array" : "object")
                    };
                }
                else
                {
                    // Primitive parameters use the same type mapping as described for primitive properties.
                    parameter = new OpenApiParameter
                    {
                        Name = parameterNameMapping == null ? edmParameter.Name : parameterNameMapping[edmParameter.Name],
                        In = isOptionalParameter ? ParameterLocation.Query : ParameterLocation.Path,
                        Required = !isOptionalParameter,
                        Schema = context.CreateEdmTypeSchemaForParameter(edmParameter.Type, document)
                    };
                }

                if (parameterNameMapping != null)
                {
                    var quote = edmParameter.Type.Definition.ShouldPathParameterBeQuoted(context.Settings) ? "'" : string.Empty;
                    parameter.Description = isOptionalParameter
                        ? $"Usage: {edmParameter.Name}={quote}@{parameterNameMapping[edmParameter.Name]}{quote}"
                        : $"Usage: {edmParameter.Name}={quote}{{{parameterNameMapping[edmParameter.Name]}}}{quote}";
                }

                parameters.Add(parameter);
            }

            return parameters;
        }

        /// <summary>
        /// Create key parameters for the <see cref="ODataKeySegment"/>.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="keySegment">The key segment.</param>
        /// <param name="document">The Open API document to lookup references.</param>
        /// <param name="parameterNameMapping">The parameter name mapping.</param>
        /// <returns>The created list of <see cref="OpenApiParameter"/>.</returns>
        public static IList<OpenApiParameter> CreateKeyParameters(this ODataContext context, ODataKeySegment keySegment,
            OpenApiDocument document,
            IDictionary<string, string> parameterNameMapping = null)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(keySegment, nameof(keySegment));
            Utils.CheckArgumentNull(document, nameof(document));
            
            if (keySegment.IsAlternateKey)
                return CreateAlternateKeyParameters(context, keySegment, document);

            IEdmEntityType entityType = keySegment.EntityType;
            IList<IEdmStructuralProperty> keys = entityType.Key().ToList();

            List<OpenApiParameter> parameters = new();
            if (keys.Count() == 1)
            {
                string keyName = keys.First().Name;

                // If dictionary parameterNameMapping is defined, there's no need of setting the
                // keyName, we will retrieve this from the dictionary key.
                if (context.Settings.PrefixEntityTypeNameBeforeKey && parameterNameMapping == null)
                {
                    keyName = entityType.Name + "-" + keys.First().Name;
                }

                OpenApiParameter parameter = new OpenApiParameter
                {
                    Name = parameterNameMapping == null ? keyName : parameterNameMapping[keyName],
                    In = ParameterLocation.Path,
                    Required = true,
                    Description = $"The unique identifier of {entityType.Name}",
                    Schema = context.CreateEdmTypeSchemaForParameter(keys[0].Type, document)
                };

                parameter.Extensions.Add(Constants.xMsKeyType, new OpenApiAny(entityType.Name));
                parameters.Add(parameter);
            }
            else
            {
                // append key parameter
                foreach (var keyProperty in entityType.Key())
                {
                    OpenApiParameter parameter = new OpenApiParameter
                    {
                        Name = parameterNameMapping == null ?
                            keyProperty.Name :
                            parameterNameMapping[keyProperty.Name],// By design: not prefix with type name if enable type name prefix
                        In = ParameterLocation.Path,
                        Required = true,
                        Description = $"Property in multi-part unique identifier of {entityType.Name}",
                        Schema = context.CreateEdmTypeSchemaForParameter(keyProperty.Type, document)
                    };

                    if (keySegment.KeyMappings != null)
                    {
                        var quote = keyProperty.Type.Definition.ShouldPathParameterBeQuoted(context.Settings) ? "'" : string.Empty;
                        parameter.Description += $", {keyProperty.Name}={quote}{{{parameter.Name}}}{quote}";
                    }

                    parameter.Extensions.Add(Constants.xMsKeyType, new OpenApiAny(entityType.Name));
                    parameters.Add(parameter);
                }
            }
            return parameters;
        }


        /// <summary>
        /// Create alternate key parameters for the <see cref="ODataKeySegment"/>.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="keySegment">The key segment.</param>
        /// <param name="document">The Open API document to lookup references.</param>
        /// <returns>A list of <see cref="OpenApiParameter"/> of alternate key parameters.</returns>
        private static List<OpenApiParameter> CreateAlternateKeyParameters(ODataContext context, ODataKeySegment keySegment, OpenApiDocument document)
        {
            Debug.Assert(keySegment.Kind == ODataSegmentKind.Key);
            
            var parameters = new List<OpenApiParameter>();
            var entityType = keySegment.EntityType;
            var alternateKeys = context.Model.GetAlternateKeysAnnotation(entityType);            
            
            foreach (var alternateKey in alternateKeys)
            {
                if (alternateKey.Count() == 1)
                {
                    if (keySegment.Identifier.Equals(alternateKey.First().Key, StringComparison.OrdinalIgnoreCase))
                    {
                        parameters.Add(
                        new OpenApiParameter
                        {
                            Name = alternateKey.First().Key,
                            In = ParameterLocation.Path,
                            Description = $"Alternate key of {entityType.Name}",
                            Schema = context.CreateEdmTypeSchemaForParameter(alternateKey.First().Value.Type, document),
                            Required = true
                        }
                     );
                    }                    
                }
                else
                {
                    foreach (var compositekey in alternateKey)
                    {
                        if (keySegment.Identifier.Contains(compositekey.Key))
                        {
                            parameters.Add(
                            new OpenApiParameter
                            {
                                Name = compositekey.Key,
                                In = ParameterLocation.Path,
                                Description = $"Property in multi-part alternate key of {entityType.Name}",
                                Schema = context.CreateEdmTypeSchemaForParameter(compositekey.Value.Type, document),
                                Required = true
                            }
                         );
                        }                        
                    }
                }
            }
            return parameters;
        }

        /// <summary>
        /// Creates the path parameters for the <see cref="ODataPath"/>
        /// </summary>
        /// <param name="path">The ODataPath</param>
        /// <param name="context">The OData context.</param>
        /// <param name="document">The Open API document to lookup references.</param>
        /// <returns>The created list of <see cref="OpenApiParameter"/></returns>
        public static List<IOpenApiParameter> CreatePathParameters(this ODataPath path, ODataContext context, OpenApiDocument document)
        {
            List<IOpenApiParameter> pathParameters = [];
            var parameterMappings = path.CalculateParameterMapping(context.Settings);

            foreach (ODataKeySegment keySegment in path.OfType<ODataKeySegment>())
            {
                IDictionary<string, string> mapping = parameterMappings[keySegment];
                pathParameters.AddRange(context.CreateKeyParameters(keySegment, document, mapping));
            }

            foreach (ODataOperationSegment operationSegment in path.OfType<ODataOperationSegment>())
            {
                if (operationSegment.Operation is not IEdmFunction function)
                {
                    continue;
                }

                if (operationSegment.ParameterMappings != null)
                {
                    var parameters = context.CreateParameters(function, document, operationSegment.ParameterMappings);
                    foreach (var parameter in parameters)
                    {
                        pathParameters.AppendParameter(parameter);
                    }
                }
                else
                {
                    IDictionary<string, string> mappings = parameterMappings[operationSegment];
                    var parameters = context.CreateParameters(function, document, mappings);
                    pathParameters.AddRange(parameters);                    
                }
            }

            // Add the route prefix parameter v1{data}
            if (context.Settings.RoutePathPrefixProvider?.Parameters != null)
            {
                pathParameters.AddRange(context.Settings.RoutePathPrefixProvider.Parameters);
            }

            return pathParameters;
        }

        /// <summary>
        /// Adds an OpenApiParameter to an existing list of OpenApiParameters.
        /// If a parameter with the same name already exists in the list, the new parameter name
        /// if suffixed with an incrementing number
        /// </summary>
        /// <param name="parameters">The list of OpenApiParameters to be appended to</param>
        /// <param name="parameter">The new OpenApiParameter to be appended</param>
        public static void AppendParameter(this IList<IOpenApiParameter> parameters, IOpenApiParameter parameter)
        {
            HashSet<string> parametersSet = new(parameters.Select(p => p.Name));

            string parameterName = parameter.Name;
            int index = 1;
            while (parametersSet.Contains(parameterName))
            {
                parameterName += index.ToString();
                index++;
            }

            if (parameter is OpenApiParameter openApiParameter)
            {
                openApiParameter.Name = parameterName;
            }
            parametersSet.Add(parameterName);
            parameters.Add(parameter);
        }

        /// <summary>
        /// Create the $top parameter.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="target">The Edm annotation target.</param>
        /// <param name="document">The Open API document.</param>
        /// <returns>The created <see cref="OpenApiParameter"/> or null.</returns>
        public static IOpenApiParameter CreateTop(this ODataContext context, IEdmVocabularyAnnotatable target, OpenApiDocument document)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(target, nameof(target));
            Utils.CheckArgumentNull(document, nameof(document));

            bool? top = context.Model.GetBoolean(target, CapabilitiesConstants.TopSupported);
            if (top == null || top.Value)
            {
                return new OpenApiParameterReference("top", document);
            }

            return null;
        }

        /// <summary>
        /// Create the $top parameter for Edm target path.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="targetPath">The string representation of the Edm target path.</param>
        /// <param name="document">The Open API document to use to build references.</param>
        /// <returns></returns>
        public static IOpenApiParameter CreateTop(this ODataContext context, string targetPath, OpenApiDocument document)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(targetPath, nameof(targetPath));
            Utils.CheckArgumentNull(document, nameof(document));

            IEdmTargetPath target = context.Model.GetTargetPath(targetPath);
            if (target == null)
                return null;

            return context.CreateTop(target, document);
        }

        /// <summary>
        /// Create the $skip parameter.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="target">The Edm annotation target.</param>
        /// <param name="document">The Open API document to use to build references.</param>
        /// <returns>The created <see cref="OpenApiParameter"/> or null.</returns>
        public static IOpenApiParameter CreateSkip(this ODataContext context, IEdmVocabularyAnnotatable target, OpenApiDocument document)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(target, nameof(target));
            Utils.CheckArgumentNull(document, nameof(document));

            bool? skip = context.Model.GetBoolean(target, CapabilitiesConstants.SkipSupported);
            if (skip == null || skip.Value)
            {
                return new OpenApiParameterReference("skip", document);
            }

            return null;
        }

        /// <summary>
        /// Create the $skip parameter for Edm target path.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="targetPath">The string representation of the Edm target path.</param>
        /// <param name="document">The Open API document to use to build references.</param>
        /// <returns></returns>
        public static IOpenApiParameter CreateSkip(this ODataContext context, string targetPath, OpenApiDocument document)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(targetPath, nameof(targetPath));
            Utils.CheckArgumentNull(document, nameof(document));

            IEdmTargetPath target = context.Model.GetTargetPath(targetPath);
            if (target == null)
                return null;

            return context.CreateSkip(target, document);
        }

        /// <summary>
        /// Create the $search parameter.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="target">The Edm annotation target.</param>
        /// <param name="document">The Open API document to use to build references.</param>
        /// <returns>The created <see cref="OpenApiParameter"/> or null.</returns>
        public static IOpenApiParameter CreateSearch(this ODataContext context, IEdmVocabularyAnnotatable target, OpenApiDocument document)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(target, nameof(target));
            Utils.CheckArgumentNull(document, nameof(document));

            SearchRestrictionsType search = context.Model.GetRecord<SearchRestrictionsType>(target, CapabilitiesConstants.SearchRestrictions);
            if (search == null || search.IsSearchable)
            {
                return new OpenApiParameterReference("search", document);
            }

            return null;
        }
        /// <summary>
        /// Create the $search parameter for Edm target path.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="targetPath">The string representation of the Edm target path.</param>
        /// <param name="document">The Open API document to use to build references.</param>
        /// <returns></returns>
        public static IOpenApiParameter CreateSearch(this ODataContext context, string targetPath, OpenApiDocument document)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(targetPath, nameof(targetPath));
            Utils.CheckArgumentNull(document, nameof(document));

            IEdmTargetPath target = context.Model.GetTargetPath(targetPath);
            if (target == null)
                return null;

            return context.CreateSearch(target, document);
        }

        /// <summary>
        /// Create the $count parameter.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="target">The Edm annotation target.</param>
        /// <param name="document">The Open API document to use to build references.</param>
        /// <returns>The created <see cref="OpenApiParameter"/> or null.</returns>
        public static IOpenApiParameter CreateCount(this ODataContext context, IEdmVocabularyAnnotatable target, OpenApiDocument document)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(target, nameof(target));
            Utils.CheckArgumentNull(document, nameof(document));

            CountRestrictionsType count = context.Model.GetRecord<CountRestrictionsType>(target, CapabilitiesConstants.CountRestrictions);
            if (count == null || count.IsCountable)
            {
                return new OpenApiParameterReference("count", document);
            }

            return null;
        }

        /// <summary>
        /// Create the $count parameter for Edm target path.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="targetPath">The string representation of the Edm target path.</param>
        /// <param name="document">The Open API document to use to build references.</param>
        /// <returns></returns>
        public static IOpenApiParameter CreateCount(this ODataContext context, string targetPath, OpenApiDocument document)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(targetPath, nameof(targetPath));
            Utils.CheckArgumentNull(document, nameof(document));

            IEdmTargetPath target = context.Model.GetTargetPath(targetPath);
            if (target == null)
                return null;

            return context.CreateCount(target, document);
        }

        /// <summary>
        /// Create the $filter parameter.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="target">The Edm annotation target.</param>
        /// <param name="document">The Open API document to use to build references.</param>
        /// <returns>The created <see cref="OpenApiParameter"/> or null.</returns>
        public static IOpenApiParameter CreateFilter(this ODataContext context, IEdmVocabularyAnnotatable target, OpenApiDocument document)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(target, nameof(target));
            Utils.CheckArgumentNull(document, nameof(document));

            FilterRestrictionsType filter = context.Model.GetRecord<FilterRestrictionsType>(target, CapabilitiesConstants.FilterRestrictions);
            if (filter == null || filter.IsFilterable)
            {
                return new OpenApiParameterReference("filter", document);
            }

            return null;
        }

        /// <summary>
        /// Create the $filter parameter for Edm target path.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="targetPath">The string representation of the Edm target path.</param>
        /// <param name="document">The Open API document to use to build references.</param>
        /// <returns></returns>
        public static IOpenApiParameter CreateFilter(this ODataContext context, string targetPath, OpenApiDocument document)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(targetPath, nameof(targetPath));

            IEdmTargetPath target = context.Model.GetTargetPath(targetPath);
            if (target == null)
                return null;

            return context.CreateFilter(target, document);
        }

        public static OpenApiParameter CreateOrderBy(this ODataContext context, string targetPath, IEdmEntityType entityType)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(targetPath, nameof(targetPath));

            IEdmTargetPath target = context.Model.GetTargetPath(targetPath);
            if (target == null)
                return null;

            return context.CreateOrderBy(target, entityType);
        }

        public static OpenApiParameter CreateOrderBy(this ODataContext context, IEdmEntitySet entitySet)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(entitySet, nameof(entitySet));

            return context.CreateOrderBy(entitySet, entitySet.EntityType);
        }

        public static OpenApiParameter CreateOrderBy(this ODataContext context, IEdmSingleton singleton)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(singleton, nameof(singleton));

            return context.CreateOrderBy(singleton, singleton.EntityType);
        }

        public static OpenApiParameter CreateOrderBy(this ODataContext context, IEdmNavigationProperty navigationProperty)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(navigationProperty, nameof(navigationProperty));

            return context.CreateOrderBy(navigationProperty, navigationProperty.ToEntityType());
        }

        /// <summary>
        /// Create $orderby parameter for the <see cref="IEdmEntitySet"/>.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="target">The Edm annotation target.</param>
        /// <param name="entityType">The Edm Entity type.</param>
        /// <returns>The created <see cref="OpenApiParameter"/> or null.</returns>
        public static OpenApiParameter CreateOrderBy(this ODataContext context, IEdmVocabularyAnnotatable target, IEdmEntityType entityType)
        {// patchwork to avoid breaking changes
            return context.CreateOrderBy(target, entityType as IEdmStructuredType);
        }

        public static OpenApiParameter CreateOrderBy(this ODataContext context, string targetPath, IEdmStructuredType structuredType)
        {
            IEdmTargetPath target = context.Model.GetTargetPath(targetPath);
            if (target == null)
                return null;

            return context.CreateOrderBy(target, structuredType);
        }

        public static OpenApiParameter CreateOrderBy(this ODataContext context, IEdmVocabularyAnnotatable target, IEdmStructuredType structuredType)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(target, nameof(target));
            Utils.CheckArgumentNull(structuredType, nameof(structuredType));

            SortRestrictionsType sort = context.Model.GetRecord<SortRestrictionsType>(target, CapabilitiesConstants.SortRestrictions);
            if (sort != null && !sort.IsSortable)
            {
                return null;
            }

            var orderByItems = new List<JsonNode>();
            foreach (var property in structuredType.StructuralProperties())
            {
                if (sort != null && sort.IsNonSortableProperty(property.Name))
                {
                    continue;
                }

                bool isAscOnly = sort != null && sort.IsAscendingOnlyProperty(property.Name);
                bool isDescOnly = sort != null && sort.IsDescendingOnlyProperty(property.Name);
                if (isAscOnly || isDescOnly)
                {
                    if (isAscOnly)
                    {
                        orderByItems.Add(property.Name);
                    }
                    else
                    {
                        orderByItems.Add(property.Name + " desc");
                    }
                }
                else
                {
                    orderByItems.Add(property.Name);
                    orderByItems.Add(property.Name + " desc");
                }
            }

            return new OpenApiParameter
            {
                Name = "$orderby",
                In = ParameterLocation.Query,
                Description = "Order items by property values",
                Schema = new OpenApiSchema
                {
                    Type = JsonSchemaType.Array,
                    UniqueItems = true,
                    Items = new OpenApiSchema
                    {
                        Type = JsonSchemaType.String,
                        Enum = context.Settings.UseStringArrayForQueryOptionsSchema ? null : orderByItems
                    }
                },
                Style = ParameterStyle.Form,
                Explode = false
            };
        }

        public static OpenApiParameter CreateSelect(this ODataContext context, string targetPath, IEdmEntityType entityType)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(targetPath, nameof(targetPath));

            IEdmTargetPath target = context.Model.GetTargetPath(targetPath);
            if (target == null)
                return null;

            return context.CreateSelect(target, entityType);
        }

        public static OpenApiParameter CreateSelect(this ODataContext context, IEdmEntitySet entitySet)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(entitySet, nameof(entitySet));

            return context.CreateSelect(entitySet, entitySet.EntityType);
        }

        public static OpenApiParameter CreateSelect(this ODataContext context, IEdmSingleton singleton)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(singleton, nameof(singleton));

            return context.CreateSelect(singleton, singleton.EntityType);
        }

        public static OpenApiParameter CreateSelect(this ODataContext context, IEdmNavigationProperty navigationProperty)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(navigationProperty, nameof(navigationProperty));

            return context.CreateSelect(navigationProperty, navigationProperty.ToEntityType());
        }

        /// <summary>
        /// Create $select parameter for the <see cref="IEdmVocabularyAnnotatable"/>.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="target">The Edm target.</param>
        /// <param name="entityType">The Edm entity type.</param>
        /// <returns>The created <see cref="OpenApiParameter"/> or null.</returns>
        public static OpenApiParameter CreateSelect(this ODataContext context, IEdmVocabularyAnnotatable target, IEdmEntityType entityType)
        { // patchwork to avoid breaking changes
            return context.CreateSelect(target, entityType as IEdmStructuredType);
        }

        public static OpenApiParameter CreateSelect(this ODataContext context, string targetPath, IEdmStructuredType structuredType)
        {
            IEdmTargetPath target = context.Model.GetTargetPath(targetPath);
            if (target == null)
                return null;

            return context.CreateSelect(target, structuredType);
        }

        public static OpenApiParameter CreateSelect(this ODataContext context, IEdmVocabularyAnnotatable target, IEdmStructuredType structuredType)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(target, nameof(target));
            Utils.CheckArgumentNull(structuredType, nameof(structuredType));

            NavigationRestrictionsType navigation = context.Model.GetRecord<NavigationRestrictionsType>(target, CapabilitiesConstants.NavigationRestrictions);
            if (navigation != null && !navigation.IsNavigable)
            {
                return null;
            }

            var selectItems = new List<JsonNode>();

            foreach (var property in structuredType.StructuralProperties())
            {
                selectItems.Add(property.Name);
            }

            foreach (var property in structuredType.NavigationProperties())
            {
                if (navigation != null && navigation.IsRestrictedProperty(property.Name))
                {
                    continue;
                }

                selectItems.Add(property.Name);
            }

            return new OpenApiParameter
            {
                Name = "$select",
                In = ParameterLocation.Query,
                Description = "Select properties to be returned",
                Schema = new OpenApiSchema
                {
                    Type = JsonSchemaType.Array,
                    UniqueItems = true,
                    Items = new OpenApiSchema
                    {
                        Type = JsonSchemaType.String,
                        Enum = context.Settings.UseStringArrayForQueryOptionsSchema ? null : selectItems
                    }
                },
                Style = ParameterStyle.Form,
                Explode = false
            };
        }

        public static OpenApiParameter CreateExpand(this ODataContext context, string targetPath, IEdmEntityType entityType)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(targetPath, nameof(targetPath));

            IEdmTargetPath target = context.Model.GetTargetPath(targetPath);
            if (target == null)
                return null;

            return context.CreateExpand(target, entityType);
        }

        public static OpenApiParameter CreateExpand(this ODataContext context, IEdmEntitySet entitySet)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(entitySet, nameof(entitySet));

            return context.CreateExpand(entitySet, entitySet.EntityType);
        }

        public static OpenApiParameter CreateExpand(this ODataContext context, IEdmSingleton singleton)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(singleton, nameof(singleton));

            return context.CreateExpand(singleton, singleton.EntityType);
        }

        public static OpenApiParameter CreateExpand(this ODataContext context, IEdmNavigationProperty navigationProperty)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(navigationProperty, nameof(navigationProperty));

            return context.CreateExpand(navigationProperty, navigationProperty.ToEntityType());
        }

        /// <summary>
        /// Create $expand parameter for the <see cref="IEdmNavigationSource"/>.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="target">The edm entity path.</param>
        /// <param name="entityType">The edm entity path.</param>
        /// <returns>The created <see cref="OpenApiParameter"/> or null.</returns>
        public static OpenApiParameter CreateExpand(this ODataContext context, IEdmVocabularyAnnotatable target, IEdmEntityType entityType)
        { // patchwork to avoid breaking changes
            return context.CreateExpand(target, entityType as IEdmStructuredType);
        }

        public static OpenApiParameter CreateExpand(this ODataContext context, string targetPath, IEdmStructuredType structuredType)
        {
            IEdmTargetPath target = context.Model.GetTargetPath(targetPath);
            if (target == null)
                return null;

            return context.CreateExpand(target, structuredType);
        }

        public static OpenApiParameter CreateExpand(this ODataContext context, IEdmVocabularyAnnotatable target, IEdmStructuredType structuredType)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(target, nameof(target));
            Utils.CheckArgumentNull(structuredType, nameof(structuredType));

            ExpandRestrictionsType expand = context.Model.GetRecord<ExpandRestrictionsType>(target, CapabilitiesConstants.ExpandRestrictions);
            if (expand != null && !expand.IsExpandable)
            {
                return null;
            }

            IList<JsonNode> expandItems = [ "*" ];

            foreach (var property in structuredType.NavigationProperties())
            {
                if (expand != null && expand.IsNonExpandableProperty(property.Name))
                {
                    continue;
                }

                expandItems.Add(property.Name);
            }

            return new OpenApiParameter
            {
                Name = "$expand",
                In = ParameterLocation.Query,
                Description = "Expand related entities",
                Schema = new OpenApiSchema
                {
                    Type = JsonSchemaType.Array,
                    UniqueItems = true,
                    Items = new OpenApiSchema
                    {
                        Type = JsonSchemaType.String,
                        Enum = context.Settings.UseStringArrayForQueryOptionsSchema ? null : expandItems
                    }
                },
                Style = ParameterStyle.Form,
                Explode = false
            };
        }

        // #top
        private static OpenApiParameter CreateTop(int topExample)
        {
            return new OpenApiParameter
            {
                Name = "$top",
                In = ParameterLocation.Query,
                Description = "Show only the first n items",
                Schema = new OpenApiSchema
                {
                    Type = JsonSchemaType.Number,
                    Format = "int64",
                    Minimum = 0,
                },
                Example = topExample,
                Style = ParameterStyle.Form,
                Explode = false
            };
        }

        // $skip
        private static OpenApiParameter CreateSkip()
        {
            return new OpenApiParameter
            {
                Name = "$skip",
                In = ParameterLocation.Query,
                Description = "Skip the first n items",
                Schema = new OpenApiSchema
                {
                    Type = JsonSchemaType.Number,
                    Format = "int64",
                    Minimum = 0,
                },
                Style = ParameterStyle.Form,
                Explode = false
            };
        }

        // $count
        private static OpenApiParameter CreateCount()
        {
            return new OpenApiParameter
            {
                Name = "$count",
                In = ParameterLocation.Query,
                Description = "Include count of items",
                Schema = new OpenApiSchema
                {
                    Type = JsonSchemaType.Boolean
                },
                Style = ParameterStyle.Form,
                Explode = false
            };
        }

        // $filter
        private static OpenApiParameter CreateFilter()
        {
            return new OpenApiParameter
            {
                Name = "$filter",
                In = ParameterLocation.Query,
                Description = "Filter items by property values",
                Schema = new OpenApiSchema
                {
                    Type = JsonSchemaType.String
                },
                Style = ParameterStyle.Form,
                Explode = false
            };
        }

        // $search
        private static OpenApiParameter CreateSearch()
        {
            return new OpenApiParameter
            {
                Name = "$search",
                In = ParameterLocation.Query,
                Description = "Search items by search phrases",
                Schema = new OpenApiSchema
                {
                    Type = JsonSchemaType.String
                },
                Style = ParameterStyle.Form,
                Explode = false
            };
        }
    }
}
