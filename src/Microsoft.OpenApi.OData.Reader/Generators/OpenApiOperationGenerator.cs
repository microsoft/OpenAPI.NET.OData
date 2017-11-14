// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;

namespace Microsoft.OpenApi.OData.Generators
{
    /// <summary>
    /// Extension methods to create <see cref="OpenApiOperation"/> by Edm elements.
    /// </summary>
    internal static class OpenApiOperationGenerator
    {
        /// <summary>
        /// The Path Item Object for the entity set contains the keyword get with an Operation Object as value
        /// that describes the capabilities for querying the entity set.
        /// </summary>
        /// <param name="entitySet">The entity set.</param>
        /// <returns>The created <see cref="OpenApiOperation"/>.</returns>
        public static OpenApiOperation CreateGetOperationForEntitySet(this IEdmEntitySet entitySet)
        {
            if (entitySet == null)
            {
                throw Error.ArgumentNull(nameof(entitySet));
            }

            OpenApiOperation operation = new OpenApiOperation
            {
                Summary = "Get entities from " + entitySet.Name,
                Tags = new List<OpenApiTag>
                {
                    new OpenApiTag
                    {
                        Name = entitySet.Name
                    }
                }
            };

            // The parameters array contains Parameter Objects for system query options allowed for this entity set,
            // and it does not list system query options not allowed for this entity set.
            operation.Parameters = new List<OpenApiParameter>
            {
                new OpenApiParameter
                {
                    Pointer = new OpenApiReference(ReferenceType.Parameter, "top")
                },
                new OpenApiParameter
                {
                    Pointer = new OpenApiReference(ReferenceType.Parameter, "skip")
                },
                new OpenApiParameter
                {
                    Pointer = new OpenApiReference(ReferenceType.Parameter, "search")
                },
                new OpenApiParameter
                {
                    Pointer = new OpenApiReference(ReferenceType.Parameter, "filter")
                },
                new OpenApiParameter
                {
                    Pointer = new OpenApiReference(ReferenceType.Parameter, "count")
                },

                // the syntax of the system query options $expand, $select, and $orderby is too flexible
                // to be formally described with OpenAPI Specification means, yet the typical use cases
                // of just providing a comma-separated list of properties can be expressed via an array-valued
                // parameter with an enum constraint
                entitySet.CreateOrderByParameter(),

                entitySet.CreateSelectParameter(),

                entitySet.CreateExpandParameter(),
            };

            // The value of responses is a Responses Object.
            // It contains a name/value pair for the success case (HTTP response code 200)
            // describing the structure of a successful response referencing the schema of the entity set’s entity type in the global schemas
            operation.Responses = new OpenApiResponses
            {
                {
                    "200",
                    new OpenApiResponse
                    {
                        Description = "Retrieved entities",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            {
                                "application/json",
                                new OpenApiMediaType
                                {
                                    Schema = new OpenApiSchema
                                    {
                                        Title = "Collection of " + entitySet.Name,
                                        Type = "object",
                                        Properties = new Dictionary<string, OpenApiSchema>
                                        {
                                            {
                                                "value",
                                                new OpenApiSchema
                                                {
                                                    Type = "array",
                                                    Items = new OpenApiSchema
                                                    {
                                                        Pointer = new OpenApiReference(ReferenceType.Schema, entitySet.EntityType().FullName())
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            operation.Responses.Add("default".GetResponse());

            return operation;
        }

        public static OpenApiOperation CreatePostOperationForEntitySet(this IEdmEntitySet entitySet)
        {
            OpenApiOperation operation = new OpenApiOperation
            {
                Summary = "Add new entity to " + entitySet.Name,
                Tags = new List<OpenApiTag>
                {
                    new OpenApiTag
                    {
                        Name = entitySet.Name
                    }
                },
                RequestBody = new OpenApiRequestBody
                {
                    Required = true,
                    Description = "New entity",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        {
                            "application/json", new OpenApiMediaType
                            {
                                Schema = new OpenApiSchema
                                {
                                    Pointer = new OpenApiReference("#/components/schemas/" + entitySet.EntityType().FullName())
                                }
                            }
                        }
                    }
                }
            };

            operation.Responses = new OpenApiResponses
            {
                {
                    "201",
                    new OpenApiResponse
                    {
                        Description = "Created entity",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            {
                                "application/json",
                                new OpenApiMediaType
                                {
                                    Schema = new OpenApiSchema
                                    {
                                        Pointer = new OpenApiReference("#/components/schemas/" + entitySet.EntityType().FullName())
                                    }
                                }
                            }
                        }
                    }
                }
            };
            operation.Responses.Add("default".GetResponse());

            return operation;
        }

        public static string CreatePathNameForEntity(this IEdmEntitySet entitySet)
        {
            string keyString;
            IList<IEdmStructuralProperty> keys = entitySet.EntityType().Key().ToList();
            if (keys.Count() == 1)
            {
                keyString = "{" + keys.First().Name + "}";
            }
            else
            {
                IList<string> temps = new List<string>();
                foreach (var keyProperty in entitySet.EntityType().Key())
                {
                    temps.Add(keyProperty.Name + "={" + keyProperty.Name + "}");
                }
                keyString = String.Join(",", temps);
            }

            return "/" + entitySet.Name + "('" + keyString + "')";
        }

        public static string CreatePathNameForSingleton(this IEdmSingleton singleton)
        {
            return "/" + singleton.Name;
        }

        public static OpenApiOperation CreateGetOperationForEntity(this IEdmEntitySet entitySet)
        {
            OpenApiOperation operation = new OpenApiOperation
            {
                Summary = "Get entity from " + entitySet.Name + " by key",
                Tags = new List<OpenApiTag>
                {
                    new OpenApiTag
                    {
                        Name = entitySet.Name
                    }
                }
            };

            operation.Parameters = entitySet.EntityType().CreateKeyParameters();

            operation.Parameters.Add(entitySet.CreateSelectParameter());

            operation.Parameters.Add(entitySet.CreateExpandParameter());

            operation.Responses = new OpenApiResponses
            {
                {
                    "200",
                    new OpenApiResponse
                    {
                        Description = "Retrieved entity",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            {
                                "application/json",
                                new OpenApiMediaType
                                {
                                    Schema = new OpenApiSchema
                                    {
                                        Pointer = new OpenApiReference(ReferenceType.Schema, entitySet.EntityType().FullName())
                                    }
                                }
                            }
                        }
                    }
                }
            };
            operation.Responses.Add("default".GetResponse());

            return operation;
        }

        public static OpenApiOperation CreateGetOperationForSingleton(this IEdmSingleton singleton)
        {
            OpenApiOperation operation = new OpenApiOperation
            {
                Summary = "Get " + singleton.Name,
                Tags = new List<OpenApiTag>
                {
                    new OpenApiTag
                    {
                        Name = singleton.Name
                    }
                }
            };

            operation.Parameters = new List<OpenApiParameter>();
            operation.Parameters.Add(singleton.CreateSelectParameter());

            operation.Parameters.Add(singleton.CreateExpandParameter());

            operation.Responses = new OpenApiResponses
            {
                {
                    "200",
                    new OpenApiResponse
                    {
                        Description = "Retrieved entity",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            {
                                "application/json",
                                new OpenApiMediaType
                                {
                                    Schema = new OpenApiSchema
                                    {
                                        Pointer = new OpenApiReference(ReferenceType.Schema, singleton.EntityType().FullName())
                                    }
                                }
                            }
                        }
                    }
                },
            };
            operation.Responses.Add("default".GetResponse());

            return operation;
        }

        public static OpenApiOperation CreatePatchOperationForEntity(this IEdmEntitySet entitySet)
        {
            OpenApiOperation operation = new OpenApiOperation
            {
                Summary = "Update entity in " + entitySet.Name,
                Tags = new List<OpenApiTag>
                {
                    new OpenApiTag
                    {
                        Name= entitySet.Name
                    }
                }
            };

            operation.Parameters = entitySet.EntityType().CreateKeyParameters();

            operation.RequestBody = new OpenApiRequestBody
            {
                Required = true,
                Description = "New property values",
                Content = new Dictionary<string, OpenApiMediaType>
                    {
                        {
                            "application/json", new OpenApiMediaType
                            {
                                Schema = new OpenApiSchema
                                {
                                    Pointer = new OpenApiReference(ReferenceType.Schema, entitySet.EntityType().FullName())
                                }
                            }
                        }
                    }
            };

            operation.Responses = new OpenApiResponses
            {
                "204".GetResponse(),
                "default".GetResponse()
            };
            return operation;
        }

        public static OpenApiOperation CreatePatchOperationForSingleton(this IEdmSingleton singleton)
        {
            OpenApiOperation operation = new OpenApiOperation
            {
                Summary = "Update " + singleton.Name,
                Tags = new List<OpenApiTag>
                {
                    new OpenApiTag
                    {
                        Name = singleton.Name
                    }
                }
            };

            operation.RequestBody = new OpenApiRequestBody
            {
                Required = true,
                Description = "New property values",
                Content = new Dictionary<string, OpenApiMediaType>
                    {
                        {
                            "application/json", new OpenApiMediaType
                            {
                                Schema = new OpenApiSchema
                                {
                                    Pointer = new OpenApiReference(ReferenceType.Schema, singleton.EntityType().FullName())
                                }
                            }
                        }
                    }
            };

            operation.Responses = new OpenApiResponses
            {
                "204".GetResponse(),
                "default".GetResponse()
            };
            return operation;
        }

        public static OpenApiOperation CreateDeleteOperationForEntity(this IEdmEntitySet entitySet)
        {
            OpenApiOperation operation = new OpenApiOperation
            {
                Summary = "Delete entity from " + entitySet.Name,
                Tags = new List<OpenApiTag>
                {
                    new OpenApiTag
                    {
                        Name = entitySet.Name
                    }
                }
            };
            operation.Parameters = entitySet.EntityType().CreateKeyParameters();
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "If-Match",
                In = ParameterLocation.Header,
                Description = "ETag",
                Schema = new OpenApiSchema
                {
                    Type = "string"
                }
            });

            operation.Responses = new OpenApiResponses
            {
                "204".GetResponse(),
                "default".GetResponse()
            };
            return operation;
        }
    }
}
