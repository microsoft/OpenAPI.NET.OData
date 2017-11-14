// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Microsoft.OpenApi.OData.Generators
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
        /// <param name="model">The Edm model.</param>
        /// <returns>The info object.</returns>
        public static IDictionary<string, OpenApiSchema> CreateSchemas(this IEdmModel model)
        {
            if (model == null)
            {
                throw Error.ArgumentNull(nameof(model));
            }

            IDictionary<string, OpenApiSchema> schemas = new Dictionary<string, OpenApiSchema>();

            // Each entity type, complex type, enumeration type, and type definition directly
            // or indirectly used in the paths field is represented as a name / value pair of the schemas map.
            foreach (var element in model.SchemaElements)
            {
                switch (element.SchemaElementKind)
                {
                    case EdmSchemaElementKind.TypeDefinition: // Type definition
                        {
                            IEdmType reference = (IEdmType)element;
                            schemas.Add(reference.FullTypeName(), VisitSchemaType(model, reference));
                        }
                        break;
                }
            }

            schemas.AppendODataErrors();
            return schemas;
        }

        private static OpenApiSchema VisitSchemaType(IEdmModel model, IEdmType definition)
        {
            switch (definition.TypeKind)
            {
                case EdmTypeKind.Complex: // complex type
                case EdmTypeKind.Entity: // entity type
                    return VisitStructuredType((IEdmStructuredType)definition, true);

                case EdmTypeKind.Enum: // enum type
                    return VisitEnumType(model, (IEdmEnumType)definition);

                case EdmTypeKind.TypeDefinition: // type definition
                    return VisitTypeDefinitions((IEdmTypeDefinition)definition);

                case EdmTypeKind.None:
                default:
                    throw Error.NotSupported(String.Format("Not supported {0} type kind.", definition.TypeKind));
            }
        }

        // 4.6.1.2 Schemas for Enumeration Types
        private static OpenApiSchema VisitEnumType(IEdmModel model, IEdmEnumType enumType)
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

        // 4.6.1.3 Schemas for Type Definitions
        private static  OpenApiSchema VisitTypeDefinitions(IEdmTypeDefinition typeDefinition)
        {
            return typeDefinition?.UnderlyingType?.CreateSchema();
        }

        // 4.6.1.1 Schemas for Entity Types and Complex Types
        private static OpenApiSchema VisitStructuredType(IEdmStructuredType structuredType, bool processBase)
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
        private static IDictionary<string, OpenApiSchema> VisitStructuredTypeProperties(IEdmStructuredType structuredType)
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
    }
}
