// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Microsoft.OpenApi.OData.Generator
{
    /// <summary>
    /// See https://github.com/oasis-tcs/odata-openapi/blob/master/examples/odata-definitions.json
    /// </summary>
    internal static class SchemaExtensions
    {
        private static string _externalResource = "https://raw.githubusercontent.com/oasis-tcs/odata-openapi/master/examples/odata-definitions.json";

        public static OpenApiSchema CreateEdmTypeSchema(this IEdmModel model, IEdmType definition)
        {
            switch (definition.TypeKind)
            {
                case EdmTypeKind.Complex: // complex type
                case EdmTypeKind.Entity: // entity type
                    return model.CreateStructuredTypeSchema((IEdmStructuredType)definition, true);

                case EdmTypeKind.Enum: // enum type
                    return model.CreateEnumTypeSchema((IEdmEnumType)definition);

                case EdmTypeKind.TypeDefinition: // type definition
                    return model.CreateTypeDefinitionSchema((IEdmTypeDefinition)definition);

                case EdmTypeKind.None:
                default:
                    throw Error.NotSupported(String.Format("Not supported {0} type kind.", definition.TypeKind));
            }
        }

        public static OpenApiSchema CreateStructuredTypeSchema(this IEdmModel model, IEdmStructuredType structuredType, bool processBase)
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
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.Schema,
                                Id = structuredType.BaseType.FullTypeName()
                            }
                        },

                        // 2. a Schema Object describing the derived type
                        model.CreateStructuredTypeSchema(structuredType, false)
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
                    Properties = model.CreateStrucutredTypePropertiesSchema(structuredType),

                    // It optionally can contain the field description,
                    // whose value is the value of the unqualified annotation Core.Description of the structured type.
                    // However, ODL doesn't support the Core.Description on structure type.
                };
            }
        }

        // 4.6.1.1 Properties
        public static IDictionary<string, OpenApiSchema> CreateStrucutredTypePropertiesSchema(this IEdmModel model, IEdmStructuredType structuredType)
        {
            // The name is the property name, the value is a Schema Object describing the allowed values of the property.
            IDictionary<string, OpenApiSchema> properties = new Dictionary<string, OpenApiSchema>();

            // structure properties
            foreach (var property in structuredType.DeclaredStructuralProperties())
            {
                // OpenApiSchema propertySchema = property.Type.CreateSchema();
                // propertySchema.Default = property.DefaultValueString != null ? new OpenApiString(property.DefaultValueString) : null;
                properties.Add(property.Name, property.CreatePropertySchema());
            }

            // navigation properties
            foreach (var property in structuredType.DeclaredNavigationProperties())
            {
                OpenApiSchema propertySchema = property.Type.CreateSchema();
                properties.Add(property.Name, propertySchema);
            }

            return properties;
        }

        public static OpenApiSchema CreateEnumTypeSchema(this IEdmModel model, IEdmEnumType enumType)
        {
            OpenApiSchema schema = new OpenApiSchema
            {
                // An enumeration type is represented as a Schema Object of type string
                Type = "string",

                // containing the OpenAPI Specification enum keyword.
                Enum = new List<IOpenApiAny>(),

                // It optionally can contain the field description,
                // whose value is the value of the unqualified annotation Core.Description of the enumeration type.
                Description = model.GetDescription(enumType)
            };

            // Enum value is an array that contains a string with the member name for each enumeration member.
            foreach (IEdmEnumMember member in enumType.Members)
            {
                schema.Enum.Add(new OpenApiString(member.Name));
            }

            schema.Title = (enumType as IEdmSchemaElement)?.Name;
            return schema;
        }

        public static OpenApiSchema CreateTypeDefinitionSchema(this IEdmModel model, IEdmTypeDefinition typeDefinition)
        {
            return typeDefinition?.UnderlyingType?.CreateSchema();
        }

        public static OpenApiSchema CreatePropertySchema(this IEdmStructuralProperty property)
        {
            if (property == null)
            {
                throw Error.ArgumentNull(nameof(property));
            }

            OpenApiSchema schema = property.Type.CreateSchema();
            schema.Default = CreateDefault(property);

            return schema;
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
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.Schema,
                            Id = reference.Definition.FullTypeName()
                        }
                    };

                case EdmTypeKind.Primitive:
                    IEdmPrimitiveTypeReference primitiveTypeReference = (IEdmPrimitiveTypeReference)reference;
                    return primitiveTypeReference.CreateSchema();

                case EdmTypeKind.TypeDefinition:
                    return ((IEdmTypeDefinitionReference)reference).TypeDefinition().UnderlyingType.CreateSchema();

                case EdmTypeKind.None:
                default:
                    throw Error.NotSupported(String.Format("Not supported {0} type kind.", reference.TypeKind()));
            }
        }

        public static OpenApiSchema CreateSchema(this IEdmPrimitiveTypeReference primitiveType)
        {
            OpenApiSchema schema = primitiveType.PrimitiveDefinition().CreateSchema();
            if (schema != null)
            {
                switch(primitiveType.PrimitiveKind())
                {
                    case EdmPrimitiveTypeKind.Binary: // binary
                        IEdmBinaryTypeReference binaryTypeReference = (IEdmBinaryTypeReference)primitiveType;
                        schema.MaxLength = binaryTypeReference.MaxLength;
                        break;

                    case EdmPrimitiveTypeKind.Decimal: // decimal
                        IEdmDecimalTypeReference decimalTypeReference = (IEdmDecimalTypeReference)primitiveType;
                        if (decimalTypeReference.Precision != null)
                        {
                            if (decimalTypeReference.Scale != null)
                            {
                                // The precision is represented with the maximum and minimum keywords and a value of ±(10^ (precision - scale) - 10^ scale).
                                double tmp = Math.Pow(10, decimalTypeReference.Precision.Value - decimalTypeReference.Scale.Value)
                                    - Math.Pow(10, -decimalTypeReference.Scale.Value);
                                schema.Minimum = (decimal?)(tmp * -1.0);
                                schema.Maximum = (decimal?)(tmp);
                            }
                            else
                            {
                                // If the scale facet has a numeric value, and ±(10^precision - 1) if the scale is variable
                                double tmp = Math.Pow(10, decimalTypeReference.Precision.Value) - 1;
                                schema.Minimum = (decimal?)(tmp * -1.0);
                                schema.Maximum = (decimal?)(tmp);
                            }
                        }

                        // The scale of properties of type Edm.Decimal are represented with the OpenAPI Specification keyword multipleOf and a value of 10 ^ -scale
                        schema.MultipleOf = decimalTypeReference.Scale == null ? null : (decimal?)(Math.Pow(10, decimalTypeReference.Scale.Value * -1));
                        break;
                    case EdmPrimitiveTypeKind.String: // string
                        IEdmStringTypeReference stringTypeReference = (IEdmStringTypeReference)primitiveType;
                        schema.MaxLength = stringTypeReference.MaxLength;
                        break;
                }
                // Nullable properties are marked with the keyword nullable and a value of true.
                schema.Nullable = primitiveType.IsNullable ? true : false;
            }

            return schema;
        }

        public static OpenApiSchema CreateSchema(this IEdmPrimitiveType primitiveType)
        {
            if (primitiveType == null)
            {
                return null;
            }

            // Spec has different configure for double, AnyOf or OneOf?
            OpenApiSchema schema = new OpenApiSchema();
            switch (primitiveType.PrimitiveKind)
            {
                case EdmPrimitiveTypeKind.Binary: // binary
                    schema.Type = "string";
                    schema.Format = "base64url";
                    break;
                case EdmPrimitiveTypeKind.Boolean: // boolean
                    schema.Type = "boolean";
                    schema.Default = new OpenApiBoolean(false);
                    break;
                case EdmPrimitiveTypeKind.Byte: // byte
                    schema.Type = "integer";
                    schema.Format = "uint8";
                    break;
                case EdmPrimitiveTypeKind.DateTimeOffset: // datetime offset
                    schema.Type = "string";
                    schema.Format = "date-time";
                    schema.Pattern = "^[0-9]{4,}-(0[1-9]|1[012])-(0[1-9]|[12][0-9]|3[01])T([01][0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]([.][0-9]{1,12})?(Z|[+-][0-9][0-9]:[0-9][0-9])$";
                    break;
                case EdmPrimitiveTypeKind.Decimal: // decimal
                    
                    schema.OneOf = new List<OpenApiSchema>
                    {
                        new OpenApiSchema { Type = "number" },
                        new OpenApiSchema { Type = "string" },
                    };
                    schema.Format = "decimal";
                    break;
                case EdmPrimitiveTypeKind.Double: // double
                    schema.OneOf = new List<OpenApiSchema>
                    {
                        new OpenApiSchema { Type = "number" },
                        new OpenApiSchema { Type = "string" },
                        new OpenApiSchema
                        {
                            Enum = new List<IOpenApiAny>
                            {
                                new OpenApiString("-INF"),
                                new OpenApiString("INF"),
                                new OpenApiString("NaN")
                            }
                        }
                    };
                    schema.Format = "double";
                    break;
                case EdmPrimitiveTypeKind.Single: // single
                    schema.OneOf = new List<OpenApiSchema>
                    {
                        new OpenApiSchema { Type = "number" },
                        new OpenApiSchema { Type = "string" },
                        new OpenApiSchema
                        {
                            Enum = new List<IOpenApiAny>
                            {
                                new OpenApiString("-INF"),
                                new OpenApiString("INF"),
                                new OpenApiString("NaN")
                            }
                        }
                    };
                    schema.Format = "float";
                    break;
                case EdmPrimitiveTypeKind.Guid: // guid
                    schema.Type = "string";
                    schema.Format = "uuid";
                    schema.Pattern = "^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$";
                    break;
                case EdmPrimitiveTypeKind.Int16:
                    schema.Type = "integer";
                    schema.Format = "int16";
                    schema.Minimum = Int16.MinValue; // -32768
                    schema.Maximum = Int16.MaxValue; // 32767
                    break;
                case EdmPrimitiveTypeKind.Int32:
                    schema.Type = "integer";
                    schema.Format = "int32";
                    schema.Minimum = Int32.MinValue; // -2147483648
                    schema.Maximum = Int32.MaxValue; // 2147483647
                    break;
                case EdmPrimitiveTypeKind.Int64:
                    schema.OneOf = new List<OpenApiSchema>
                    {
                        new OpenApiSchema { Type = "integer" },
                        new OpenApiSchema { Type = "string" }
                    };
                    schema.Format = "int64";
                    break;
                case EdmPrimitiveTypeKind.SByte:
                    schema.Type = "integer";
                    schema.Format = "int8";
                    schema.Minimum = SByte.MinValue; // -128
                    schema.Maximum = SByte.MaxValue; // 127
                    break;
                case EdmPrimitiveTypeKind.String: // string
                    schema.Type = "string";
                    break;
                case EdmPrimitiveTypeKind.Stream: // stream
                    schema.Type = "string";
                    schema.Format = "base64url";
                    break;
                case EdmPrimitiveTypeKind.Duration: // duration
                    schema.Type = "string";
                    schema.Format = "duration";
                    schema.Pattern = "^-?P([0-9]+D)?(T([0-9]+H)?([0-9]+M)?([0-9]+([.][0-9]+)?S)?)?$";
                    break;
                case EdmPrimitiveTypeKind.Date:
                    schema.Type = "string";
                    schema.Format = "date";
                    schema.Pattern = "^[0-9]{4,}-(0[1-9]|1[012])-(0[1-9]|[12][0-9]|3[01])$";
                    break;
                case EdmPrimitiveTypeKind.TimeOfDay:
                    schema.Type = "string";
                    schema.Format = "time";
                    schema.Pattern = "^([01][0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]([.][0-9]{1,12})?$";
                    break;

                case EdmPrimitiveTypeKind.Geography:
                    schema.Reference = new OpenApiReference
                    {
                        ExternalResource = _externalResource,
                        Id = "definitions/Edm.Geography"
                    };
                    break;
                case EdmPrimitiveTypeKind.GeographyPoint:
                    schema.Reference = new OpenApiReference
                    {
                        ExternalResource = _externalResource,
                        Id = "definitions/Edm.GeographyPoint"
                    };
                    break;
                case EdmPrimitiveTypeKind.GeographyLineString:
                    schema.Reference = new OpenApiReference
                    {
                        ExternalResource = _externalResource,
                        Id = "definitions/Edm.GeographyLineString"
                    };
                    break;
                case EdmPrimitiveTypeKind.GeographyPolygon:
                    schema.Reference = new OpenApiReference
                    {
                        ExternalResource = _externalResource,
                        Id = "definitions/Edm.GeographyPolygon"
                    };
                    break;
                case EdmPrimitiveTypeKind.GeographyCollection:
                    schema.Reference = new OpenApiReference
                    {
                        ExternalResource = _externalResource,
                        Id = "definitions/Edm.GeographyCollection"
                    };
                    break;
                case EdmPrimitiveTypeKind.GeographyMultiPolygon:
                    schema.Reference = new OpenApiReference
                    {
                        ExternalResource = _externalResource,
                        Id = "definitions/Edm.GeographyMultiPolygon"
                    };
                    break;
                case EdmPrimitiveTypeKind.GeographyMultiLineString:
                    schema.Reference = new OpenApiReference
                    {
                        ExternalResource = _externalResource,
                        Id = "definitions/Edm.GeographyMultiLineString"
                    };
                    break;
                case EdmPrimitiveTypeKind.GeographyMultiPoint:
                    schema.Reference = new OpenApiReference
                    {
                        ExternalResource = _externalResource,
                        Id = "definitions/Edm.GeographyMultiPoint"
                    };
                    break;

                case EdmPrimitiveTypeKind.Geometry: // Geometry
                    schema.Reference = new OpenApiReference
                    {
                        ExternalResource = _externalResource,
                        Id = "definitions/Edm.Geometry"
                    };
                    break;
                case EdmPrimitiveTypeKind.GeometryPoint:
                    schema.Reference = new OpenApiReference
                    {
                        ExternalResource = _externalResource,
                        Id = "definitions/Edm.GeometryPoint"
                    };
                    break;
                case EdmPrimitiveTypeKind.GeometryLineString:
                    schema.Reference = new OpenApiReference
                    {
                        ExternalResource = _externalResource,
                        Id = "definitions/Edm.GeometryLineString"
                    };
                    break;
                case EdmPrimitiveTypeKind.GeometryPolygon:
                    schema.Reference = new OpenApiReference
                    {
                        ExternalResource = _externalResource,
                        Id = "definitions/Edm.GeometryPolygon"
                    };
                    break;
                case EdmPrimitiveTypeKind.GeometryCollection:
                    schema.Reference = new OpenApiReference
                    {
                        ExternalResource = _externalResource,
                        Id = "definitions/Edm.GeometryCollection"
                    };
                    break;
                case EdmPrimitiveTypeKind.GeometryMultiPolygon:
                    schema.Reference = new OpenApiReference
                    {
                        ExternalResource = _externalResource,
                        Id = "definitions/Edm.GeometryMultiPolygon"
                    };
                    break;
                case EdmPrimitiveTypeKind.GeometryMultiLineString:
                    schema.Reference = new OpenApiReference
                    {
                        ExternalResource = _externalResource,
                        Id = "definitions/Edm.GeometryMultiLineString"
                    };
                    break;
                case EdmPrimitiveTypeKind.GeometryMultiPoint:
                    schema.Reference = new OpenApiReference
                    {
                        ExternalResource = _externalResource,
                        Id = "definitions/Edm.GeometryMultiPoint"
                    };
                    break;

                case EdmPrimitiveTypeKind.None:
                default:
                    throw new OpenApiException("Not supported primitive type.");
            }

            return schema;
        }

        public static void AppendODataErrors(this IDictionary<string, OpenApiSchema> schemas)
        {
            if (schemas == null)
            {
                return;
            }

            // odata.error
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
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.Schema,
                                Id = "odata.error.main"
                            }
                        }
                    }
                }
            });

            // odata.error.main
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
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.Schema,
                                    Id = "odata.error.detail"
                                }
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

            // odata.error.detail
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

        private static IOpenApiAny CreateDefault(IEdmStructuralProperty property)
        {
            if (property == null ||
                property.DefaultValueString == null ||
                !property.Type.IsPrimitive())
            {
                return null;
            }

            IEdmPrimitiveTypeReference primitiveTypeReference = property.Type.AsPrimitive();
            switch (primitiveTypeReference.PrimitiveKind())
            {
                case EdmPrimitiveTypeKind.Boolean:
                    {
                        bool result;
                        if (Boolean.TryParse(property.DefaultValueString, out result))
                        {
                            return new OpenApiBoolean(result);
                        }
                    }
                    break;

                case EdmPrimitiveTypeKind.Int16:
                case EdmPrimitiveTypeKind.Int32:
                    {
                        int result;
                        if (Int32.TryParse(property.DefaultValueString, out result))
                        {
                            return new OpenApiInteger(result);
                        }
                    }
                    break;

                case EdmPrimitiveTypeKind.Int64:
                    break;

                // The type 'System.Double' is not supported in Open API document.
                case EdmPrimitiveTypeKind.Double:
                    /*
                    {
                        double result;
                        if (Double.TryParse(property.DefaultValueString, out result))
                        {
                            return new OpenApiDouble((float)result);
                        }
                    }*/
                    break;
            }

            return new OpenApiString(property.DefaultValueString);
        }
    }
}
