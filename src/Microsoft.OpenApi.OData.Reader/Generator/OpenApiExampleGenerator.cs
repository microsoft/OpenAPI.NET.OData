// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Exceptions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.Generator
{
    /// <summary>
    /// Extension methods to create <see cref="OpenApiExample"/> by <see cref="ODataContext"/>.
    /// </summary>
    internal static class OpenApiExampleGenerator
    {
        /// <summary>
        /// Create the dictionary of <see cref="OpenApiExample"/> object.
        /// </summary>
        /// <param name="context">The OData to Open API context.</param>
        /// <returns>The created <see cref="OpenApiExample"/> dictionary.</returns>
        public static IDictionary<string, OpenApiExample> CreateExamples(this ODataContext context)
        {
            Utils.CheckArgumentNull(context, nameof(context));

            IDictionary<string, OpenApiExample> examples = new Dictionary<string, OpenApiExample>();

            // Each entity type, complex type, enumeration type, and type definition directly
            // or indirectly used in the paths field is represented as a name / value pair of the schemas map.
            // Ideally this would be driven off the types used in the paths, but in practice, it is simply
            // all of the types present in the model.
            IEnumerable<IEdmSchemaElement> elements = context.Model.GetAllElements();

            foreach (var element in elements)
            {
                switch (element.SchemaElementKind)
                {
                    case EdmSchemaElementKind.TypeDefinition: // Type definition
                        {
                            IEdmType reference = (IEdmType)element;
                            OpenApiExample example = context.CreateExample(reference);
                            if (example != null)
                            {
                                examples.Add(reference.FullTypeName(), example);
                            }
                        }
                        break;
                }
            }

            return examples;
        }

        private static OpenApiExample CreateExample(this ODataContext context, IEdmType edmType)
        {
            Debug.Assert(context != null);
            Debug.Assert(edmType != null);

            switch (edmType.TypeKind)
            {
                case EdmTypeKind.Complex: // complex type
                case EdmTypeKind.Entity: // entity type
                    return CreateStructuredTypeExample((IEdmStructuredType)edmType);
            }

            return null;
        }

        private static OpenApiExample CreateStructuredTypeExample(IEdmStructuredType structuredType)
        {
            OpenApiExample example = new OpenApiExample();

            OpenApiObject value = new OpenApiObject();

            IEdmEntityType entityType = structuredType as IEdmEntityType;

            // properties
            foreach (var property in structuredType.DeclaredProperties.OrderBy(p => p.Name))
            {
                // IOpenApiAny item;
                IEdmTypeReference propertyType = property.Type;

                IOpenApiAny item = GetTypeNameForExample(propertyType);

                EdmTypeKind typeKind = propertyType.TypeKind();
                if (typeKind == EdmTypeKind.Primitive && item is OpenApiString)
                {
                    OpenApiString stringAny = item as OpenApiString;
                    string propertyValue = stringAny.Value;
                    if (entityType != null && entityType.Key().Any(k => k.Name == property.Name))
                    {
                        propertyValue += " (identifier)";
                    }
                    if (propertyType.IsDateTimeOffset() || propertyType.IsDate() || propertyType.IsTimeOfDay())
                    {
                        propertyValue += " (timestamp)";
                    }
                    item = new OpenApiString(propertyValue);
                }

                value.Add(property.Name, item);
            }
            example.Value = value;
            return example;
        }

        private static IOpenApiAny GetTypeNameForExample(IEdmTypeReference edmTypeReference)
        {
            switch (edmTypeReference.TypeKind())
            {
                case EdmTypeKind.Primitive:
                    if (edmTypeReference.IsBinary())
                    {
                        // return new OpenApiBinary(new byte[] { 0x00 }); issue on binary writing
                        return new OpenApiString(Convert.ToBase64String(new byte[] { 0x00 }));
                    }
                    else if (edmTypeReference.IsBoolean())
                    {
                        return new OpenApiBoolean(true);
                    }
                    else if (edmTypeReference.IsByte())
                    {
                        return new OpenApiByte(0x00);
                    }
                    else if (edmTypeReference.IsDate())
                    {
                        return new OpenApiDate(DateTime.MinValue);
                    }
                    else if (edmTypeReference.IsDateTimeOffset())
                    {
                        return new OpenApiDateTime(DateTimeOffset.MinValue);
                    }
                    else if (edmTypeReference.IsDecimal() || edmTypeReference.IsDouble())
                    {
                        return new OpenApiDouble(0D);
                    }
                    else if (edmTypeReference.IsFloating())
                    {
                        return new OpenApiFloat(0F);
                    }
                    else if (edmTypeReference.IsGuid())
                    {
                        return new OpenApiString(Guid.Empty.ToString());
                    }
                    else if (edmTypeReference.IsInt16() || edmTypeReference.IsInt32())
                    {
                        return new OpenApiInteger(0);
                    }
                    else if (edmTypeReference.IsInt64())
                    {
                        return new OpenApiLong(0L);
                    }
                    else
                    {
                        return new OpenApiString(edmTypeReference.AsPrimitive().PrimitiveDefinition().Name);
                    }

                case EdmTypeKind.Entity:
                case EdmTypeKind.Complex:
                case EdmTypeKind.Enum:
                    OpenApiObject obj = new OpenApiObject();
                    obj["@odata.type"] = new OpenApiString(edmTypeReference.FullName());
                    return obj;

                case EdmTypeKind.Collection:
                    OpenApiArray array = new OpenApiArray();
                    IEdmTypeReference elementType = edmTypeReference.AsCollection().ElementType();
                    array.Add(GetTypeNameForExample(elementType));
                    return array;

                case EdmTypeKind.TypeDefinition:
                    var typedef = edmTypeReference.AsTypeDefinition().TypeDefinition();
                    return GetTypeNameForExample(new EdmPrimitiveTypeReference(typedef.UnderlyingType, edmTypeReference.IsNullable));

                case EdmTypeKind.Untyped:
                    return new OpenApiObject();

                case EdmTypeKind.EntityReference:
                default:
                    throw new OpenApiException("Not support for the type kind " + edmTypeReference.TypeKind());
            }
        }
    }
}
