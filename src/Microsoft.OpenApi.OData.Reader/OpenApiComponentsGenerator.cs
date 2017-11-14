//---------------------------------------------------------------------
// <copyright file="EdmOpenApiComponentsGenerator.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Schema;

namespace Microsoft.OpenApi.OData
{
    /// <summary>
    /// Visit Edm model to generate <see cref="OpenApiComponents"/>
    /// </summary>
    internal class OpenApiComponentsGenerator
    {
        private IEdmModel _model;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenApiComponentsGenerator" /> class.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="settings">The Open Api writer settings.</param>
        public OpenApiComponentsGenerator(IEdmModel model)
        {
            _model = model ?? throw Error.ArgumentNull(nameof(model));
        }

        /// <summary>
        /// Generate the <see cref="OpenApiComponents"/>
        /// </summary>
        /// <returns>the components object.</returns>
        public OpenApiComponents Generate()
        {
            // The value of components is a Components Object.
            // It holds maps of reusable schemas describing message bodies, operation parameters, and responses.
            return new OpenApiComponents
            {
                // The value of schemas is a map of Schema Objects.
                // Each entity type, complex type, enumeration type, and type definition directly
                // or indirectly used in the paths field is represented as a name/value pair of the schemas map.
                Schemas = VisitSchemas(),

                // The value of parameters is a map of Parameter Objects.
                // It allows defining query options and headers that can be reused across operations of the service.
                Parameters = VisitParameters(),

                // The value of responses is a map of Response Objects.
                // It allows defining responses that can be reused across operations of the service.
                Responses = VisitResponses()
            };
        }

        /// <summary>
        /// Visit the Edm schema and generate the <see cref="OpenApiSchema"/>.
        /// The name of each pair is the namespace-qualified name of the type. It uses the namespace instead of the alias.
        /// The value of each pair is a <see cref="OpenApiSchema"/>.
        /// </summary>
        /// <returns></returns>
        private IDictionary<string, OpenApiSchema> VisitSchemas()
        {
            IDictionary<string, OpenApiSchema> schemas = new Dictionary<string, OpenApiSchema>();

            foreach (var element in _model.SchemaElements)
            {
                switch (element.SchemaElementKind)
                {
                    case EdmSchemaElementKind.TypeDefinition:
                        {
                            IEdmType reference = (IEdmType)element;
                            schemas.Add(reference.FullTypeName(), VisitSchemaType(reference));
                        }
                        break;
                }
            }

            AppendODataErrors(schemas);

            return schemas;
        }

        private OpenApiSchema VisitSchemaType(IEdmType definition)
        {
            switch (definition.TypeKind)
            {
                case EdmTypeKind.Complex:
                case EdmTypeKind.Entity:
                    return VisitStructuredType((IEdmStructuredType)definition, true);

                case EdmTypeKind.Enum: // enum type
                    return VisitEnumType((IEdmEnumType)definition);

                case EdmTypeKind.TypeDefinition:
                    return VisitTypeDefinitions((IEdmTypeDefinition)definition);

                case EdmTypeKind.None:
                default:
                    throw Error.NotSupported(String.Format("Not supported {0} type kind.", definition.TypeKind));
            }
        }

        // 4.6.1.2 Schemas for Enumeration Types
        private OpenApiSchema VisitEnumType(IEdmEnumType enumType)
        {
            OpenApiSchema schema = new OpenApiSchema
            {
                // An enumeration type is represented as a Schema Object of type string
                Type = "string",

                // containing the OpenAPI Specification enum keyword.
                Enum = new List<IOpenApiAny>(),

                // It optionally can contain the field description,
                // whose value is the value of the unqualified annotation Core.Description of the enumeration type.
                Description = _model.GetDescription(enumType)
            };

            // Enum value is an array that contains a string with the member name for each enumeration member.
            foreach (IEdmEnumMember member in enumType.Members)
            {
                schema.Enum.Add(new OpenApiString(member.Name));
            }

            schema.Title = (enumType as IEdmSchemaElement)?.Name;
            return schema;
        }

        // 4.6.1.3 Schemas for Type Definitions
        private OpenApiSchema VisitTypeDefinitions(IEdmTypeDefinition typeDefinition)
        {
            return typeDefinition?.UnderlyingType?.CreateSchema();
        }

