// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;

namespace Microsoft.OpenApi.OData.Generator
{
    /// <summary>
    /// Extension methods to create <see cref="OpenApiOperation"/> by Edm elements.
    /// </summary>
    internal static class OpenApiOperationGenerator
    {
        #region EntitySetOperations
        /// <summary>
        /// Query a Collection of Entities
        /// The Path Item Object for the entity set contains the keyword get with an Operation Object as value
        /// that describes the capabilities for querying the entity set.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="entitySet">The entity set.</param>
        /// <returns>The created <see cref="OpenApiOperation"/>.</returns>
        public static OpenApiOperation CreateEntitySetGetOperation(this ODataContext context, IEdmEntitySet entitySet)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            if (entitySet == null)
            {
                throw Error.ArgumentNull(nameof(entitySet));
            }

            OpenApiOperation operation = new OpenApiOperation
            {
                Summary = "Get entities from " + entitySet.Name,

                // The tags array of the Operation Object includes the entity set name.
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
                    Reference = new OpenApiReference { Type = ReferenceType.Parameter, Id = "top" }
                },
                new OpenApiParameter
                {
                    Reference = new OpenApiReference { Type = ReferenceType.Parameter, Id = "skip" }
                },
                new OpenApiParameter
                {
                    Reference = new OpenApiReference { Type = ReferenceType.Parameter, Id = "search" }
                },
                new OpenApiParameter
                {
                    Reference = new OpenApiReference { Type = ReferenceType.Parameter, Id = "filter" }
                },
                new OpenApiParameter
                {
                    Reference = new OpenApiReference { Type = ReferenceType.Parameter, Id = "count" }
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
                                                        Reference = new OpenApiReference
                                                        {
                                                            Type = ReferenceType.Schema,
                                                            Id = entitySet.EntityType().FullName()
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
                }
            };

            operation.Responses.Add("default", "default".GetResponse());

            return operation;
        }

        /// <summary>
        /// Create an Entity:
        /// The Path Item Object for the entity set contains the keyword post with an Operation Object as value
        /// that describes the capabilities for creating new entities.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="entitySet">The entity set.</param>
        /// <returns>The created <see cref="OpenApiOperation"/>.</returns>
        public static OpenApiOperation CreateEntitySetPostOperation(this ODataContext context, IEdmEntitySet entitySet)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            if (entitySet == null)
            {
                throw Error.ArgumentNull(nameof(entitySet));
            }

