// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.OData.Properties;
using Microsoft.OpenApi.Models;

namespace Microsoft.OpenApi.OData.Generator
{
    /// <summary>
    /// Extension methods to create <see cref="OpenApiSchema"/> by <see cref="IEdmModel"/>.
    /// </summary>
    internal static class OpenApiSchemaGenerator
    {
        /// <summary>
        /// Create the dictionary of <see cref="OpenApiSchema"/> object.
        /// The name of each pair is the namespace-qualified name of the type. It uses the namespace instead of the alias.
        /// The value of each pair is a <see cref="OpenApiSchema"/>.
        /// </summary>
        /// <param name="context">The OData to Open API context.</param>
        /// <returns>The string/schema dictionary.</returns>
        public static IDictionary<string, OpenApiSchema> CreateSchemas(this ODataContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            IDictionary<string, OpenApiSchema> schemas = new Dictionary<string, OpenApiSchema>();

            // Each entity type, complex type, enumeration type, and type definition directly
            // or indirectly used in the paths field is represented as a name / value pair of the schemas map.
            foreach (var element in context.Model.SchemaElements)
            {
                switch (element.SchemaElementKind)
                {
                    case EdmSchemaElementKind.TypeDefinition: // Type definition
                        {
                            IEdmType reference = (IEdmType)element;
                            schemas.Add(reference.FullTypeName(), context.CreateEdmTypeSchema(reference));
                        }
                        break;
                }
            }

            schemas.AppendODataErrors();

            return schemas;
        }

        /// <summary>
        /// Create a <see cref="OpenApiSchema"/> for a <see cref="IEdmType"/>.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="edmType">The Edm type.</param>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        public static OpenApiSchema CreateEdmTypeSchema(this ODataContext context, IEdmType edmType)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            if (edmType == null)
            {
                throw Error.ArgumentNull(nameof(edmType));
            }

            switch (edmType.TypeKind)
            {
                case EdmTypeKind.Complex: // complex type
                case EdmTypeKind.Entity: // entity type
                    return context.CreateStructuredTypeSchema((IEdmStructuredType)edmType, true);

                case EdmTypeKind.Enum: // enum type
                    return context.CreateEnumTypeSchema((IEdmEnumType)edmType);

                case EdmTypeKind.TypeDefinition: // type definition
                    return context.CreateTypeDefinitionSchema((IEdmTypeDefinition)edmType);

                case EdmTypeKind.None:
                default:
                    throw Error.NotSupported(String.Format(SRResource.NotSupportedEdmTypeKind, edmType.TypeKind));
            }
        }

        /// <summary>
        /// Create a <see cref="OpenApiSchema"/> for a <see cref="IEdmEnumType"/>.
        /// An enumeration type is represented as a Schema Object of type string containing the OpenAPI Specification enum keyword.
        /// Its value is an array that contains a string with the member name for each enumeration member.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="enumType">The Edm enum type.</param>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        public static OpenApiSchema CreateEnumTypeSchema(this ODataContext context, IEdmEnumType enumType)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            if (enumType == null)
            {
                throw Error.ArgumentNull(nameof(enumType));
            }

            OpenApiSchema schema = new OpenApiSchema
            {
                // An enumeration type is represented as a Schema Object of type string
                Type = "string",

                // containing the OpenAPI Specification enum keyword.
                Enum = new List<IOpenApiAny>(),

                // It optionally can contain the field description,
                // whose value is the value of the unqualified annotation Core.Description of the enumeration type.
                Description = context.Model.GetDescriptionAnnotation(enumType)
            };

            // Enum value is an array that contains a string with the member name for each enumeration member.
            foreach (IEdmEnumMember member in enumType.Members)
            {
                schema.Enum.Add(new OpenApiString(member.Name));
            }

            schema.Title = enumType.Name;
            return schema;
        }

        /// <summary>
        /// Create a <see cref="OpenApiSchema"/> for a <see cref="IEdmStructuredType"/>.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="structuredType">The Edm structured type.</param>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        public static OpenApiSchema CreateStructuredTypeSchema(this ODataContext context, IEdmStructuredType structuredType)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            if (structuredType == null)
            {
                throw Error.ArgumentNull(nameof(structuredType));
            }

            return context.CreateStructuredTypeSchema(structuredType, true);
        }

        // 4.6.1.1 Properties
        public static IDictionary<string, OpenApiSchema> CreateStructuredTypePropertiesSchema(this ODataContext context, IEdmStructuredType structuredType)
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

        public static OpenApiSchema CreateTypeDefinitionSchema(this ODataContext context, IEdmTypeDefinition typeDefinition)
        {
            return typeDefinition?.UnderlyingType?.CreateSchema();
        }

        private static OpenApiSchema CreateStructuredTypeSchema(this ODataContext context, IEdmStructuredType structuredType, bool processBase)
        {
            Debug.Assert(context != null);
            Debug.Assert(structuredType != null);

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
                        context.CreateStructuredTypeSchema(structuredType, false)
                    },

                    AnyOf = null,
                    OneOf = null,
                    Properties = null
                };
            }
            else
            {
                // A structured type without a base type is represented as a Schema Object of type object
                OpenApiSchema schema = new OpenApiSchema
                {
                    Title = (structuredType as IEdmSchemaElement)?.Name,

                    Type = "object",

                    // Each structural property and navigation property is represented
                    // as a name/value pair of the standard OpenAPI properties object.
                    Properties = context.CreateStructuredTypePropertiesSchema(structuredType),

                    // make others null
                    AllOf = null,
                    OneOf = null,
                    AnyOf = null
                };

                // It optionally can contain the field description,
                // whose value is the value of the unqualified annotation Core.Description of the structured type.
                if (structuredType.TypeKind == EdmTypeKind.Complex)
                {
                    IEdmComplexType complex = (IEdmComplexType)structuredType;
                    schema.Description = context.Model.GetDescriptionAnnotation(complex);
                }
                else if (structuredType.TypeKind == EdmTypeKind.Entity)
                {
                    IEdmEntityType entity = (IEdmEntityType)structuredType;
                    schema.Description = context.Model.GetDescriptionAnnotation(entity);
                }

                return schema;
            }
        }
    }
}
