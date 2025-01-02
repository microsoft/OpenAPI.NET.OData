// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Exceptions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Properties;
using Microsoft.OpenApi.OData.Common;

namespace Microsoft.OpenApi.OData.Generator
{
    /// <summary>
    /// Extension methods to create <see cref="OpenApiSchema"/> for Edm type.
    /// </summary>
    internal static class OpenApiEdmTypeSchemaGenerator
    {
        /// <summary>
        /// Create a <see cref="OpenApiSchema"/> for a <see cref="IEdmTypeReference"/>.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="edmTypeReference">The Edm type reference.</param>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        public static OpenApiSchema CreateEdmTypeSchema(this ODataContext context, IEdmTypeReference edmTypeReference)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(edmTypeReference, nameof(edmTypeReference));

            switch (edmTypeReference.TypeKind())
            {
                // Collection-valued structural and navigation are represented as Schema Objects of type array.
                // The value of the items keyword is a Schema Object specifying the type of the items.
                case EdmTypeKind.Collection:  
                    
                    IEdmTypeReference typeRef = edmTypeReference.AsCollection().ElementType();
                    OpenApiSchema schema;
                    schema = typeRef.TypeKind() == EdmTypeKind.Complex || typeRef.TypeKind() == EdmTypeKind.Entity
                        ? context.CreateStructuredTypeSchema(typeRef.AsStructured(), true)
                        : context.CreateEdmTypeSchema(typeRef);

                    return new OpenApiSchema
                    {
                        Type = "array",
                        Items = schema
                    };

                // Complex, enum, entity, entity reference are represented as JSON References to the Schema Object of the complex,
                // enum, entity and entity reference type, either as local references for types directly defined in the CSDL document,
                // or as external references for types defined in referenced CSDL documents.
                case EdmTypeKind.Complex:
                case EdmTypeKind.Entity:
                    return context.CreateStructuredTypeSchema(edmTypeReference.AsStructured());

                case EdmTypeKind.Enum:
                    return context.CreateEnumTypeSchema(edmTypeReference.AsEnum());

                // Primitive properties of type Edm.PrimitiveType, Edm.Stream, and any of the Edm.Geo* types are
                // represented as Schema Objects that are JSON References to definitions in the Definitions Object
                case EdmTypeKind.Primitive:
                    IEdmPrimitiveTypeReference primitiveTypeReference = (IEdmPrimitiveTypeReference)edmTypeReference;
                    return context.CreateSchema(primitiveTypeReference);

                case EdmTypeKind.TypeDefinition:
                    return context.CreateSchema(((IEdmTypeDefinitionReference)edmTypeReference).TypeDefinition().UnderlyingType);

                case EdmTypeKind.EntityReference:
                    return context.CreateTypeDefinitionSchema(edmTypeReference.AsTypeDefinition());

                case EdmTypeKind.Untyped:
                    return new OpenApiSchema();

                case EdmTypeKind.None:
                default:
                    throw Error.NotSupported(String.Format(SRResource.NotSupportedEdmTypeKind, edmTypeReference.TypeKind()));
            }
        }

        /// <summary>
        /// Create a <see cref="OpenApiSchema"/> for a <see cref="IEdmPrimitiveTypeReference"/>.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="primitiveType">The Edm primitive reference.</param>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        public static OpenApiSchema CreateSchema(this ODataContext context, IEdmPrimitiveTypeReference primitiveType)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(primitiveType, nameof(primitiveType));