        // 4.6.1.1 Schemas for Entity Types and Complex Types
        private OpenApiSchema VisitStructuredType(IEdmStructuredType structuredType, bool processBase)
        {
            if (processBase && structuredType.BaseType != null)
            {
                // A structured type with a base type is represented as a Schema Object
                // that contains the keyword allOf whose value is an array with two items:
                return new OpenApiSchema
                {
                    AllOf = new List<OpenApiSchema>
                    {
                        // 1. a JSON Reference to the Schema Object of the base type
                        new OpenApiSchema
                        {
                            Pointer = new OpenApiReference(ReferenceType.Schema, structuredType.BaseType.FullTypeName())
                        },

                        // 2. a Schema Object describing the derived type
                        VisitStructuredType(structuredType, false)
                    }
                };
            }
            else
            {
                // A structured type without a base type is represented as a Schema Object of type object
                return new OpenApiSchema
                {
                    Title = (structuredType as IEdmSchemaElement).Name,

                    Type = "object",

                    // Each structural property and navigation property is represented
                    // as a name/value pair of the standard OpenAPI properties object.
                    Properties = VisitStructuredTypeProperties(structuredType),

                    // It optionally can contain the field description,
                    // whose value is the value of the unqualified annotation Core.Description of the structured type.
                    // However, ODL doesn't support the Core.Description on structure type.
                };
            }
        }

        // 4.6.1.1 Properties
        private IDictionary<string, OpenApiSchema> VisitStructuredTypeProperties(IEdmStructuredType structuredType)
        {
            // The name is the property name, the value is a Schema Object describing the allowed values of the property.
            IDictionary<string, OpenApiSchema> properties = new Dictionary<string, OpenApiSchema>();

            // structure properties
            foreach (var property in structuredType.DeclaredStructuralProperties())
            {
                OpenApiSchema propertySchema = property.Type.CreateSchema();
                propertySchema.Default = property.DefaultValueString != null ? new OpenApiString(property.DefaultValueString) : null;
                properties.Add(property.Name, propertySchema);
            }

            // navigation properties
            foreach (var property in structuredType.DeclaredNavigationProperties())
            {
                OpenApiSchema propertySchema = property.Type.CreateSchema();
                properties.Add(property.Name, propertySchema);
            }

            return properties;
        }

        private IDictionary<string, OpenApiParameter> VisitParameters()
        {
            return new Dictionary<string, OpenApiParameter>
            {
                { "top", VisitTop() },
                { "skip", VisitSkip() },
                { "count", VisitCount() },
                { "filter", VisitFilter() },
                { "search", VisitSearch() },
            };
        }

        private OpenApiParameter VisitTop()
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

        private OpenApiParameter VisitSkip()
        {
            return new OpenApiParameter
            {
                Name = "$skip",
                In = ParameterLocation.Query,
                Description = "Skip only the first n items",
                Schema = new OpenApiSchema
                {
                    Type = "integer",
                    Minimum = 0,
                }
            };
        }

        private OpenApiParameter VisitCount()
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

        private OpenApiParameter VisitFilter()
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

        private OpenApiParameter VisitSearch()
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

        private void AppendODataErrors(IDictionary<string, OpenApiSchema> schemas)
        {
            schemas.Add("odata.error", new OpenApiSchema
            {
                Type = "object",
                Required = new List<string>
                {
                    "error"
                },
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    {
                        "error",
                        new OpenApiSchema
                        {
                            Pointer = new OpenApiReference("#/components/schemas/odata.error.main")
                        }
                    }
                }
            });

            schemas.Add("odata.error.main", new OpenApiSchema
            {
                Type = "object",
                Required = new List<string>
                {
                    "code", "message"
                },
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    {
                        "code", new OpenApiSchema { Type = "string" }
                    },
                    {
                        "message", new OpenApiSchema { Type = "string" }
                    },
                    {
                        "target", new OpenApiSchema { Type = "string" }
                    },
                    {
                        "details",
                        new OpenApiSchema
                        {
                            Type = "array",
                            Items = new OpenApiSchema
                            {
                                Pointer = new OpenApiReference("#/components/schemas/odata.error.detail")
                            }
                        }
                    },
                    {
                        "innererror",
                        new OpenApiSchema
                        {
                            Type = "object",
                            Description = "The structure of this object is service-specific"
                        }
                    }
                }
            });

            schemas.Add("odata.error.detail", new OpenApiSchema
            {
                Type = "object",
                Required = new List<string>
                {
                    "code", "message"
                },
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    {
                        "code", new OpenApiSchema { Type = "string" }
                    },
                    {
                        "message", new OpenApiSchema { Type = "string" }
                    },
                    {
                        "target", new OpenApiSchema { Type = "string" }
                    }
                }
            });
        }

        /// <summary>
        /// It contains one name/value pair for the standard OData error response
        /// that is referenced from all operations of the service.
        /// </summary>
        /// <returns>Teh name/value pairs for the standard OData error response</returns>
        private IDictionary<string, OpenApiResponse> VisitResponses()
        {
            return new Dictionary<string, OpenApiResponse>
            {
                { "error", VisitError() }
            };
        }

        private OpenApiResponse VisitError()
        {
            return new OpenApiResponse
            {
                Description = "error",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    {
                        "application/json",
                        new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Pointer = new OpenApiReference("#/components/schemas/odata.error")
                            }
                        }
                    }
                }
            };
        }
    }
}
