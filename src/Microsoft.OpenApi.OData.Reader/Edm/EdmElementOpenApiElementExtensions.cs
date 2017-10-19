//---------------------------------------------------------------------
// <copyright file="EdmElementOpenApiElementExtensions.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.OData.Edm;

namespace Microsoft.OData.OpenAPI
{
    /// <summary>
    /// Extension methods for Edm Elements to Open Api Elements.
    /// </summary>
    internal static class EdmElementOpenApiElementExtensions
    {
        private static IDictionary<string, OpenApiResponse> Responses =
           new Dictionary<string, OpenApiResponse>
           {
                { "default",
                    new OpenApiResponse
                    {
                        Reference = new OpenApiReference("#/components/responses/error")
                    }
                },
                { "204", new OpenApiResponse { Description = "Success"} },
           };

        public static KeyValuePair<string, OpenApiResponse> GetResponse(this string statusCode)
        {
            return new KeyValuePair<string, OpenApiResponse>(statusCode, Responses[statusCode]);
        }

        public static OpenApiSchema CreateSchema(this IEdmTypeReference reference)
        {
            if (reference == null)
            {
                return null;
            }

            switch (reference.TypeKind())
            {
                case EdmTypeKind.Collection:
                    return new OpenApiSchema
                    {
                        Type = "array",
                        Items = CreateSchema(reference.AsCollection().ElementType())
                    };

                case EdmTypeKind.Complex:
                case EdmTypeKind.Entity:
                case EdmTypeKind.EntityReference:
                case EdmTypeKind.Enum:
                    return new OpenApiSchema
                    {
                        Reference = new OpenApiReference("#/components/schemas/" + reference.Definition.FullTypeName())
                    };

                case EdmTypeKind.Primitive:
                    OpenApiSchema schema;
                    if (reference.IsInt64())
                    {
                        schema = new OpenApiSchema
                        {
                            OneOf = new List<OpenApiSchema>
                            {
                                new OpenApiSchema { Type = "integer" },
                                new OpenApiSchema { Type = "string" }
                            },
                            Format = "int64",
                            Nullable = reference.IsNullable ? (bool?)true : null
                        };
                    }
                    else if (reference.IsDouble())
                    {
                        schema = new OpenApiSchema
                        {
                            OneOf = new List<OpenApiSchema>
                            {
                                new OpenApiSchema { Type = "number" },
                                new OpenApiSchema { Type = "string" }
                            },
                            Format = "double",
                        };
                    }
                    else
                    {
                        schema = new OpenApiSchema
                        {
                            Type = reference.AsPrimitive().GetOpenApiDataType().GetCommonName()
                        };
                    }
                    schema.Nullable = reference.IsNullable ? (bool?)true : null;
                    break;

                case EdmTypeKind.TypeDefinition:
                case EdmTypeKind.None:
                default:
                    throw Error.NotSupported("Not supported!");
            }

            return null;
        }

        public static OpenApiPathItem CreatePathItem(this IEdmOperationImport operationImport)
        {
            if (operationImport.Operation.IsAction())
            {
                return ((IEdmActionImport)operationImport).CreatePathItem();
            }

            return ((IEdmFunctionImport)operationImport).CreatePathItem();
        }

        public static OpenApiPathItem CreatePathItem(this IEdmOperation operation)
        {
            if (operation.IsAction())
            {
                return ((IEdmAction)operation).CreatePathItem();
            }

            return ((IEdmFunction)operation).CreatePathItem();
        }

        public static OpenApiPathItem CreatePathItem(this IEdmActionImport actionImport)
        {
            return CreatePathItem(actionImport.Action);
        }

        public static OpenApiPathItem CreatePathItem(this IEdmAction action)
        {
            return new OpenApiPathItem
            {
                Post = new OpenApiOperation
                {
                    Summary = "Invoke action " + action.Name,
                    Tags = CreateTags(action),
                    Parameters = CreateParameters(action),
                    Responses = CreateResponses(action)
                }
            };
        }