            OpenApiSchema schema = context.CreateSchema(primitiveType.PrimitiveDefinition());
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
                // nullable cannot be true when type is empty, often common in anyof/allOf since individual entries are nullable
                schema.Nullable = !string.IsNullOrEmpty(schema.Type) && primitiveType.IsNullable;
            }

            return schema;
        }

        /// <summary>
        /// Create a <see cref="OpenApiSchema"/> for a <see cref="IEdmPrimitiveType"/>.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="primitiveType">The Edm primitive type.</param>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        public static OpenApiSchema CreateSchema(this ODataContext context, IEdmPrimitiveType primitiveType)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(primitiveType, nameof(primitiveType));

            // Spec has different configure for double, AnyOf or OneOf?
            OpenApiSchema schema = new OpenApiSchema
            {
                AllOf = null,
                OneOf = null,
                AnyOf = null
            };

            switch (primitiveType.PrimitiveKind)
            {
                case EdmPrimitiveTypeKind.Binary: // binary
                    schema.Type = Constants.StringType;
                    schema.Format = "base64url";
                    break;
                case EdmPrimitiveTypeKind.Boolean: // boolean
                    schema.Type = "boolean";
                    schema.Default = new OpenApiBoolean(false);
                    break;
                case EdmPrimitiveTypeKind.Byte: // byte
                    schema.Type = Constants.NumberType;
                    schema.Format = "uint8";
                    break;
                case EdmPrimitiveTypeKind.DateTimeOffset: // datetime offset
                    schema.Type = Constants.StringType;
                    schema.Format = "date-time";
                    schema.Pattern = "^[0-9]{4,}-(0[1-9]|1[012])-(0[1-9]|[12][0-9]|3[01])T([01][0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]([.][0-9]{1,12})?(Z|[+-][0-9][0-9]:[0-9][0-9])$";
                    break;
                case EdmPrimitiveTypeKind.Decimal: // decimal
                    if (context.Settings.IEEE754Compatible)
                    {
                        schema.OneOf = new List<OpenApiSchema>
                        {
                            new OpenApiSchema { Type = Constants.NumberType, Format = Constants.DecimalFormat, Nullable = true },
                            new OpenApiSchema { Type = Constants.StringType, Nullable = true },
                        };
                    }
                    else
                    {
                        schema.Type = Constants.NumberType;
                        schema.Format = Constants.DecimalFormat;
                    }
                    break;
                case EdmPrimitiveTypeKind.Double: // double
                    schema.OneOf = new List<OpenApiSchema>
                    {
                        new OpenApiSchema { Type = Constants.NumberType, Format = "double", Nullable = true },
                        new OpenApiSchema { Type = Constants.StringType, Nullable = true },
                        new OpenApiSchema
                        {
                            UnresolvedReference = true,
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.Schema,
                                Id = Constants.ReferenceNumericName
                            }
                        }
                    };
                    break;
                case EdmPrimitiveTypeKind.Single: // single
                    schema.OneOf = new List<OpenApiSchema>
                    {
                        new OpenApiSchema { Type = Constants.NumberType, Format = "float", Nullable = true },
                        new OpenApiSchema { Type = Constants.StringType, Nullable = true },
                        new OpenApiSchema
                        {
                            UnresolvedReference = true,
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.Schema,
                                Id = Constants.ReferenceNumericName
                            }
                        }
                    };
                    break;
                case EdmPrimitiveTypeKind.Guid: // guid
                    schema.Type = Constants.StringType;
                    schema.Format = "uuid";
                    schema.Pattern = "^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$";
                    break;
                case EdmPrimitiveTypeKind.Int16:
                    schema.Type = Constants.NumberType;
                    schema.Format = "int16";
                    schema.Minimum = Int16.MinValue; // -32768
                    schema.Maximum = Int16.MaxValue; // 32767
                    break;
                case EdmPrimitiveTypeKind.Int32:
                    schema.Type = Constants.NumberType;
                    schema.Format = "int32";
                    schema.Minimum = Int32.MinValue; // -2147483648
                    schema.Maximum = Int32.MaxValue; // 2147483647
                    break;
                case EdmPrimitiveTypeKind.Int64:
                    if (context.Settings.IEEE754Compatible)
                    {
                        schema.OneOf = new List<OpenApiSchema>
                        {
                            new OpenApiSchema { Type = Constants.NumberType, Format = Constants.Int64Format, Nullable = true },
                            new OpenApiSchema { Type = Constants.StringType, Nullable = true }
                        };
                    }
                    else
                    {
                        schema.Type = Constants.NumberType;
                        schema.Format = Constants.Int64Format;
                    }
                    break;
                case EdmPrimitiveTypeKind.SByte:
                    schema.Type = Constants.NumberType;
                    schema.Format = "int8";
                    schema.Minimum = SByte.MinValue; // -128
                    schema.Maximum = SByte.MaxValue; // 127
                    break;
                case EdmPrimitiveTypeKind.String: // string
                    schema.Type = Constants.StringType;
                    break;
                case EdmPrimitiveTypeKind.Stream: // stream
                    schema.Type = Constants.StringType;
                    schema.Format = "base64url";
                    break;
                case EdmPrimitiveTypeKind.Duration: // duration
                    schema.Type = Constants.StringType;
                    schema.Format = "duration";
                    schema.Pattern = "^-?P([0-9]+D)?(T([0-9]+H)?([0-9]+M)?([0-9]+([.][0-9]+)?S)?)?$";
                    break;
                case EdmPrimitiveTypeKind.Date:
                    schema.Type = Constants.StringType;
                    schema.Format = "date";
                    schema.Pattern = "^[0-9]{4,}-(0[1-9]|1[012])-(0[1-9]|[12][0-9]|3[01])$";
                    break;
                case EdmPrimitiveTypeKind.TimeOfDay:
                    schema.Type = Constants.StringType;
                    schema.Format = "time";
                    schema.Pattern = "^([01][0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]([.][0-9]{1,12})?$";
                    break;

                case EdmPrimitiveTypeKind.Geography:
                    schema.Reference = new OpenApiReference
                    {
                        Type = ReferenceType.Schema,
                        Id = "Edm.Geography"
                    };
                    schema.UnresolvedReference = true;
                    break;
                case EdmPrimitiveTypeKind.GeographyPoint:
                    schema.Reference = new OpenApiReference
                    {
                        Type = ReferenceType.Schema,
                        Id = "Edm.GeographyPoint"
                    };
                    schema.UnresolvedReference = true;
                    break;
                case EdmPrimitiveTypeKind.GeographyLineString:
                    schema.Reference = new OpenApiReference
                    {
                        Type = ReferenceType.Schema,
                        Id = "Edm.GeographyLineString"
                    };
                    schema.UnresolvedReference = true;
                    break;
                case EdmPrimitiveTypeKind.GeographyPolygon:
                    schema.Reference = new OpenApiReference
                    {
                        Type = ReferenceType.Schema,
                        Id = "Edm.GeographyPolygon"
                    };
                    schema.UnresolvedReference = true;
                    break;
                case EdmPrimitiveTypeKind.GeographyCollection:
                    schema.Reference = new OpenApiReference
                    {
                        Type = ReferenceType.Schema,
                        Id = "Edm.GeographyCollection"
                    };
                    schema.UnresolvedReference = true;
                    break;
                case EdmPrimitiveTypeKind.GeographyMultiPolygon:
                    schema.Reference = new OpenApiReference
                    {
                        Type = ReferenceType.Schema,
                        Id = "Edm.GeographyMultiPolygon"
                    };
                    schema.UnresolvedReference = true;
                    break;
                case EdmPrimitiveTypeKind.GeographyMultiLineString:
                    schema.Reference = new OpenApiReference
                    {
                        Type = ReferenceType.Schema,
                        Id = "Edm.GeographyMultiLineString"
                    };
                    schema.UnresolvedReference = true;
                    break;
                case EdmPrimitiveTypeKind.GeographyMultiPoint:
                    schema.Reference = new OpenApiReference
                    {
                        Type = ReferenceType.Schema,
                        Id = "Edm.GeographyMultiPoint"
                    };
                    schema.UnresolvedReference = true;
                    break;
                case EdmPrimitiveTypeKind.Geometry: // Geometry
                    schema.Reference = new OpenApiReference
                    {
                        Type = ReferenceType.Schema,
                        Id = "Edm.Geometry"
                    };
                    schema.UnresolvedReference = true;
                    break;
                case EdmPrimitiveTypeKind.GeometryPoint:
                    schema.Reference = new OpenApiReference
                    {
                        Type = ReferenceType.Schema,
                        Id = "Edm.GeometryPoint"
                    };
                    schema.UnresolvedReference = true;
                    break;
                case EdmPrimitiveTypeKind.GeometryLineString:
                    schema.Reference = new OpenApiReference
                    {
                        Type = ReferenceType.Schema,
                        Id = "Edm.GeometryLineString"
                    };
                    schema.UnresolvedReference = true;
                    break;
                case EdmPrimitiveTypeKind.GeometryPolygon:
                    schema.Reference = new OpenApiReference
                    {
                        Type = ReferenceType.Schema,
                        Id = "Edm.GeometryPolygon"
                    };
                    schema.UnresolvedReference = true;
                    break;
                case EdmPrimitiveTypeKind.GeometryCollection:
                    schema.Reference = new OpenApiReference
                    {
                        Type = ReferenceType.Schema,
                        Id = "Edm.GeometryCollection"
                    };
                    schema.UnresolvedReference = true;
                    break;
                case EdmPrimitiveTypeKind.GeometryMultiPolygon:
                    schema.Reference = new OpenApiReference
                    {
                        Type = ReferenceType.Schema,
                        Id = "Edm.GeometryMultiPolygon"
                    };
                    schema.UnresolvedReference = true;
                    break;
                case EdmPrimitiveTypeKind.GeometryMultiLineString:
                    schema.Reference = new OpenApiReference
                    {
                        Type = ReferenceType.Schema,
                        Id = "Edm.GeometryMultiLineString"
                    };
                    schema.UnresolvedReference = true;
                    break;
                case EdmPrimitiveTypeKind.GeometryMultiPoint:
                    schema.Reference = new OpenApiReference
                    {
                        Type = ReferenceType.Schema,
                        Id = "Edm.GeometryMultiPoint"
                    };
                    schema.UnresolvedReference = true;
                    break;

                case EdmPrimitiveTypeKind.None:
                default:
                    throw new OpenApiException(String.Format(SRResource.NotSupportedEdmTypeKind, primitiveType.PrimitiveKind));
            }

            return schema;
        }

        private static OpenApiSchema CreateEnumTypeSchema(this ODataContext context, IEdmEnumTypeReference typeReference)
        {
            Debug.Assert(context != null);
            Debug.Assert(typeReference != null);

            OpenApiSchema schema = new OpenApiSchema();
            schema.Reference = null;

            if (typeReference.IsNullable && context.Settings.OpenApiSpecVersion >= OpenApiSpecVersion.OpenApi3_0)
            {
                schema.AnyOf = new List<OpenApiSchema>
                {
                    new OpenApiSchema
                    {
                        UnresolvedReference = true,
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.Schema,
                            Id = typeReference.Definition.FullTypeName()
                        }
                    },
                    new OpenApiSchema
                    {
                        Type = "object",
                        Nullable = true
                    }
                };
            }
            else
            {
                schema.Type = null;
                schema.AnyOf = null;
                schema.Reference = new OpenApiReference
                {
                    Type = ReferenceType.Schema,
                    Id = typeReference.Definition.FullTypeName()
                };
                schema.UnresolvedReference = true;
                schema.Nullable = typeReference.IsNullable;
            }

            return schema;
        }

        private static OpenApiSchema CreateStructuredTypeSchema(this ODataContext context, IEdmStructuredTypeReference typeReference, bool isTypeCollection = false)
        {
            Debug.Assert(context != null);
            Debug.Assert(typeReference != null);

            OpenApiSchema schema = new OpenApiSchema();

            // AnyOf will only be valid openApi for version 3
            // otherwise the reference should be set directly
            // as per OASIS documentation for openApi version 2
            // Collections of structured types cannot be nullable
            if (typeReference.IsNullable && !isTypeCollection &&
                (context.Settings.OpenApiSpecVersion >= OpenApiSpecVersion.OpenApi3_0))
            {
                schema.Reference = null;
                schema.AnyOf = new List<OpenApiSchema>
                {
                    new OpenApiSchema
                    {
                        UnresolvedReference = true,
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.Schema,
                            Id = typeReference.Definition.FullTypeName()
                        }
                    },
                    new OpenApiSchema
                    {
                        Type = "object",
                        Nullable = true
                    }
                };
            }
            else
            {
                schema.Type = null;
                schema.AnyOf = null;
                schema.Reference = new OpenApiReference
                {
                    Type = ReferenceType.Schema,
                    Id = typeReference.Definition.FullTypeName()
                };
                schema.UnresolvedReference = true;
                schema.Nullable = typeReference.IsNullable;
            }

            return schema;
        }

        private static OpenApiSchema CreateTypeDefinitionSchema(this ODataContext context, IEdmTypeDefinitionReference reference)
        {
            Debug.Assert(context != null);
            Debug.Assert(reference != null);

            OpenApiSchema schema = new OpenApiSchema();
            schema.Reference = null;

            if (reference.IsNullable && context.Settings.OpenApiSpecVersion >= OpenApiSpecVersion.OpenApi3_0)
            {
                schema.AnyOf = new List<OpenApiSchema>
                {
                    new OpenApiSchema
                    {
                        UnresolvedReference = true,
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.Schema,
                            Id = reference.Definition.FullTypeName()
                        }
                    },
                    new OpenApiSchema
                    {
                        Type = "object",
                        Nullable = true
                    }
                };
            }
            else
            {
                schema.Type = null;
                schema.AnyOf = null;
                schema.Reference = new OpenApiReference
                {
                    Type = ReferenceType.Schema,
                    Id = reference.Definition.FullTypeName()
                };
                schema.UnresolvedReference = true;
                schema.Nullable = reference.IsNullable;
            }
            

            return schema;
        }
    }
}