            OpenApiOperation operation = new OpenApiOperation
            {
                Summary = "Add new entity to " + entitySet.Name,

                // The tags array of the Operation Object includes the entity set name.
                Tags = new List<OpenApiTag>
                {
                    new OpenApiTag
                    {
                        Name = entitySet.Name
                    }
                },

                Parameters = null,

                // The requestBody field contains a Request Body Object for the request body
                // that references the schema of the entity set’s entity type in the global schemas.
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
                                    Reference = new OpenApiReference
                                    {
                                        Type = ReferenceType.Schema,
                                        Id = entitySet.EntityType().FullName()
                                    }
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
                                        Reference = new OpenApiReference
                                        {
                                            Type = ReferenceType.Schema,
                                            Id = entitySet.EntityType().FullName()
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
            operation.Responses.Add("default", "default".GetResponse());

            return operation;
        }
        #endregion

        #region EntityOperations
        /// <summary>
        /// Retrieve an Entity:
        /// The Path Item Object for the entity contains the keyword get with an Operation Object as value
        /// that describes the capabilities for retrieving a single entity.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="entitySet">The entity set.</param>
        /// <returns>The created <see cref="OpenApiOperation"/>.</returns>
        public static OpenApiOperation CreateEntityGetOperation(this ODataContext context, IEdmEntitySet entitySet)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            if (entitySet == null)
            {
                throw Error.ArgumentNull(nameof(entitySet));
            }

            OpenApiOperation operation = new OpenApiOperation
            {
                Summary = "Get entity from " + entitySet.Name + " by key",
                Tags = new List<OpenApiTag>
                {
                    new OpenApiTag
                    {
                        Name = entitySet.Name
                    }
                },
                RequestBody = null
            };

            operation.Parameters = context.CreateKeyParameters(entitySet.EntityType());

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
                                        Reference = new OpenApiReference
                                        {
                                            Type = ReferenceType.Schema,
                                            Id = entitySet.EntityType().FullName()
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
            operation.Responses.Add("default", "default".GetResponse());

            return operation;
        }

        /// <summary>
        /// Update an Entity
        /// The Path Item Object for the entity set contains the keyword patch with an Operation Object as value
        /// that describes the capabilities for updating the entity.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="singleton">The singleton.</param>
        /// <returns>The created <see cref="OpenApiOperation"/>.</returns>
        public static OpenApiOperation CreateEntityPatchOperation(this ODataContext context, IEdmEntitySet entitySet)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            if (entitySet == null)
            {
                throw Error.ArgumentNull(nameof(entitySet));
            }

            OpenApiOperation operation = new OpenApiOperation
            {
                Summary = "Update entity in " + entitySet.Name,

                // The tags array of the Operation Object includes the entity set name.
                Tags = new List<OpenApiTag>
                {
                    new OpenApiTag
                    {
                        Name= entitySet.Name
                    }
                }
            };

            operation.Parameters = context.CreateKeyParameters(entitySet.EntityType());

            // TODO: If the entity set uses optimistic concurrency control,
            // i.e. requires ETags for modification operations, the parameters array contains
            // a Parameter Object for the If-Match header.

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
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.Schema,
                                    Id = entitySet.EntityType().FullName()
                                }
                            }
                        }
                    }
                }
            };

            operation.Responses = new OpenApiResponses
            {
                { "204", "204".GetResponse() },
                { "default", "default".GetResponse() }
            };

            return operation;
        }

        /// <summary>
        /// Delete an Entity
        /// The Path Item Object for the entity set contains the keyword delete with an Operation Object as value
        /// that describes the capabilities for deleting the entity.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="singleton">The singleton.</param>
        /// <returns>The created <see cref="OpenApiOperation"/>.</returns>
        public static OpenApiOperation CreateEntityDeleteOperation(this ODataContext context, IEdmEntitySet entitySet)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            if (entitySet == null)
            {
                throw Error.ArgumentNull(nameof(entitySet));
            }

            OpenApiOperation operation = new OpenApiOperation
            {
                Summary = "Delete entity from " + entitySet.Name,

                // The tags array of the Operation Object includes the entity set name.
                Tags = new List<OpenApiTag>
                {
                    new OpenApiTag
                    {
                        Name = entitySet.Name
                    }
                },
                RequestBody = null
            };

            operation.Parameters = context.CreateKeyParameters(entitySet.EntityType());
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
                { "204", "204".GetResponse() },
                { "default", "default".GetResponse() }
            };
            return operation;
        }
        #endregion

        #region SingletonOperations
        /// <summary>
        /// Retrieve a Singleton
        /// The Path Item Object for the entity set contains the keyword get with an Operation Object as value
        /// that describes the capabilities for retrieving the singleton.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="singleton">The singleton.</param>
        /// <returns>The created <see cref="OpenApiOperation"/>.</returns>
        public static OpenApiOperation CreateSingletonGetOperation(this ODataContext context, IEdmSingleton singleton)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            if (singleton == null)
            {
                throw Error.ArgumentNull(nameof(singleton));
            }

            OpenApiOperation operation = new OpenApiOperation
            {
                Summary = "Get " + singleton.Name,

                // The tags array of the Operation Object includes the singleton’s name.
                Tags = new List<OpenApiTag>
                {
                    new OpenApiTag
                    {
                        Name = singleton.Name
                    }
                },
                RequestBody = null
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
                                        Reference = new OpenApiReference
                                        {
                                            Type = ReferenceType.Schema,
                                            Id = singleton.EntityType().FullName()
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
            };
            operation.Responses.Add("default", "default".GetResponse());

            return operation;
        }

        /// <summary>
        /// Update a Singleton
        /// The Path Item Object for the entity set contains the keyword patch with an Operation Object as value
        /// that describes the capabilities for updating the singleton, unless the singleton is read-only.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="singleton">The singleton.</param>
        /// <returns>The created <see cref="OpenApiOperation"/>.</returns>
        public static OpenApiOperation CreateSingletonPatchOperation(this ODataContext context, IEdmSingleton singleton)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            if (singleton == null)
            {
                throw Error.ArgumentNull(nameof(singleton));
            }

            OpenApiOperation operation = new OpenApiOperation
            {
                Summary = "Update " + singleton.Name,

                // The tags array of the Operation Object includes the singleton’s name.
                Tags = new List<OpenApiTag>
                {
                    new OpenApiTag
                    {
                        Name = singleton.Name
                    }
                },
                Parameters = null
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
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.Schema,
                                    Id = singleton.EntityType().FullName()
                                }
                            }
                        }
                    }
                }
            };

            operation.Responses = new OpenApiResponses
            {
                { "204", "204".GetResponse() },
                { "default", "default".GetResponse() }
            };

            return operation;
        }
        #endregion

        #region EdmOperation Operations

        /// <summary>
        /// Create a <see cref="OpenApiOperation"/> for a <see cref="IEdmOperation"/>.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="navigationSource">The binding navigation source.</param>
        /// <param name="edmOperation">The Edm operation.</param>
        /// <returns>The created <see cref="OpenApiOperation"/>.</returns>
        public static OpenApiOperation CreateOperation(this ODataContext context, IEdmNavigationSource navigationSource,
            IEdmOperation edmOperation)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            if (navigationSource == null)
            {
                throw Error.ArgumentNull(nameof(navigationSource));
            }

            if (edmOperation == null)
            {
                throw Error.ArgumentNull(nameof(edmOperation));
            }

            OpenApiOperation operation = new OpenApiOperation();
            operation.Summary = "Invoke " + (edmOperation.IsAction() ? "action " : "function ") + edmOperation.Name;

            // The tags array of the Operation Object includes the entity set name.
            operation.Tags = new List<OpenApiTag>
            {
                new OpenApiTag { Name = navigationSource.Name }
            };

            // For actions and functions bound to a single entity within an entity
            // set the parameters array contains a Parameter Object for each key property,
            // using the same type mapping as described for primitive properties.
            IEdmSingleton singleton = navigationSource as IEdmSingleton;
            if (singleton == null && edmOperation.IsBound)
            {
                IEdmOperationParameter bindingParameter = edmOperation.Parameters.FirstOrDefault();
                if (bindingParameter !=null &&
                    !bindingParameter.Type.IsCollection() && // bound to a single entity
                    bindingParameter.Type.IsEntity())
                {
                    operation.Parameters = context.CreateKeyParameters(bindingParameter
                        .Type.AsEntity().EntityDefinition());
                }
            }

            if (edmOperation.IsFunction())
            {
                // For bound functions, the parameters array contains a Parameter Object for each non-binding parameter.
                IEdmFunction function = (IEdmFunction)edmOperation;
                IList<OpenApiParameter> parameters = context.CreateParameters(function);
                if (operation.Parameters == null)
                {
                    operation.Parameters = parameters;
                }
                else
                {
                    foreach(var parameter in parameters)
                    {
                        operation.Parameters.Add(parameter);
                    }
                }

                operation.Responses = context.CreateResponses(function);
            }
            else
            {
                IEdmAction action = (IEdmAction)edmOperation;
                operation.RequestBody = context.CreateRequestBody(action);

                // TODO: For bound actions on entities that use optimistic concurrency control,
                // i.e. require ETags for modification operations, the parameters array contains
                // a Parameter Object for the If-Match header.

                operation.Responses = context.CreateResponses(action);
            }

            // TODO: Depending on the result type of the bound action or function the parameters
            // array contains specific Parameter Objects for the allowed system query options.

            // The responses object contains a name/value pair for the success case (HTTP response code 200)
            // describing the structure of the success response by referencing an appropriate schema
            // in the global schemas. In addition, it contains a default name/value pair for
            // the OData error response referencing the global responses.
            //operation.Responses = new OpenApiResponses
            //{
            //    { "204", "204".GetResponse() },
            //    { "default", "default".GetResponse() }
            //};

            return operation;
        }

        /// <summary>
        /// Create a <see cref="OpenApiOperation"/> for a <see cref="IEdmOperationImport"/>.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="operationImport">The Edm operation import.</param>
        /// <returns>The created <see cref="OpenApiOperation"/>.</returns>
        public static OpenApiOperation CreateOperation(this ODataContext context,
            IEdmOperationImport operationImport)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            if (operationImport == null)
            {
                throw Error.ArgumentNull(nameof(operationImport));
            }

            OpenApiOperation operation = new OpenApiOperation();
            operation.Summary = "Invoke " + (operationImport.IsActionImport() ? "action " : "function ") + operationImport.Name;

            // If the action or function import specifies the EntitySet attribute,
            // the tags array of the Operation Object includes the entity set name.
            operation.Tags = CreateTags(operationImport);

            if (operationImport.IsActionImport())
            {
                IEdmActionImport actionImport = (IEdmActionImport)operationImport;

                // The requestBody field contains a Request Body Object describing the structure of the request body.
                // Its schema value follows the rules for Schema Objects for complex types, with one property per action parameter.
                operation.RequestBody = context.CreateRequestBody(actionImport);

            }
            else
            {
                IEdmFunctionImport functionImport = (IEdmFunctionImport)operationImport;

                //The parameters array contains a Parameter Object for each parameter of the function overload,
                // and it contains specific Parameter Objects for the allowed system query options.
                operation.Parameters = context.CreateParameters(functionImport);
            }

            // The responses object contains a name/value pair for the success case (HTTP response code 200)
            // describing the structure of the success response by referencing an appropriate schema
            // in the global schemas. In addition, it contains a default name/value pair for
            // the OData error response referencing the global responses.
            operation.Responses = context.CreateResponses(operationImport);

            return operation;
        }

        private static IList<OpenApiTag> CreateTags(IEdmOperationImport operationImport)
        {
            if (operationImport.EntitySet != null)
            {
                var pathExpression = operationImport.EntitySet as IEdmPathExpression;
                if (pathExpression != null)
                {
                    return new List<OpenApiTag>
                    {
                        new OpenApiTag
                        {
                            Name = PathAsString(pathExpression.PathSegments)
                        }
                    };
                }
            }

            return null;
        }

        internal static string PathAsString(IEnumerable<string> path)
        {
            return String.Join("/", path);
        }
        #endregion
    }
}
