// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

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
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            // It allows defining query options and headers that can be reused across operations of the service.
            // The value of parameters is a map of Parameter Objects.
            return new Dictionary<string, OpenApiParameter>
            {
                { "top", CreateTop() },
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
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            if (functionImport == null)
            {
                throw Error.ArgumentNull(nameof(functionImport));
            }

            return context.CreateParameters(functionImport.Function);
        }

        /// <summary>
        /// Create the list of <see cref="OpenApiParameter"/> for a <see cref="IEdmFunction"/>.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="function">The Edm function.</param>
        /// <returns>The created list of <see cref="OpenApiParameter"/>.</returns>
        public static IList<OpenApiParameter> CreateParameters(this ODataContext context, IEdmFunction function)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            if (function == null)
            {
                throw Error.ArgumentNull(nameof(function));
            }

            IList<OpenApiParameter> parameters = new List<OpenApiParameter>();
            int skip = function.IsBound ? 1 : 0;
            foreach (IEdmOperationParameter edmParameter in function.Parameters.Skip(skip))
            {
                // Structured or collection-valued parameters are represented as a parameter alias
                // in the path template and the parameters array contains a Parameter Object for
                // the parameter alias as a query option of type string.
                if (edmParameter.Type.IsStructured() ||
                    edmParameter.Type.IsCollection())
                {
                    parameters.Add(new OpenApiParameter
                    {
                        Name = edmParameter.Name,
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
                    });
                }
                else
                {
                    // Primitive parameters use the same type mapping as described for primitive properties.
                    parameters.Add(new OpenApiParameter
                    {
                        Name = edmParameter.Name,
                        In = ParameterLocation.Path,
                        Required = true,
                        Schema = context.CreateEdmTypeSchema(edmParameter.Type)
                    });
                }
            }

            return parameters;
        }

        /// <summary>
        /// Create key parameters for the <see cref="IEdmEntityType"/>.
        /// </summary>
        /// <param name="entityType">The entity type.</param>
        /// <returns>The created the list of <see cref="OpenApiParameter"/>.</returns>
        public static IList<OpenApiParameter> CreateKeyParameters(this ODataContext context, IEdmEntityType entityType)
        {
            if (entityType == null)
            {
                throw Error.ArgumentNull(nameof(entityType));
            }

            IList<OpenApiParameter> parameters = new List<OpenApiParameter>();

            // append key parameter
            foreach (var keyProperty in entityType.Key())
            {
                OpenApiParameter parameter = new OpenApiParameter
                {
                    Name = keyProperty.Name,
                    In = ParameterLocation.Path,
                    Required = true,
                    Description = "key: " + keyProperty.Name,
                    Schema = context.CreateEdmTypeSchema(keyProperty.Type)
                };

                parameters.Add(parameter);
            }

            return parameters;
        }

        /// <summary>
        /// Create $orderby parameter for the <see cref="IEdmEntitySet"/>.
        /// </summary>
        /// <param name="entitySet">The entity set.</param>
        /// <returns>The created <see cref="OpenApiParameter"/>.</returns>
        public static OpenApiParameter CreateOrderByParameter(this IEdmEntitySet entitySet)
        {
            if (entitySet == null)
            {
                throw Error.ArgumentNull(nameof(entitySet));
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
                        Enum = VisitOrderbyItems(entitySet.EntityType())
                    }
                }
            };
        }

        /// <summary>
        /// Create $select parameter for the <see cref="IEdmNavigationSource"/>.
        /// </summary>
        /// <param name="navigationSource">The navigation source.</param>
        /// <returns>The created <see cref="OpenApiParameter"/>.</returns>
        public static OpenApiParameter CreateSelectParameter(this IEdmNavigationSource navigationSource)
        {
            if (navigationSource == null)
            {
                throw Error.ArgumentNull(nameof(navigationSource));
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
                        Enum = VisitSelectItems(navigationSource.EntityType())
                    }
                }
            };
        }

        /// <summary>
        /// Create $expand parameter for the <see cref="IEdmNavigationSource"/>.
        /// </summary>
        /// <param name="navigationSource">The navigation source.</param>
        /// <returns>The created <see cref="OpenApiParameter"/>.</returns>
        public static OpenApiParameter CreateExpandParameter(this IEdmNavigationSource navigationSource)
        {
            if (navigationSource == null)
            {
                throw Error.ArgumentNull(nameof(navigationSource));
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
                        Enum = VisitExpandItems(navigationSource.EntityType())
                    }
                }
            };
        }

        // #top
        private static OpenApiParameter CreateTop()
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
                Example = new OpenApiInteger(50)
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

        private static IList<IOpenApiAny> VisitOrderbyItems(IEdmEntityType entityType)
        {
            IList<IOpenApiAny> orderByItems = new List<IOpenApiAny>();

            foreach (var property in entityType.StructuralProperties())
            {
                orderByItems.Add(new OpenApiString(property.Name));
                orderByItems.Add(new OpenApiString(property.Name + " desc"));
            }

            return orderByItems;
        }

        private static IList<IOpenApiAny> VisitSelectItems(IEdmEntityType entityType)
        {
            IList<IOpenApiAny> selectItems = new List<IOpenApiAny>();

            foreach (var property in entityType.StructuralProperties())
            {
                selectItems.Add(new OpenApiString(property.Name));
            }

            return selectItems;
        }

        private static IList<IOpenApiAny> VisitExpandItems(IEdmEntityType entityType)
        {
            IList<IOpenApiAny> expandItems = new List<IOpenApiAny>
            {
                new OpenApiString("*")
            };

            foreach (var property in entityType.NavigationProperties())
            {
                expandItems.Add(new OpenApiString(property.Name));
            }

            return expandItems;
        }
    }
}
