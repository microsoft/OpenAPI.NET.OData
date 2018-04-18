// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Exceptions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Properties;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Annotations;

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
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(entitySet, nameof(entitySet));

            HttpRequestsAnnotation httpRequests = new HttpRequestsAnnotation(context.Model, entitySet);
            HttpRequest request = httpRequests.GetRequest("Get");

            OpenApiOperation operation = new OpenApiOperation
            {
                Summary = "Get entities from " + entitySet.Name,

                // The tags array of the Operation Object includes the entity set name.
                Tags = new List<OpenApiTag>
                {
                    new OpenApiTag
                    {
                        Name = entitySet.Name + "." + entitySet.EntityType().Name
                    }
                }
            };

            if (context.Settings.OperationId)
            {
                //operation.OperationId = "GetEntitiesFrom" + Utils.UpperFirstChar(entitySet.Name);
                operation.OperationId = entitySet.Name + "." + entitySet.EntityType().Name;
            }

            operation.Tags[0].Extensions.Add("x-ms-docs-toc-type", new OpenApiString("page"));
            context.AppendTag(operation.Tags[0]);
            // The parameters array contains Parameter Objects for all system query options allowed for this collection,
            // and it does not list system query options not allowed for this collection, see terms
            // Capabilities.TopSupported, Capabilities.SkipSupported, Capabilities.SearchRestrictions,
            // Capabilities.FilterRestrictions, and Capabilities.CountRestrictions
            operation.Parameters = new List<OpenApiParameter>();
            // $top
            OpenApiParameter parameter = context.CreateTop(entitySet);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }

            // $skip
            parameter = context.CreateSkip(entitySet);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }

            // $search
            parameter = context.CreateSearch(entitySet);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }

            // $filter
            parameter = context.CreateFilter(entitySet);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }

            // $count
            parameter = context.CreateCount(entitySet);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }

            // the syntax of the system query options $expand, $select, and $orderby is too flexible
            // to be formally described with OpenAPI Specification means, yet the typical use cases
            // of just providing a comma-separated list of properties can be expressed via an array-valued
            // parameter with an enum constraint
            // $order
            parameter = context.CreateOrderBy(entitySet);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }

            // $select
            parameter = context.CreateSelect(entitySet);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }

            // $expand
            parameter = context.CreateExpand(entitySet);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }

            AppendCustomParameters(operation, request);

            // The value of responses is a Responses Object.
            // It contains a name/value pair for the success case (HTTP response code 200)
            // describing the structure of a successful response referencing the schema of the entity set’s entity type in the global schemas
            operation.Responses = new OpenApiResponses
            {
                {
                    Constants.StatusCode200,
                    new OpenApiResponse
                    {
                        Description = "Retrieved entities",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            {
                                Constants.ApplicationJsonMediaType,
                                new OpenApiMediaType
                                {
                                    Schema = new OpenApiSchema
                                    {
                                        Title = "Collection of " + entitySet.EntityType().Name,
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

            operation.Responses.Add(Constants.StatusCodeDefault, Constants.StatusCodeDefault.GetResponse());

            // AppendResponses(operation, request);
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
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(entitySet, nameof(entitySet));

            OpenApiOperation operation = new OpenApiOperation
            {
                Summary = "Add new entity to " + entitySet.Name,

                // The tags array of the Operation Object includes the entity set name.
                Tags = new List<OpenApiTag>
                {
                    new OpenApiTag
                    {
                        Name = entitySet.Name + "." + entitySet.EntityType().Name
                    }
                },

                // The requestBody field contains a Request Body Object for the request body
                // that references the schema of the entity set’s entity type in the global schemas.
                RequestBody = new OpenApiRequestBody
                {
                    Required = true,
                    Description = "New entity",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        {
                            Constants.ApplicationJsonMediaType, new OpenApiMediaType
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
            operation.Tags[0].Extensions.Add("x-ms-docs-toc-type", new OpenApiString("page"));
            context.AppendTag(operation.Tags[0]);

            operation.Responses = new OpenApiResponses
            {
                {
                    Constants.StatusCode201,
                    new OpenApiResponse
                    {
                        Description = "Created entity",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            {
                                Constants.ApplicationJsonMediaType,
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
            operation.Responses.Add(Constants.StatusCodeDefault, Constants.StatusCodeDefault.GetResponse());

            if (context.Settings.OperationId)
            {
                // operation.OperationId = "AddEntityTo" + Utils.UpperFirstChar(entitySet.Name);
                operation.OperationId = entitySet.Name +"." + entitySet.EntityType().Name;
            }

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
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(entitySet, nameof(entitySet));

            IEdmEntityType entityType = entitySet.EntityType();

            HttpRequestsAnnotation httpRequests = new HttpRequestsAnnotation(context.Model, entitySet);
            HttpRequest request = httpRequests.GetRequest("Get");

            OpenApiOperation operation = new OpenApiOperation
            {
                Summary = "Get entity from " + entitySet.Name + " by key",
                Tags = new List<OpenApiTag>
                {
                    new OpenApiTag
                    {
                        Name = entitySet.Name + "." + entitySet.EntityType().Name
                    }
                },
                RequestBody = null
            };
            operation.Tags[0].Extensions.Add("x-ms-docs-toc-type", new OpenApiString("page"));
            context.AppendTag(operation.Tags[0]);

            // override the summary using the request.Description.
            if (request != null && request.Description != null)
            {
                operation.Summary = request.Description;
            }

            if (context.Settings.OperationId)
            {
                //operation.OperationId = "GetEntityFrom" + Utils.UpperFirstChar(entitySet.Name) + "ByKey";
                operation.OperationId = entitySet.Name + "." + entitySet.EntityType().Name;
            }

            operation.Parameters = context.CreateKeyParameters(entitySet.EntityType());

            // $select
            OpenApiParameter parameter = context.CreateSelect(entitySet);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }

            // $expand
            parameter = context.CreateExpand(entitySet);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }

            AppendCustomParameters(operation, request);

            operation.Responses = new OpenApiResponses
            {
                {
                    Constants.StatusCode200,
                    new OpenApiResponse
                    {
                        Description = "Retrieved entity",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            {
                                Constants.ApplicationJsonMediaType,
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
                        },
                        Links = context.CreateLinks(entitySet)
                    }
                }
            };
            operation.Responses.Add(Constants.StatusCodeDefault, Constants.StatusCodeDefault.GetResponse());
            // AppendResponses(operation, request);

            if (request != null)
            {
                operation.Security = context.CreateSecurityRequirements(request.SecuritySchemes).ToList();
            }

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
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(entitySet, nameof(entitySet));

            OpenApiOperation operation = new OpenApiOperation
            {
                Summary = "Update entity in " + entitySet.Name,

                // The tags array of the Operation Object includes the entity set name.
                Tags = new List<OpenApiTag>
                {
                    new OpenApiTag
                    {
                        Name= entitySet.Name + "." + entitySet.EntityType().Name
                    }
                }
            };
            operation.Tags[0].Extensions.Add("x-ms-docs-toc-type", new OpenApiString("page"));
            context.AppendTag(operation.Tags[0]);

            if (context.Settings.OperationId)
            {
                // operation.OperationId = "UpdateEntityIn" + Utils.UpperFirstChar(entitySet.Name);
                operation.OperationId = entitySet.Name + "." + entitySet.EntityType().Name;
            }

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
                        Constants.ApplicationJsonMediaType, new OpenApiMediaType
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
                { Constants.StatusCode204, Constants.StatusCode204.GetResponse() },
                { Constants.StatusCodeDefault, Constants.StatusCodeDefault.GetResponse() }
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
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(entitySet, nameof(entitySet));

            OpenApiOperation operation = new OpenApiOperation
            {
                Summary = "Delete entity from " + entitySet.Name,

                // The tags array of the Operation Object includes the entity set name.
                Tags = new List<OpenApiTag>
                {
                    new OpenApiTag
                    {
                        Name = entitySet.Name  + "." + entitySet.EntityType().Name
                    }
                },
                RequestBody = null
            };
            operation.Tags[0].Extensions.Add("x-ms-docs-toc-type", new OpenApiString("page"));
            context.AppendTag(operation.Tags[0]);

            if (context.Settings.OperationId)
            {
                // operation.OperationId = "DeleteEntityFrom" + Utils.UpperFirstChar(entitySet.Name);
                operation.OperationId = entitySet.Name + "." + entitySet.EntityType().Name;
            }

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
                { Constants.StatusCode204, Constants.StatusCode204.GetResponse() },
                { Constants.StatusCodeDefault, Constants.StatusCodeDefault.GetResponse() }
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
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(singleton, nameof(singleton));

            OpenApiOperation operation = new OpenApiOperation
            {
                Summary = "Get " + singleton.Name,

                // The tags array of the Operation Object includes the singleton’s name.
                Tags = new List<OpenApiTag>
                {
                    new OpenApiTag
                    {
                        Name = singleton.Name  + "." + singleton.EntityType().Name
                    }
                },
                RequestBody = null
            };

            if (context.Settings.OperationId)
            {
                // operation.OperationId = "Get" + Utils.UpperFirstChar(singleton.Name);
                operation.OperationId = singleton.Name + "." + singleton.EntityType().Name;
            }
            operation.Tags[0].Extensions.Add("x-ms-docs-toc-type", new OpenApiString("page"));
            context.AppendTag(operation.Tags[0]);

            operation.Parameters = new List<OpenApiParameter>();
            IEdmEntityType entityType = singleton.EntityType();

            // $select
            OpenApiParameter parameter = context.CreateSelect(singleton);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }

            // $expand
            parameter = context.CreateExpand(singleton);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }

            operation.Responses = new OpenApiResponses
            {
                {
                    Constants.StatusCode200,
                    new OpenApiResponse
                    {
                        Description = "Retrieved entity",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            {
                                Constants.ApplicationJsonMediaType,
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
            operation.Responses.Add(Constants.StatusCodeDefault, Constants.StatusCodeDefault.GetResponse());

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
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(singleton, nameof(singleton));

            OpenApiOperation operation = new OpenApiOperation
            {
                Summary = "Update " + singleton.Name,

                // The tags array of the Operation Object includes the singleton’s name.
                Tags = new List<OpenApiTag>
                {
                    new OpenApiTag
                    {
                        Name = singleton.Name + "." + singleton.EntityType().Name
                    }
                }
            };
            operation.Tags[0].Extensions.Add("x-ms-docs-toc-type", new OpenApiString("page"));
            context.AppendTag(operation.Tags[0]);

            if (context.Settings.OperationId)
            {
                //operation.OperationId = "Update" + Utils.UpperFirstChar(singleton.Name);
                operation.OperationId = singleton.Name + "." + singleton.EntityType().Name;
            }

            operation.RequestBody = new OpenApiRequestBody
            {
                Required = true,
                Description = "New property values",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    {
                        Constants.ApplicationJsonMediaType, new OpenApiMediaType
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
                { Constants.StatusCode204, Constants.StatusCode204.GetResponse() },
                { Constants.StatusCodeDefault, Constants.StatusCodeDefault.GetResponse() }
            };

            return operation;
        }
        #endregion

        #region NavigationOperations

        /// <summary>
        /// Retrieve a navigation property from a navigation source.
        /// The Path Item Object for the entity contains the keyword get with an Operation Object as value
        /// that describes the capabilities for retrieving a navigation property form a navigation source.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="navigationSource">The Edm navigation source.</param>
        /// <param name="property">The Edm navigation property.</param>
        /// <returns>The created <see cref="OpenApiOperation"/>.</returns>
        public static OpenApiOperation CreateNavigationGetOperation(this ODataContext context, IEdmNavigationSource navigationSource,
            IEdmNavigationProperty property)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(navigationSource, nameof(navigationSource));
            Utils.CheckArgumentNull(property, nameof(property));

            IEdmEntityType declaringEntityType = property.ToEntityType();

            OpenApiOperation operation = new OpenApiOperation
            {
                Summary = "Get " + property.Name + " from " + navigationSource.Name,
                Tags = new List<OpenApiTag>
                {
                    new OpenApiTag
                    {
                        Name = navigationSource.Name + "." + declaringEntityType.Name
                    }
                },
                RequestBody = null
            };
            operation.Tags[0].Extensions.Add("x-ms-docs-toc-type", new OpenApiString("page"));
            context.AppendTag(operation.Tags[0]);

            if (context.Settings.OperationId)
            {
                // operation.OperationId = "Get" + Utils.UpperFirstChar(property.Name) + "From" + Utils.UpperFirstChar(navigationSource.Name);

                if (context.Model.IsNavigationTypeOverload(navigationSource.EntityType(), property))
                {
                    string key = navigationSource.Name + "." + declaringEntityType.Name;
                    operation.OperationId = navigationSource.Name + "." + context.GetIndex(key) + "-" + declaringEntityType.Name;
                }
                else
                {
                    operation.OperationId = navigationSource.Name + "." + declaringEntityType.Name;
                }
            }

            IEdmEntitySet entitySet = navigationSource as IEdmEntitySet;
            if (entitySet != null)
            {
                operation.Parameters = context.CreateKeyParameters(declaringEntityType);
            }

            IEdmEntityType navEntityType = property.ToEntityType();

            if (operation.Parameters == null)
            {
                operation.Parameters = new List<OpenApiParameter>();
            }

            if (property.TargetMultiplicity() == EdmMultiplicity.Many)
            {
                // The parameters array contains Parameter Objects for system query options allowed for this entity set,
                // and it does not list system query options not allowed for this entity set.
                OpenApiParameter parameter = context.CreateTop(property);
                if (parameter != null)
                {
                    operation.Parameters.Add(parameter);
                }

                parameter = context.CreateSkip(property);
                if (parameter != null)
                {
                    operation.Parameters.Add(parameter);
                }

                parameter = context.CreateSearch(property);
                if (parameter != null)
                {
                    operation.Parameters.Add(parameter);
                }

                parameter = context.CreateFilter(property);
                if (parameter != null)
                {
                    operation.Parameters.Add(parameter);
                }

                parameter = context.CreateCount(property);
                if (parameter != null)
                {
                    operation.Parameters.Add(parameter);
                }

                parameter = context.CreateOrderBy(property);
                if (parameter != null)
                {
                    operation.Parameters.Add(parameter);
                }

                parameter = context.CreateSelect(property);
                if (parameter != null)
                {
                    operation.Parameters.Add(parameter);
                }

                parameter = context.CreateExpand(property);
                if (parameter != null)
                {
                    operation.Parameters.Add(parameter);
                }
            }
            else
            {
                OpenApiParameter parameter = context.CreateSelect(property);
                if (parameter != null)
                {
                    operation.Parameters.Add(parameter);
                }

                parameter = context.CreateExpand(property);
                if (parameter != null)
                {
                    operation.Parameters.Add(parameter);
                }
            }

            operation.Responses = new OpenApiResponses
            {
                {
                    Constants.StatusCode200,
                    new OpenApiResponse
                    {
                        Description = "Retrieved navigation property",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            {
                                Constants.ApplicationJsonMediaType,
                                new OpenApiMediaType
                                {
                                    Schema = new OpenApiSchema
                                    {
                                        Reference = new OpenApiReference
                                        {
                                            Type = ReferenceType.Schema,
                                            Id = navEntityType.FullName()
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
            operation.Responses.Add(Constants.StatusCodeDefault, Constants.StatusCodeDefault.GetResponse());

            return operation;
        }

        /// <summary>
        /// Update a navigation property for a navigation source.
        /// The Path Item Object for the entity set contains the keyword patch with an Operation Object as value
        /// that describes the capabilities for updating the navigation property for a navigation source.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="navigationSource">The Edm navigation source.</param>
        /// <param name="property">The Edm navigation property.</param>
        /// <returns>The created <see cref="OpenApiOperation"/>.</returns>
        public static OpenApiOperation CreateNavigationPatchOperation(this ODataContext context, IEdmNavigationSource navigationSource,
            IEdmNavigationProperty property)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(navigationSource, nameof(navigationSource));
            Utils.CheckArgumentNull(property, nameof(property));

            if (property.TargetMultiplicity() == EdmMultiplicity.Many)
            {
                throw new OpenApiException(String.Format(SRResource.UpdateCollectionNavigationPropertyInvalid, property.Name));
            }

            IEdmEntityType declaringEntityType = property.ToEntityType();

            OpenApiOperation operation = new OpenApiOperation
            {
                Summary = "Update the navigation property " + property.Name + " in " + navigationSource.Name,

                // The tags array of the Operation Object includes the entity set name.
                Tags = new List<OpenApiTag>
                {
                    new OpenApiTag
                    {
                        Name= navigationSource.Name + "." + declaringEntityType.Name
                    }
                }
            };
            operation.Tags[0].Extensions.Add("x-ms-docs-toc-type", new OpenApiString("page"));
            context.AppendTag(operation.Tags[0]);

            if (context.Settings.OperationId)
            {
                //operation.OperationId = "Update" + Utils.UpperFirstChar(property.Name) + "In" + Utils.UpperFirstChar(navigationSource.Name);
                if (context.Model.IsNavigationTypeOverload(navigationSource.EntityType(), property))
                {
                    string key = navigationSource.Name + "." + declaringEntityType.Name;
                    operation.OperationId = navigationSource.Name + "." + context.GetIndex(key) + "-" + declaringEntityType.Name;
                }
                else
                {
                    operation.OperationId = navigationSource.Name + "." + declaringEntityType.Name;
                }
            }

            operation.Parameters = context.CreateKeyParameters(navigationSource.EntityType());

            // TODO: If the entity set uses optimistic concurrency control,
            // i.e. requires ETags for modification operations, the parameters array contains
            // a Parameter Object for the If-Match header.

            operation.RequestBody = new OpenApiRequestBody
            {
                Required = true,
                Description = "New navigation property values",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    {
                        Constants.ApplicationJsonMediaType, new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.Schema,
                                    Id = navigationSource.EntityType().FullName()
                                }
                            }
                        }
                    }
                }
            };

            operation.Responses = new OpenApiResponses
            {
                { Constants.StatusCode204, Constants.StatusCode204.GetResponse() },
                { Constants.StatusCodeDefault, Constants.StatusCodeDefault.GetResponse() }
            };

            return operation;
        }

        /// <summary>
        /// Create a navigation for a navigation source.
        /// The Path Item Object for the entity set contains the keyword delete with an Operation Object as value
        /// that describes the capabilities for create a navigation for a navigation source.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="navigationSource">The Edm navigation source.</param>
        /// <param name="property">The Edm navigation property.</param>
        /// <returns>The created <see cref="OpenApiOperation"/>.</returns>
        public static OpenApiOperation CreateNavigationPostOperation(this ODataContext context, IEdmNavigationSource navigationSource,
            IEdmNavigationProperty property)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(navigationSource, nameof(navigationSource));
            Utils.CheckArgumentNull(property, nameof(property));

            if (property.TargetMultiplicity() != EdmMultiplicity.Many)
            {
                throw new OpenApiException(String.Format(SRResource.PostToNonCollectionNavigationPropertyInvalid, property.Name));
            }

            IEdmEntityType declaringEntityType = property.ToEntityType();

            OpenApiOperation operation = new OpenApiOperation
            {
                Summary = "Add new navigation property to " + property.Name + " for " + navigationSource.Name,

                // The tags array of the Operation Object includes the entity set name.
                Tags = new List<OpenApiTag>
                {
                    new OpenApiTag
                    {
                        Name = navigationSource.Name + "." + declaringEntityType.Name
                    }
                },

                // The requestBody field contains a Request Body Object for the request body
                // that references the schema of the entity set’s entity type in the global schemas.
                RequestBody = new OpenApiRequestBody
                {
                    Required = true,
                    Description = "New navigation property",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        {
                            Constants.ApplicationJsonMediaType, new OpenApiMediaType
                            {
                                Schema = new OpenApiSchema
                                {
                                    Reference = new OpenApiReference
                                    {
                                        Type = ReferenceType.Schema,
                                        Id = navigationSource.EntityType().FullName()
                                    }
                                }
                            }
                        }
                    }
                }
            };
            operation.Tags[0].Extensions.Add("x-ms-docs-toc-type", new OpenApiString("page"));
            context.AppendTag(operation.Tags[0]);

            operation.Responses = new OpenApiResponses
            {
                {
                    Constants.StatusCode201,
                    new OpenApiResponse
                    {
                        Description = "Created navigation property.",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            {
                                Constants.ApplicationJsonMediaType,
                                new OpenApiMediaType
                                {
                                    Schema = new OpenApiSchema
                                    {
                                        Reference = new OpenApiReference
                                        {
                                            Type = ReferenceType.Schema,
                                            Id = navigationSource.EntityType().FullName()
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
            operation.Responses.Add(Constants.StatusCodeDefault, Constants.StatusCodeDefault.GetResponse());

            if (context.Settings.OperationId)
            {
                //operation.OperationId = "Add" + Utils.UpperFirstChar(property.Name) + "To" + Utils.UpperFirstChar(navigationSource.Name);
                if (context.Model.IsNavigationTypeOverload(navigationSource.EntityType(), property))
                {
                    string key = navigationSource.Name + "." + declaringEntityType.Name;
                    operation.OperationId = navigationSource.Name + "." + context.GetIndex(key) + "-" + declaringEntityType.Name;
                }
                else
                {
                    operation.OperationId = navigationSource.Name + "." + declaringEntityType.Name;
                }
            }

            return operation;
        }
        #endregion

        #region EdmOperation Operations
        public static OpenApiOperation CreateOperation(this ODataContext context, IEdmNavigationSource navigationSource, 
            IEdmOperation edmOperation)
        {
            return context.CreateOperation(navigationSource, navigationSource.EntityType(), edmOperation);
        }

        /// <summary>
        /// Create a <see cref="OpenApiOperation"/> for a <see cref="IEdmOperation"/>.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="navigationSource">The binding navigation source.</param>
        /// <param name="edmOperation">The Edm operation.</param>
        /// <returns>The created <see cref="OpenApiOperation"/>.</returns>
        public static OpenApiOperation CreateOperation(this ODataContext context, IEdmNavigationSource navigationSource, IEdmEntityType entityType,
            IEdmOperation edmOperation)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(navigationSource, nameof(navigationSource));
            Utils.CheckArgumentNull(edmOperation, nameof(edmOperation));

            OpenApiOperation operation = new OpenApiOperation();
            operation.Summary = "Invoke " + (edmOperation.IsAction() ? "action " : "function ") + edmOperation.Name;

            if (context.Settings.OperationId)
            {
                operation.OperationId = navigationSource.Name;

                // Append the type cast
                if (!entityType.IsEquivalentTo(navigationSource.EntityType()))
                {
                    operation.OperationId += "." + entityType.Name;
                }

                if (!context.Model.IsOperationOverload(edmOperation))
                {
                    operation.OperationId += "." + Utils.UpperFirstChar(edmOperation.Name);
                }
                else
                {
                    string key = operation.OperationId + "." + edmOperation.Name; // get the operationId so far.
                    operation.OperationId += "." + context.GetIndex(key) + "-" + Utils.UpperFirstChar(edmOperation.Name);
                    // operation.OperationId += ".Invoke" + Utils.UpperFirstChar(edmOperation.Name) + GetParameters(edmOperation);
                }
            }

            // The tags array of the Operation Object includes the entity set name.
            operation.Tags = new List<OpenApiTag>
            {
                new OpenApiTag { Name = navigationSource.Name + "." + edmOperation.Name }
            };

            // operation.Tags[0].Extensions.Add("x-ms-doc-type", new OpenApiString((edmOperation.IsAction() ? "action" : "function")));
            //operation.Extensions.Add("x-ms-doc-type", new OpenApiString((edmOperation.IsAction() ? "action" : "function")));
           // context.AppendTagItem(new TagItem(operation.Tags[0].Name, null, edmOperation.IsAction() ? "action" : "function"));
            operation.Tags[0].Extensions.Add("x-ms-docs-toc-type", new OpenApiString("container"));
            context.AppendTag(operation.Tags[0]);

            // For actions and functions bound to a single entity within an entity
            // set the parameters array contains a Parameter Object for each key property,
            // using the same type mapping as described for primitive properties.
            IEdmSingleton singleton = navigationSource as IEdmSingleton;
            if (singleton == null && edmOperation.IsBound)
            {
                IEdmOperationParameter bindingParameter = edmOperation.Parameters.FirstOrDefault();
                if (bindingParameter != null &&
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
            }
            else
            {
                IEdmAction action = (IEdmAction)edmOperation;
                operation.RequestBody = context.CreateRequestBody(action);

                // TODO: For bound actions on entities that use optimistic concurrency control,
                // i.e. require ETags for modification operations, the parameters array contains
                // a Parameter Object for the If-Match header.
            }

            // TODO: Depending on the result type of the bound action or function the parameters
            // array contains specific Parameter Objects for the allowed system query options.

            // The responses object contains a name/value pair for the success case (HTTP response code 200)
            // describing the structure of the success response by referencing an appropriate schema
            // in the global schemas. In addition, it contains a default name/value pair for
            // the OData error response referencing the global responses.
            operation.Responses = context.CreateResponses(edmOperation);

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
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(operationImport, nameof(operationImport));

            OpenApiOperation operation = new OpenApiOperation();
            operation.Summary = "Invoke " + (operationImport.IsActionImport() ? "action " : "function ") + operationImport.Name;

            if (context.Settings.OperationId)
            {
                // operation.OperationId = "Invoke" + Utils.UpperFirstChar(operationImport.Name) + GetParameters(operationImport.Operation);

                if (!context.Model.IsOperationOverload(operationImport))
                {
                    operation.OperationId += "." + Utils.UpperFirstChar(operationImport.Name);
                }
                else
                {
                    string key = "." + operationImport.Name;
                    operation.OperationId += "." + context.GetIndex(key) + "-" + Utils.UpperFirstChar(operationImport.Name);
                }
            }

            // If the action or function import specifies the EntitySet attribute,
            // the tags array of the Operation Object includes the entity set name.
            operation.Tags = CreateTags(operationImport);
            //operation.Tags[0].Extensions.Add("x-ms-doc-type", new OpenApiString((operationImport.IsActionImport() ? "action" : "function")));
            //operation.Extensions.Add("x-ms-doc-type", new OpenApiString((operationImport.IsActionImport() ? "action" : "function")));
            operation.Tags[0].Extensions.Add("x-ms-docs-toc-type", new OpenApiString("container"));
            context.AppendTag(operation.Tags[0]);

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

        private static void AppendResponses(OpenApiOperation operation, HttpRequest request)
        {
            if (request == null || request.HttpResponses == null || operation == null || operation.Responses == null)
            {
                return;
            }

            if (request.CustomQueryOptions != null)
            {
                AppendCustomParameters(operation.Parameters, request.CustomQueryOptions, ParameterLocation.Query);
            }

            foreach (var response in request.HttpResponses)
            {
                operation.Responses.TryGetValue(response.ResponseCode, out OpenApiResponse operationResponse);
                if (operationResponse == null)
                {
                    continue;
                }

                if (response.Examples != null)
                {
                    if (operationResponse.Content == null)
                    {
                        operationResponse.Content = new Dictionary<string, OpenApiMediaType>();
                    }

                    foreach (var example in response.Examples)
                    {
                        OpenApiMediaType mediaType = new OpenApiMediaType();
                        if (example is ExternalExample)
                        {
                            var externalExample = (ExternalExample)example;
                            mediaType.Example = new OpenApiString(externalExample.ExternalValue);
                        }
                        else
                        {
                            var inlineExample = (InlineExample)example;
                            mediaType.Example = new OpenApiString(inlineExample.InlineValue);
                        }

                        operationResponse.Content[example.Description] = mediaType;
                    }
                }
            }
        }

        private static void AppendCustomParameters(OpenApiOperation operation, HttpRequest request)
        {
            if (request == null)
            {
                return;
            }

            if (operation.Parameters == null)
            {
                operation.Parameters = new List<OpenApiParameter>();
            }

            if (request.CustomQueryOptions != null)
            {
                AppendCustomParameters(operation.Parameters, request.CustomQueryOptions, ParameterLocation.Query);
            }

            if (request.CustomQueryOptions != null)
            {
                AppendCustomParameters(operation.Parameters, request.CustomHeaders, ParameterLocation.Header);
            }
        }

        private static void AppendCustomParameters(IList<OpenApiParameter> parameters, IList<CustomParameter> headers, ParameterLocation location)
        {
            foreach (var param in headers)
            {
                OpenApiParameter parameter = new OpenApiParameter
                {
                    In = location,
                    Name = param.Name,
                    Description = param.Description,
                    Schema = new OpenApiSchema
                    {
                        // Type = param.Type
                    },
                    Required = param.Required ?? false,
                    Example = new OpenApiString(param.DocumentationURL)
                };

                if (param.ExampleValues != null)
                {
                    parameter.Examples = new List<OpenApiExample>();
                    foreach (var example in param.ExampleValues)
                    {
                        OpenApiExample ex = new OpenApiExample
                        {
                            Description = example.Description
                        };

                        if (example is ExternalExample)
                        {
                            var externalExample = (ExternalExample)example;
                            ex.Value = new OpenApiString(externalExample.ExternalValue);
                        }
                        else
                        {
                            var inlineExample = (InlineExample)example;
                            ex.Value = new OpenApiString(inlineExample.InlineValue);
                        }

                        parameter.Examples.Add(ex);
                    }
                }

                parameters.Add(parameter);
            }
        }
        #endregion

        private static string GetParameters(IEdmOperation operation)
        {
            int skip = operation.IsBound ? 1 : 0;
            return operation.Parameters.Skip(skip).Any() ? "By" + String.Join("And", operation.Parameters.Skip(skip).Select(e => e.Name)) :
                string.Empty;
        }
    }
}
