// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.Json.Nodes;
using Microsoft.OData.Edm;
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
            OpenApiExample example = new();

            JsonObject value = new();

            // properties
            foreach (var property in structuredType.DeclaredProperties.OrderBy(static p => p.Name, StringComparer.Ordinal))
            {
                IEdmTypeReference propertyType = property.Type;

                JsonNode item = GetTypeNameForExample(propertyType);

                if (propertyType.TypeKind() == EdmTypeKind.Primitive &&
                    item is JsonValue jsonValue &&
                    jsonValue.TryGetValue(out string stringAny) &&
                    structuredType is IEdmEntityType entityType &&
                    entityType.Key().Any(k => StringComparer.Ordinal.Equals(k.Name, property.Name)))
                {
                    item = $"{stringAny} (identifier)";
                }

                value.Add(property.Name, item);
            }
            example.Value = value;
            return example;
        }

        private static JsonNode GetTypeNameForExample(IEdmTypeReference edmTypeReference)
        {
            return edmTypeReference.TypeKind() switch
            {
                // return new OpenApiBinary(new byte[] { 0x00 }); issue on binary writing
                EdmTypeKind.Primitive when edmTypeReference.IsBinary() => Convert.ToBase64String(new byte[] { 0x00 }),
                EdmTypeKind.Primitive when edmTypeReference.IsBoolean() => true,
                EdmTypeKind.Primitive when edmTypeReference.IsByte() => 0x00,
                EdmTypeKind.Primitive when edmTypeReference.IsDate() => DateTime.MinValue,
                EdmTypeKind.Primitive when edmTypeReference.IsDateTimeOffset() => DateTimeOffset.MinValue,
                EdmTypeKind.Primitive when edmTypeReference.IsDecimal() || edmTypeReference.IsDouble() => 0D,
                EdmTypeKind.Primitive when edmTypeReference.IsFloating() => 0F,
                EdmTypeKind.Primitive when edmTypeReference.IsGuid() => Guid.Empty.ToString(),
                EdmTypeKind.Primitive when edmTypeReference.IsInt16() || edmTypeReference.IsInt32() => 0,
                EdmTypeKind.Primitive when edmTypeReference.IsInt64() => 0L,
                EdmTypeKind.Primitive => edmTypeReference.AsPrimitive().PrimitiveDefinition().Name,
                EdmTypeKind.Entity or EdmTypeKind.Complex or EdmTypeKind.Enum => new JsonObject()
                    {//TODO this is wrong for enums, and should instead use one of the enum members
                        [Constants.OdataType] = edmTypeReference.FullName()
                    },

                EdmTypeKind.Collection => new JsonArray(GetTypeNameForExample(edmTypeReference.AsCollection().ElementType())),

                EdmTypeKind.TypeDefinition => GetTypeNameForExample(new EdmPrimitiveTypeReference(edmTypeReference.AsTypeDefinition().TypeDefinition().UnderlyingType, edmTypeReference.IsNullable)),

                EdmTypeKind.Untyped => new JsonObject(),

                _ => throw new OpenApiException("Not support for the type kind " + edmTypeReference.TypeKind()),
            };
        }
    }
}