        public static OpenApiPathItem CreatePathItem(this IEdmFunctionImport functionImport)
        {
            return CreatePathItem(functionImport.Function);
        }

        public static OpenApiPathItem CreatePathItem(this IEdmFunction function)
        {
            return new OpenApiPathItem
            {
                Get = new OpenApiOperation
                {
                    Summary = "Invoke function " + function.Name,
                    Tags = CreateTags(function),
                    Parameters = CreateParameters(function),
                    Responses = CreateResponses(function)
                }
            };
        }

        public static string CreatePathItemName(this IEdmActionImport actionImport)
        {
            return CreatePathItemName(actionImport.Action);
        }

        public static string CreatePathItemName(this IEdmAction action)
        {
            return "/" + action.Name;
        }

        public static string CreatePathItemName(this IEdmFunctionImport functionImport)
        {
            return CreatePathItemName(functionImport.Function);
        }

        public static string CreatePathItemName(this IEdmFunction function)
        {
            StringBuilder functionName = new StringBuilder("/" + function.Name + "(");

            functionName.Append(String.Join(",",
                function.Parameters.Select(p => p.Name + "=" + "{" + p.Name + "}")));
            functionName.Append(")");

            return functionName.ToString();
        }

        public static string CreatePathItemName(this IEdmOperationImport operationImport)
        {
            if (operationImport.Operation.IsAction())
            {
                return ((IEdmActionImport)operationImport).CreatePathItemName();
            }

            return ((IEdmFunctionImport)operationImport).CreatePathItemName();
        }

        public static string CreatePathItemName(this IEdmOperation operation)
        {
            if (operation.IsAction())
            {
                return ((IEdmAction)operation).CreatePathItemName();
            }

            return ((IEdmFunction)operation).CreatePathItemName();
        }

        private static OpenApiResponses CreateResponses(this IEdmAction actionImport)
        {
            return new OpenApiResponses
            {
                "204".GetResponse(),
                "default".GetResponse()
            };
        }

        private static OpenApiResponses CreateResponses(this IEdmFunction function)
        {
            OpenApiResponses responses = new OpenApiResponses();

            OpenApiResponse response = new OpenApiResponse
            {
                Description = "Success",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    {
                        "application/json",
                        new OpenApiMediaType
                        {
                            Schema = function.ReturnType.CreateSchema()
                        }
                    }
                }
            };
            responses.Add("200", response);
            responses.Add("default".GetResponse());
            return responses;
        }

        private static IList<string> CreateTags(this IEdmOperationImport operationImport)
        {
            if (operationImport.EntitySet != null)
            {
                var pathExpression = operationImport.EntitySet as IEdmPathExpression;
                if (pathExpression != null)
                {
                    return new List<string>
                    {
                        PathAsString(pathExpression.PathSegments)
                    };
                }
            }

            return null;
        }

        private static IList<string> CreateTags(this IEdmOperation operation)
        {
            if (operation.EntitySetPath != null)
            {
                var pathExpression = operation.EntitySetPath as IEdmPathExpression;
                if (pathExpression != null)
                {
                    return new List<string>
                    {
                        PathAsString(pathExpression.PathSegments)
                    };
                }
            }

            return null;
        }

        private static IList<OpenApiParameter> CreateParameters(this IEdmOperation operation)
        {
            IList<OpenApiParameter> parameters = new List<OpenApiParameter>();

            foreach (IEdmOperationParameter edmParameter in operation.Parameters)
            {
                parameters.Add(new OpenApiParameter
                {
                    Name = edmParameter.Name,
                    In = ParameterLocation.path,
                    Required = true,
                    Schema = edmParameter.Type.CreateSchema()
                });
            }

            return parameters;
        }

        internal static string PathAsString(IEnumerable<string> path)
        {
            return String.Join("/", path);
        }
    }
}
