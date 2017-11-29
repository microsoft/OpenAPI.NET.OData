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
    /// Extension methods to create <see cref="OpenApiParameter"/> from Edm elements.
    /// </summary>
    internal static class ParameterExtensions
    {
        /// <summary>
        /// Create key parameters for the <see cref="IEdmEntityType"/>.
        /// </summary>
        /// <param name="entityType">The entity type.</param>
        /// <returns>The created the list of <see cref="OpenApiParameter"/>.</returns>
        public static IList<OpenApiParameter> CreateKeyParameters(this IEdmEntityType entityType)
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
                    Schema = keyProperty.Type.CreateSchema()
                };

                parameters.Add(parameter);
            }

            return parameters;
        }

        public static IList<OpenApiParameter> CreateParameters(this IEdmOperation operation)
        {
            IList<OpenApiParameter> parameters = new List<OpenApiParameter>();

            foreach (IEdmOperationParameter edmParameter in operation.Parameters)
            {
                parameters.Add(new OpenApiParameter
                {
                    Name = edmParameter.Name,
                    In = ParameterLocation.Path,
                    Required = true,
                    Schema = edmParameter.Type.CreateSchema()
                });
            }

            return parameters;
        }

        public static IList<OpenApiParameter> CreateParameters(this ODataContext context, IEdmFunction function)
        {
            IList<OpenApiParameter> parameters = new List<OpenApiParameter>();

            foreach (IEdmOperationParameter edmParameter in function.Parameters.Skip(1))
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
                        Schema = edmParameter.Type.CreateSchema()
                    });
                }
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
