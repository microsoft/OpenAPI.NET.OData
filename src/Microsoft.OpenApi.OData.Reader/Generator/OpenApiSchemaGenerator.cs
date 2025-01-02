// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Properties;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.Exceptions;
using System.Linq;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.MicrosoftExtensions;
using Microsoft.OpenApi.OData.Vocabulary.Core;
using System.Text.Json.Nodes;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models.References;
using System.Globalization;

namespace Microsoft.OpenApi.OData.Generator
{
    /// <summary>
    /// Extension methods to create <see cref="OpenApiSchema"/> by <see cref="IEdmModel"/>.
    /// </summary>
    internal static class OpenApiSchemaGenerator
    {
        /// <summary>
        /// Adds the component schemas to the Open API document.
        /// </summary>
        /// <param name="context">The OData to Open API context.</param>
        /// <param name="document">The Open API document to use for references lookup.</param>
        public static void AddSchemasToDocument(this ODataContext context, OpenApiDocument document)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(document, nameof(document));

            // append the Edm.Spatial
            foreach(var schema in context.CreateSpatialSchemas(document))
            {
                document.AddComponentSchema(schema.Key, schema.Value);
            }

            // append the OData errors
            foreach(var schema in context.CreateODataErrorSchemas(document))
            {
                document.AddComponentSchema(schema.Key, schema.Value);
            }

            if(context.Settings.EnableDollarCountPath)
                document.AddComponentSchema(Constants.DollarCountSchemaName, new OpenApiSchema {
                    Type = JsonSchemaType.Number,
                    Format = "int64"
                });

            if(context.HasAnyNonContainedCollections())                                        
            {
                document.AddComponentSchema($"String{Constants.CollectionSchemaSuffix}", CreateCollectionSchema(context, new OpenApiSchema { Type = JsonSchemaType.String }, Constants.StringType, document));
            }

            document.AddComponentSchema(Constants.ReferenceUpdateSchemaName, new()
            {
                Type = JsonSchemaType.Object,
                Properties = new Dictionary<string, OpenApiSchema>
                    {
                        {Constants.OdataId, new OpenApiSchema { Type = JsonSchemaType.String, Nullable = false }},
                        {Constants.OdataType, new OpenApiSchema { Type = JsonSchemaType.String, Nullable = true }},
                    }
            });

            document.AddComponentSchema(Constants.ReferenceCreateSchemaName, new()
            {
                Type = JsonSchemaType.Object,
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    {Constants.OdataId, new OpenApiSchema { Type = JsonSchemaType.String, Nullable = false }}
                },
                AdditionalProperties = new OpenApiSchema { Type = JsonSchemaType.Object }
            });

            document.AddComponentSchema(Constants.ReferenceNumericName, new()
            {
                Type = JsonSchemaType.String,
                Nullable = true,
                Enum =
                [
                    "-INF",
                    "INF",
                    "NaN"
                ]
            });

            if (context.Settings.EnableODataAnnotationReferencesForResponses)
            {
                // @odata.nextLink + @odata.count
                if (context.Settings.EnablePagination || context.Settings.EnableCount)
                {
                    var responseSchema = new OpenApiSchema()
                    {
                        Title = "Base collection pagination and count responses",
                        Type = JsonSchemaType.Object,
                    };
                    document.AddComponentSchema(Constants.BaseCollectionPaginationCountResponse, responseSchema);

                    if (context.Settings.EnableCount)
                        responseSchema.Properties.Add(ODataConstants.OdataCount);
                    if (context.Settings.EnablePagination)
                        responseSchema.Properties.Add(ODataConstants.OdataNextLink);
                }

                // @odata.nextLink + @odata.deltaLink
                if (context.Model.SchemaElements.OfType<IEdmFunction>().Any(static x => x.IsDeltaFunction()))
                {
                    document.AddComponentSchema(Constants.BaseDeltaFunctionResponse, new()
                    {
                        Title = "Base delta function response",
                        Type = JsonSchemaType.Object,
                        Properties = new Dictionary<string, OpenApiSchema>
                        {
                            {ODataConstants.OdataNextLink.Key, ODataConstants.OdataNextLink.Value},
                            {ODataConstants.OdataDeltaLink.Key, ODataConstants.OdataDeltaLink.Value}
                        }
                    });
                }
            }

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
                            var fullTypeName = reference.FullTypeName();
                            if(reference is IEdmComplexType &&
                                fullTypeName.Split(['.'], StringSplitOptions.RemoveEmptyEntries)
                                            .Last()
                                            .Equals(context.Settings.InnerErrorComplexTypeName, StringComparison.Ordinal))
                                continue;
                            
