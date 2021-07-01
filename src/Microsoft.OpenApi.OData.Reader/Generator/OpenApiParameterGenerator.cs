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
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Vocabulary;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;

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
        /// <returns>The created map of <see cref="OpenApiParameter"/> object.</returns>
        public static IDictionary<string, OpenApiParameter> CreateParameters(this ODataContext context)
        {
            Utils.CheckArgumentNull(context, nameof(context));

            // It allows defining query options and headers that can be reused across operations of the service.
            // The value of parameters is a map of Parameter Objects.
            return new Dictionary<string, OpenApiParameter>
            {
                { "top", CreateTop(context.Settings.TopExample) },
                { "skip", CreateSkip() },
                { "count", CreateCount() },
                { "filter", CreateFilter() },
                { "search", CreateSearch() },
            };
        }

        /// <summary>
        /// Create the list of <see cref="OpenApiParameter"/> for a <see cref="IEdmFunctionImport"/>.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="functionImport">The Edm function import.</param>
        /// <returns>The created list of <see cref="OpenApiParameter"/>.</returns>
        public static IList<OpenApiParameter> CreateParameters(this ODataContext context, IEdmFunctionImport functionImport)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(functionImport, nameof(functionImport));

            return context.CreateParameters(functionImport.Function);
        }

        /// <summary>
        /// Create the list of <see cref="OpenApiParameter"/> for a <see cref="IEdmFunction"/>.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="function">The Edm function.</param>
        /// <param name="parameterNameMapping">The parameter name mapping.</param>
        /// <returns>The created list of <see cref="OpenApiParameter"/>.</returns>
        public static IList<OpenApiParameter> CreateParameters(this ODataContext context, IEdmFunction function,
            IDictionary<string, string> parameterNameMapping = null)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(function, nameof(function));

            IList<OpenApiParameter> parameters = new List<OpenApiParameter>();
            int skip = function.IsBound ? 1 : 0;
            foreach (IEdmOperationParameter edmParameter in function.Parameters.Skip(skip))
            {
                if (parameterNameMapping != null)
                {
                    if (!parameterNameMapping.ContainsKey(edmParameter.Name))
                    {
                        continue;
                    }
                }

                OpenApiParameter parameter;
                // Structured or collection-valued parameters are represented as a parameter alias
                // in the path template and the parameters array contains a Parameter Object for
                // the parameter alias as a query option of type string.
                if (edmParameter.Type.IsStructured() ||
                    edmParameter.Type.IsCollection())
                {
                    parameter = new OpenApiParameter
                    {
                        Name = parameterNameMapping == null ? edmParameter.Name : parameterNameMapping[edmParameter.Name],
                        In = ParameterLocation.Query, // as query option
                        Required = true,
                        Schema = new OpenApiSchema
                        {
                            Type = "string",

                            // These Parameter Objects optionally can contain the field description,
                            // whose value is the value of the unqualified annotation Core.Description of the parameter.
                            Description = context.Model.GetDescriptionAnnotation(edmParameter)
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
                        In = ParameterLocation.Path,
                        Required = true,
                        Schema = context.CreateEdmTypeSchema(edmParameter.Type)
                    };
                }

                if (parameterNameMapping != null)
                {
                    parameter.Description = $"Usage: {edmParameter.Name}={{{parameterNameMapping[edmParameter.Name]}}}";
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
        /// <param name="parameterNameMapping">The parameter name mapping.</param>
        /// <returns>The created the list of <see cref="OpenApiParameter"/>.</returns>
        public static IList<OpenApiParameter> CreateKeyParameters(this ODataContext context, ODataKeySegment keySegment,
            IDictionary<string, string> parameterNameMapping = null)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(keySegment, nameof(keySegment));

            IList<OpenApiParameter> parameters = new List<OpenApiParameter>();
            IEdmEntityType entityType = keySegment.EntityType;

            IList<IEdmStructuralProperty> keys = entityType.Key().ToList();
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
                    Name = parameterNameMapping == null ? keyName: parameterNameMapping[keyName],
                    In = ParameterLocation.Path,
                    Required = true,
                    Description = "key: " + keyName + " of " + entityType.Name,
                    Schema = context.CreateEdmTypeSchema(keys.First().Type)
                };

                parameter.Extensions.Add(Constants.xMsKeyType, new OpenApiString(entityType.Name));
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
                            keyProperty.Name:
                            parameterNameMapping[keyProperty.Name],// By design: not prefix with type name if enable type name prefix
                        In = ParameterLocation.Path,
                        Required = true,
                        Description = "key: " + keyProperty.Name + " of " + entityType.Name,
                        Schema = context.CreateEdmTypeSchema(keyProperty.Type)
                    };

                    if (keySegment.KeyMappings != null)
                    {
                        parameter.Description = parameter.Description + $", {keyProperty.Name}={{{parameter.Name}}}";
                    }

                    parameter.Extensions.Add(Constants.xMsKeyType, new OpenApiString(entityType.Name));
                    parameters.Add(parameter);
                }
            }

            return parameters;
        }

        /// <summary>
        /// Create the $top parameter.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="target">The Edm annotation target.</param>
        /// <returns>The created <see cref="OpenApiParameter"/> or null.</returns>
        public static OpenApiParameter CreateTop(this ODataContext context, IEdmVocabularyAnnotatable target)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(target, nameof(target));

            bool? top = context.Model.GetBoolean(target, CapabilitiesConstants.TopSupported);
            if (top == null || top.Value)
            {
                return new OpenApiParameter
                {
                    Reference = new OpenApiReference { Type = ReferenceType.Parameter, Id = "top" }
                };
            }

            return null;
        }

        /// <summary>
        /// Create the $skip parameter.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="target">The Edm annotation target.</param>
        /// <returns>The created <see cref="OpenApiParameter"/> or null.</returns>
        public static OpenApiParameter CreateSkip(this ODataContext context, IEdmVocabularyAnnotatable target)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(target, nameof(target));

            bool? skip = context.Model.GetBoolean(target, CapabilitiesConstants.SkipSupported);
            if (skip == null || skip.Value)
            {
                return new OpenApiParameter
                {
                    Reference = new OpenApiReference { Type = ReferenceType.Parameter, Id = "skip" }
                };
            }

            return null;
        }

        /// <summary>
        /// Create the $search parameter.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="target">The Edm annotation target.</param>
        /// <returns>The created <see cref="OpenApiParameter"/> or null.</returns>
        public static OpenApiParameter CreateSearch(this ODataContext context, IEdmVocabularyAnnotatable target)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(target, nameof(target));

            SearchRestrictionsType search = context.Model.GetRecord<SearchRestrictionsType>(target, CapabilitiesConstants.SearchRestrictions);
            if (search == null || search.IsSearchable)
            {
                return new OpenApiParameter
                {
                    Reference = new OpenApiReference { Type = ReferenceType.Parameter, Id = "search" }
                };
            }

            return null;
        }

        /// <summary>
        /// Create the $count parameter.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="target">The Edm annotation target.</param>
        /// <returns>The created <see cref="OpenApiParameter"/> or null.</returns>
        public static OpenApiParameter CreateCount(this ODataContext context, IEdmVocabularyAnnotatable target)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(target, nameof(target));

            CountRestrictionsType count = context.Model.GetRecord<CountRestrictionsType>(target, CapabilitiesConstants.CountRestrictions);
            if (count == null || count.IsCountable)
            {
                return new OpenApiParameter
                {
                    Reference = new OpenApiReference { Type = ReferenceType.Parameter, Id = "count" }
                };
            }

            return null;
        }

        /// <summary>
        /// Create the $filter parameter.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="target">The Edm annotation target.</param>
        /// <returns>The created <see cref="OpenApiParameter"/> or null.</returns>
        public static OpenApiParameter CreateFilter(this ODataContext context, IEdmVocabularyAnnotatable target)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(target, nameof(target));

            FilterRestrictionsType filter = context.Model.GetRecord<FilterRestrictionsType>(target, CapabilitiesConstants.FilterRestrictions);
            if (filter == null || filter.IsFilterable)
            {
                return new OpenApiParameter
                {
                    Reference = new OpenApiReference { Type = ReferenceType.Parameter, Id = "filter" }
                };
            }

            return null;
        }

        public static OpenApiParameter CreateOrderBy(this ODataContext context, IEdmEntitySet entitySet)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(entitySet, nameof(entitySet));

            return context.CreateOrderBy(entitySet, entitySet.EntityType());
        }

        public static OpenApiParameter CreateOrderBy(this ODataContext context, IEdmSingleton singleton)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(singleton, nameof(singleton));

            return context.CreateOrderBy(singleton, singleton.EntityType());
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
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(target, nameof(target));
            Utils.CheckArgumentNull(entityType, nameof(entityType));

            SortRestrictionsType sort = context.Model.GetRecord<SortRestrictionsType>(target, CapabilitiesConstants.SortRestrictions);
            if (sort != null && !sort.IsSortable)
            {
                return null;
            }

            IList<IOpenApiAny> orderByItems = new List<IOpenApiAny>();
            foreach (var property in entityType.StructuralProperties())
            {
                if (sort != null && sort.IsNonSortableProperty(property.Name))
                {
                    continue;
                }

                bool isAscOnly = sort != null ? sort.IsAscendingOnlyProperty(property.Name) : false ;
                bool isDescOnly = sort != null ? sort.IsDescendingOnlyProperty(property.Name) : false;
                if (isAscOnly || isDescOnly)
                {
                    if (isAscOnly)
                    {
                        orderByItems.Add(new OpenApiString(property.Name));
                    }
                    else
                    {
                        orderByItems.Add(new OpenApiString(property.Name + " desc"));
                    }
                }
                else
                {
                    orderByItems.Add(new OpenApiString(property.Name));
                    orderByItems.Add(new OpenApiString(property.Name + " desc"));
                }
            }

            return new OpenApiParameter
            {
                Name = "$orderby",
                In = ParameterLocation.Query,
                Description = "Order items by property values",
                Schema = new OpenApiSchema
                {
                    Type = "array",
                    UniqueItems = true,
                    Items = new OpenApiSchema
                    {
                        Type = "string",
                        Enum = orderByItems
                    }
                },
                Style = ParameterStyle.Form,
                Explode = false
            };
        }

        public static OpenApiParameter CreateSelect(this ODataContext context, IEdmEntitySet entitySet)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(entitySet, nameof(entitySet));

            return context.CreateSelect(entitySet, entitySet.EntityType());
        }

        public static OpenApiParameter CreateSelect(this ODataContext context, IEdmSingleton singleton)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(singleton, nameof(singleton));

            return context.CreateSelect(singleton, singleton.EntityType());
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
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(target, nameof(target));
            Utils.CheckArgumentNull(entityType, nameof(entityType));

            NavigationRestrictionsType navigation = context.Model.GetRecord<NavigationRestrictionsType>(target, CapabilitiesConstants.NavigationRestrictions);
            if (navigation != null && !navigation.IsNavigable)
            {
                return null;
            }

            IList<IOpenApiAny> selectItems = new List<IOpenApiAny>();

            foreach (var property in entityType.StructuralProperties())
            {
                selectItems.Add(new OpenApiString(property.Name));
            }

            foreach (var property in entityType.NavigationProperties())
            {
                if (navigation != null && navigation.IsRestrictedProperty(property.Name))
                {
                    continue;
                }

                selectItems.Add(new OpenApiString(property.Name));
            }

            return new OpenApiParameter
            {
                Name = "$select",
                In = ParameterLocation.Query,
                Description = "Select properties to be returned",
                Schema = new OpenApiSchema
                {
                    Type = "array",
                    UniqueItems = true,
                    Items = new OpenApiSchema
                    {
                        Type = "string",
                        Enum = selectItems
                    }
                },
                Style = ParameterStyle.Form,
                Explode = false
            };
        }

        public static OpenApiParameter CreateExpand(this ODataContext context, IEdmEntitySet entitySet)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(entitySet, nameof(entitySet));

            return context.CreateExpand(entitySet, entitySet.EntityType());
        }

        public static OpenApiParameter CreateExpand(this ODataContext context, IEdmSingleton singleton)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(singleton, nameof(singleton));

            return context.CreateExpand(singleton, singleton.EntityType());
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
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(target, nameof(target));
            Utils.CheckArgumentNull(entityType, nameof(entityType));

            ExpandRestrictionsType expand = context.Model.GetRecord<ExpandRestrictionsType>(target, CapabilitiesConstants.ExpandRestrictions);
            if (expand != null && !expand.IsExpandable)
            {
                return null;
            }

            IList<IOpenApiAny> expandItems = new List<IOpenApiAny>
            {
                new OpenApiString("*")
            };

            foreach (var property in entityType.NavigationProperties())
            {
                if (expand != null && expand.IsNonExpandableProperty(property.Name))
                {
                    continue;
                }

                expandItems.Add(new OpenApiString(property.Name));
            }

            return new OpenApiParameter
            {
                Name = "$expand",
                In = ParameterLocation.Query,
                Description = "Expand related entities",
                Schema = new OpenApiSchema
                {
                    Type = "array",
                    UniqueItems = true,
                    Items = new OpenApiSchema
                    {
                        Type = "string",
                        Enum = expandItems
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
                    Type = "integer",
                    Minimum = 0,
                },
                Example = new OpenApiInteger(topExample)
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
                    Type = "integer",
                    Minimum = 0,
                }
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
                    Type = "boolean"
                }
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
                    Type = "string"
                }
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
                    Type = "string"
                }
            };
        }
    }
}