                            document.AddComponentSchema(fullTypeName, context.CreateSchemaTypeSchema(reference, document));
                        }
                        break;
                }
            }

            foreach(var collectionEntry in context.GetAllCollectionEntityTypes()
                                        .Select(x => new KeyValuePair<string, OpenApiSchema>(
                                                            $"{(x is IEdmEntityType eType ? eType.FullName() : x.FullTypeName())}{Constants.CollectionSchemaSuffix}",
                                                            CreateCollectionSchema(context, x, document)))
                            .Concat(context.GetAllCollectionComplexTypes()
                                        .Select(x => new KeyValuePair<string, OpenApiSchema>(
                                                            $"{x.FullTypeName()}{Constants.CollectionSchemaSuffix}",
                                                            CreateCollectionSchema(context, x, document))))
                            .ToArray())
            {
                document.AddComponentSchema(collectionEntry.Key, collectionEntry.Value);
            }
        }
        internal static bool HasAnyNonContainedCollections(this ODataContext context)
        {
            return context.Model
                    .SchemaElements
                    .OfType<IEdmStructuredType>()
                    .SelectMany(x => x.NavigationProperties())
                    .Any(x => x.TargetMultiplicity() == EdmMultiplicity.Many && !x.ContainsTarget);
        }
        internal static IEnumerable<IEdmComplexType> GetAllCollectionComplexTypes(this ODataContext context)
        {
            return context.Model
                        .SchemaElements
                        .OfType<IEdmStructuredType>()
                        .SelectMany(x => x.StructuralProperties())
                        .Where(x => x.Type.IsCollection())
                        .Select(x => x.Type.Definition.AsElementType())
                        .OfType<IEdmComplexType>()
                        .Distinct()
                        .ToList();
        }
        internal static IEnumerable<IEdmStructuredType> GetAllCollectionEntityTypes(this ODataContext context)
        {
            var collectionEntityTypes = new HashSet<IEdmStructuredType>(
                                                (context.EntityContainer?
                                                    .EntitySets()
                                                    .Select(x => x.EntityType) ??
                                                Enumerable.Empty<IEdmStructuredType>())
                                                .Union(context.Model
                                                                .SchemaElements
                                                                .OfType<IEdmStructuredType>()
                                                                .SelectMany(x => x.NavigationProperties())
                                                                .Where(x => x.TargetMultiplicity() == EdmMultiplicity.Many)
                                                                .Select(x => x.Type.ToStructuredType()))
                                                .Distinct()); // we could include actions and functions but actions are not pageable by nature (OData.NextLink) and functions might have specific annotations (deltalink)
            var derivedCollectionTypes = collectionEntityTypes.SelectMany(x => context.Model.FindAllDerivedTypes(x).OfType<IEdmStructuredType>())
                                                                .Where(x => !collectionEntityTypes.Contains(x))
                                                                .Distinct()
                                                                .ToArray();
            return collectionEntityTypes.Union(derivedCollectionTypes);
        }

        private static OpenApiSchema CreateCollectionSchema(ODataContext context, IEdmStructuredType structuredType, OpenApiDocument document)
        {
            OpenApiSchema schema = null;
            var entityType = structuredType as IEdmEntityType;

            if (context.Settings.EnableDerivedTypesReferencesForResponses && entityType != null)
            {
                schema = EdmModelHelper.GetDerivedTypesReferenceSchema(entityType, context.Model, document);
            }

            if (schema == null)
            {
                schema = new OpenApiSchemaReference(entityType?.FullName() ?? structuredType.FullTypeName(), document);
            }
            return CreateCollectionSchema(context, schema, entityType?.Name ?? structuredType.FullTypeName(), document);
        }
        private static OpenApiSchema CreateCollectionSchema(ODataContext context, OpenApiSchema schema, string typeName, OpenApiDocument document)
        {
            var properties = new Dictionary<string, OpenApiSchema>
            {
                {
                    "value",
                    new OpenApiSchema
                    {
                        Type = JsonSchemaType.Array,
                        Items = schema
                    }
                }
            };

            OpenApiSchema baseSchema = new()
            {
                Type = JsonSchemaType.Object,
                Properties = properties
            };

            OpenApiSchema collectionSchema;
            if (context.Settings.EnablePagination || context.Settings.EnableCount)
            {
                if (context.Settings.EnableODataAnnotationReferencesForResponses)
                {
                    // @odata.nextLink + @odata.count
                    OpenApiSchema paginationCountSchema = new OpenApiSchemaReference(Constants.BaseCollectionPaginationCountResponse, document);

                    collectionSchema = new OpenApiSchema
                    {
                        AllOf = new List<OpenApiSchema>
                        {
                            paginationCountSchema,
                            baseSchema
                        }
                    };
                }
                else
                {
                    if (context.Settings.EnablePagination)
                        baseSchema.Properties.Add(ODataConstants.OdataNextLink);

                    if (context.Settings.EnableCount)
                        baseSchema.Properties.Add(ODataConstants.OdataCount);

                    collectionSchema = baseSchema;
                }               
            }
            else
            {
                collectionSchema = baseSchema;
            }

            collectionSchema.Title = $"Collection of {typeName}";
            collectionSchema.Type = JsonSchemaType.Object;
            return collectionSchema;
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
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(enumType, nameof(enumType));

            OpenApiSchema schema = new()
            {
                // An enumeration type is represented as a Schema Object of type string
                Type = JsonSchemaType.String,

                // containing the OpenAPI Specification enum keyword.
                Enum = new List<JsonNode>(),

                // It optionally can contain the field description,
                // whose value is the value of the unqualified annotation Core.Description of the enumeration type.
                Description = context.Model.GetDescriptionAnnotation(enumType)
            };
            
            // If the enum is flagged, add the extension info to the description
            if (context.Settings.AddEnumFlagsExtension && enumType.IsFlags)
            {
                var enumFlagsExtension = new OpenApiEnumFlagsExtension
                {
                    IsFlags = true,
                };
                schema.Extensions.Add(OpenApiEnumFlagsExtension.Name, enumFlagsExtension);
            }

            var extension = (context.Settings.OpenApiSpecVersion == OpenApiSpecVersion.OpenApi2_0 ||
                            context.Settings.OpenApiSpecVersion == OpenApiSpecVersion.OpenApi3_0  ||
                            context.Settings.OpenApiSpecVersion == OpenApiSpecVersion.OpenApi3_1) &&
                            context.Settings.AddEnumDescriptionExtension ? 
                                new OpenApiEnumValuesDescriptionExtension {
                                    EnumName = enumType.Name,
                                } : 
                                null;

            // Enum value is an array that contains a string with the member name for each enumeration member.
            foreach (IEdmEnumMember member in enumType.Members)
            {
                schema.Enum.Add(member.Name);
                AddEnumDescription(member, extension, context);
            }

            if(extension?.ValuesDescriptions.Any() ?? false)
                schema.Extensions.Add(OpenApiEnumValuesDescriptionExtension.Name, extension);
            schema.Title = enumType.Name;
            return schema;
        }
        private static void AddEnumDescription(IEdmEnumMember member, OpenApiEnumValuesDescriptionExtension target, ODataContext context)
        {
            if (target == null)
                return;
            
            var enumDescription = context.Model.GetDescriptionAnnotation(member);
            if(!string.IsNullOrEmpty(enumDescription))
                target.ValuesDescriptions.Add(new EnumDescription
                {
                    Name = member.Name,
                    Value = member.Name,
                    Description = enumDescription
                });
        }

        /// <summary>
        /// Create a <see cref="OpenApiSchema"/> for a <see cref="IEdmStructuredType"/>.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="structuredType">The Edm structured type.</param>
        /// <param name="document">The Open API document to lookup references.</param>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        public static OpenApiSchema CreateStructuredTypeSchema(this ODataContext context, IEdmStructuredType structuredType, OpenApiDocument document)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(structuredType, nameof(structuredType));
            Utils.CheckArgumentNull(document, nameof(document));

            return context.CreateStructuredTypeSchema(structuredType, true, true, document);
        }

        /// <summary>
        /// Create a <see cref="OpenApiSchema"/> for a <see cref="IEdmProperty"/>.
        /// Each structural property and navigation property is represented as a name/value pair of the
        /// standard OpenAPI properties object. The name is the property name,
        /// the value is a Schema Object describing the allowed values of the property.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="property">The Edm property.</param>
        /// <param name="document">The Open API document to lookup references.</param>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        public static OpenApiSchema CreatePropertySchema(this ODataContext context, IEdmProperty property, OpenApiDocument document)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(property, nameof(property));
            Utils.CheckArgumentNull(document, nameof(document));

            OpenApiSchema schema = context.CreateEdmTypeSchema(property.Type, document);

            switch (property.PropertyKind)
            {
                case EdmPropertyKind.Structural:
                    IEdmStructuralProperty structuralProperty = (IEdmStructuralProperty)property;
                    schema.Default = CreateDefault(structuralProperty);
                    break;
            }

            // The Schema Object for a property optionally can contain the field description,
            // whose value is the value of the unqualified annotation Core.Description of the property.
            schema.Description = context.Model.GetDescriptionAnnotation(property);

            // Set property with Computed Annotation in CSDL to readonly
            schema.ReadOnly = context.Model.GetBoolean(property, CoreConstants.Computed) ?? false;

            return schema;
        }

        /// <summary>
        /// Create a map of string/<see cref="OpenApiSchema"/> map for a <see cref="IEdmStructuredType"/>'s all properties.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="structuredType">The Edm structured type.</param>
        /// <param name="document">The Open API document to lookup references.</param>
        /// <returns>The created map of <see cref="OpenApiSchema"/>.</returns>
        public static IDictionary<string, OpenApiSchema> CreateStructuredTypePropertiesSchema(this ODataContext context, IEdmStructuredType structuredType, OpenApiDocument document)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(structuredType, nameof(structuredType));

            // The name is the property name, the value is a Schema Object describing the allowed values of the property.
            IDictionary<string, OpenApiSchema> properties = new Dictionary<string, OpenApiSchema>();

            // structure properties
            foreach (var property in structuredType.DeclaredStructuralProperties())
            {
                OpenApiSchema propertySchema = context.CreatePropertySchema(property, document);
                propertySchema.Description = context.Model.GetDescriptionAnnotation(property);
                // we always want a new copy because it's a reference
                propertySchema.Extensions = propertySchema.Extensions is null ? [] : new Dictionary<string, IOpenApiExtension>(propertySchema.Extensions);
                propertySchema.Extensions.AddCustomAttributesToExtensions(context, property);
                properties.Add(property.Name, propertySchema);
            }

            // navigation properties
            foreach (var property in structuredType.DeclaredNavigationProperties())
            {
                OpenApiSchema propertySchema = context.CreateEdmTypeSchema(property.Type, document);
                propertySchema.Description = context.Model.GetDescriptionAnnotation(property);
                // we always want a new copy because it's a reference
                propertySchema.Extensions = propertySchema.Extensions is null ? [] : new Dictionary<string, IOpenApiExtension>(propertySchema.Extensions);
                propertySchema.Extensions.AddCustomAttributesToExtensions(context, property);
                propertySchema.Extensions.Add(Constants.xMsNavigationProperty, new OpenApiAny(true));
                properties.Add(property.Name, propertySchema);
            }

            return properties;
        }

        public static OpenApiSchema CreateSchemaTypeDefinitionSchema(this ODataContext context, IEdmTypeDefinition typeDefinition, OpenApiDocument document)
        {
            return context.CreateSchema(typeDefinition.UnderlyingType, document);
        }

        internal static OpenApiSchema CreateSchemaTypeSchema(this ODataContext context, IEdmType edmType, OpenApiDocument document)
        {
            Debug.Assert(context != null);
            Debug.Assert(edmType != null);

            switch (edmType.TypeKind)
            {
                case EdmTypeKind.Complex: // complex type
                case EdmTypeKind.Entity: // entity type
                    return context.CreateStructuredTypeSchema((IEdmStructuredType)edmType, true, true, document);

                case EdmTypeKind.Enum: // enum type
                    return context.CreateEnumTypeSchema((IEdmEnumType)edmType);

                case EdmTypeKind.TypeDefinition: // type definition
                    return context.CreateSchemaTypeDefinitionSchema((IEdmTypeDefinition)edmType, document);

                case EdmTypeKind.None:
                default:
                    throw Error.NotSupported(string.Format(SRResource.NotSupportedEdmTypeKind, edmType.TypeKind));
            }
        }

        private static OpenApiSchema CreateStructuredTypeSchema(this ODataContext context, IEdmStructuredType structuredType, bool processBase, bool processExample,
            OpenApiDocument document,
            IEnumerable<IEdmStructuredType> derivedTypes = null)
        {
            Debug.Assert(context != null);
            Debug.Assert(structuredType != null);

            JsonNode example = null;
            if (context.Settings.ShowSchemaExamples)
            {
                example = CreateStructuredTypePropertiesExample(context, structuredType, document);
            }

            if (context.Settings.EnableDiscriminatorValue && derivedTypes == null)
            {
                derivedTypes = context.Model.FindAllDerivedTypes(structuredType);
            }

            if (processBase && structuredType.BaseType != null)
            {
                // The x-ms-discriminator-value extension is added to structured types which are derived types.
                Dictionary<string, IOpenApiExtension> extension = null;
                if (context.Settings.EnableDiscriminatorValue && !derivedTypes.Any())
                {
                    extension = new Dictionary<string, IOpenApiExtension>
                    {
                        { Constants.xMsDiscriminatorValue, new OpenApiAny("#" + structuredType.FullTypeName()) }
                    };
                }

                // A structured type with a base type is represented as a Schema Object
                // that contains the keyword allOf whose value is an array with two items:
                return new OpenApiSchema
                {
                    Extensions = extension,

                    AllOf = new List<OpenApiSchema>
                    {
                        // 1. a JSON Reference to the Schema Object of the base type
                        new OpenApiSchemaReference(structuredType.BaseType.FullTypeName(), document),

                        // 2. a Schema Object describing the derived type
                        context.CreateStructuredTypeSchema(structuredType, false, false, document, derivedTypes)
                    },

                    AnyOf = null,
                    OneOf = null,
                    Properties = null,
                    Example = example
                };
            }
            else
            {
                // The discriminator object is added to structured types which have derived types.
                OpenApiDiscriminator discriminator = null;
                if (context.Settings.EnableDiscriminatorValue && derivedTypes.Any())
                {
                    Dictionary<string, string> mapping = derivedTypes
                        .ToDictionary(x => $"#{x.FullTypeName()}", x => new OpenApiSchemaReference(x.FullTypeName(), document).Reference.ReferenceV3);

                    discriminator = new OpenApiDiscriminator
                    {
                        PropertyName = Constants.OdataType,
                        Mapping = mapping
                    };
                }

                // A structured type without a base type is represented as a Schema Object of type object
                OpenApiSchema schema = new()
                {
                    Title = (structuredType as IEdmSchemaElement)?.Name,

                    Type = JsonSchemaType.Object,

                    Discriminator = discriminator,

                    // Each structural property and navigation property is represented
                    // as a name/value pair of the standard OpenAPI properties object.
                    Properties = context.CreateStructuredTypePropertiesSchema(structuredType, document),

                    // make others null
                    AllOf = null,
                    OneOf = null,
                    AnyOf = null
                };

                if (context.Settings.EnableDiscriminatorValue)
                {
                    JsonNode defaultValue = null;
                    bool isBaseTypeEntity = Constants.EntityName.Equals(structuredType.BaseType?.FullTypeName().Split('.').Last(), StringComparison.OrdinalIgnoreCase);
                    bool isBaseTypeAbstractNonEntity = (structuredType.BaseType?.IsAbstract ?? false) && !isBaseTypeEntity;

                    if (!context.Settings.EnableTypeDisambiguationForDefaultValueOfOdataTypeProperty ||
                        isBaseTypeAbstractNonEntity ||
                        context.Model.IsBaseTypeReferencedAsTypeInModel(structuredType.BaseType))
                    {
                        defaultValue = "#" + structuredType.FullTypeName();
                    }

                    if (!schema.Properties.TryAdd(Constants.OdataType, new OpenApiSchema()
                    {
                        Type = JsonSchemaType.String,
                        Default = defaultValue,
                    }))
                    {
                        throw new InvalidOperationException(
                            $"Property {Constants.OdataType} is already present in schema {structuredType.FullTypeName()}; verify CSDL.");
                    }
                    schema.Required.Add(Constants.OdataType);
                }

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

                if (processExample)
                {
                    schema.Example = example;
                }

                return schema;
            }
        }

        internal static JsonObject CreateStructuredTypePropertiesExample(ODataContext context, IEdmStructuredType structuredType, OpenApiDocument document)
        {
            JsonObject example = [];

            // properties
            foreach (var property in structuredType.Properties())
            {
                IEdmTypeReference propertyType = property.Type;

                JsonNode item = GetTypeNameForExample(context, propertyType, document);

                if (propertyType.TypeKind() == EdmTypeKind.Primitive &&
                    item is JsonValue jsonValue &&
                    jsonValue.TryGetValue(out string stringAny) &&
                    structuredType is IEdmEntityType entityType &&
                    entityType.Key().Any(k => StringComparer.Ordinal.Equals(k.Name, property.Name)))
                {
                    item = $"{stringAny} (identifier)";
                }

                example.Add(property.Name, item);
            }

            return example;
        }

        private static JsonNode GetTypeNameForPrimitive(ODataContext context, IEdmTypeReference edmTypeReference, OpenApiDocument document)
        {
            IEdmPrimitiveType primitiveType = edmTypeReference.AsPrimitive().PrimitiveDefinition();
            OpenApiSchema schema = context.CreateSchema(primitiveType, document);

            if (edmTypeReference.IsBoolean())
            {
                return true;
            }
            else
            {
                if (schema.Reference != null)
                {
                    return schema.Reference.Id;
                }
                else
                {
                    return schema.Type.ToIdentifier() ??
                        (schema.AnyOf ?? Enumerable.Empty<OpenApiSchema>())
                        .Union(schema.AllOf ?? Enumerable.Empty<OpenApiSchema>())
                        .Union(schema.OneOf ?? Enumerable.Empty<OpenApiSchema>())
                        .FirstOrDefault(static x => !string.IsNullOrEmpty(x.Format))?.Format ?? schema.Format;
                }
            }
        }

        private static JsonNode GetTypeNameForExample(ODataContext context, IEdmTypeReference edmTypeReference, OpenApiDocument document)
        {
            return edmTypeReference.TypeKind() switch
            {
                // return new OpenApiBinary(new byte[] { 0x00 }); issue on binary writing
                EdmTypeKind.Primitive when edmTypeReference.IsBinary() => Convert.ToBase64String(new byte[] { 0x00 }),
                EdmTypeKind.Primitive when edmTypeReference.IsBoolean() => true,
                EdmTypeKind.Primitive when edmTypeReference.IsByte() => 0x00,
                EdmTypeKind.Primitive when edmTypeReference.IsDate() => DateTime.MinValue.ToString("o", CultureInfo.InvariantCulture),
                EdmTypeKind.Primitive when edmTypeReference.IsDateTimeOffset() => DateTimeOffset.MinValue.ToString("o", CultureInfo.InvariantCulture),
                EdmTypeKind.Primitive when edmTypeReference.IsGuid() => Guid.Empty.ToString("D", CultureInfo.InvariantCulture),
                EdmTypeKind.Primitive when edmTypeReference.IsInt16() ||
                                            edmTypeReference.IsInt32() ||
                                            edmTypeReference.IsDecimal() ||
                                            edmTypeReference.IsInt64() ||
                                            edmTypeReference.IsFloating() ||
                                            edmTypeReference.IsDouble() => 0,
                EdmTypeKind.Primitive => GetTypeNameForPrimitive(context, edmTypeReference, document),

                EdmTypeKind.Entity or EdmTypeKind.Complex or EdmTypeKind.Enum => new JsonObject()
                    {//TODO this is wrong for enums, and should instead use one of the enum members
                        [Constants.OdataType] = edmTypeReference.FullName()
                    },

                EdmTypeKind.Collection => new JsonArray(GetTypeNameForExample(context, edmTypeReference.AsCollection().ElementType(), document)),
                EdmTypeKind.TypeDefinition => GetTypeNameForExample(context, new EdmPrimitiveTypeReference(edmTypeReference.AsTypeDefinition().TypeDefinition().UnderlyingType, edmTypeReference.IsNullable), document),
                EdmTypeKind.Untyped => new JsonObject(),
                _ => throw new OpenApiException("Not support for the type kind " + edmTypeReference.TypeKind()),
            };
        }

        private static JsonNode CreateDefault(this IEdmStructuralProperty property)
        {
            if (property == null ||
                property.DefaultValueString == null)
            {
                return null;
            }

            if (property.Type.IsEnum())
            {
                return property.DefaultValueString;
            }

            if (!property.Type.IsPrimitive())
            {
                return null;
            }

            IEdmPrimitiveTypeReference primitiveTypeReference = property.Type.AsPrimitive();
            switch (primitiveTypeReference.PrimitiveKind())
            {
                case EdmPrimitiveTypeKind.Boolean:
                    {
                        if (bool.TryParse(property.DefaultValueString, out bool result))
                        {
                            return result;
                        }
                    }
                    break;

                case EdmPrimitiveTypeKind.Int16:
                case EdmPrimitiveTypeKind.Int32:
                    {
                        if (int.TryParse(property.DefaultValueString, out int result))
                        {
                            return result;
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

            return property.DefaultValueString;
        }
    }
}
