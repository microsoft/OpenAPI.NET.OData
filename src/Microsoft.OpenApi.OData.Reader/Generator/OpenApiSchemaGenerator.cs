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
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.Exceptions;
using System.Linq;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.MicrosoftExtensions;
using Microsoft.OpenApi.OData.Vocabulary.Core;

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
            Utils.CheckArgumentNull(context, nameof(context));

            IDictionary<string, OpenApiSchema> schemas = new Dictionary<string, OpenApiSchema>();

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
                                fullTypeName.Split(new char[] {'.'}, StringSplitOptions.RemoveEmptyEntries)
                                            .Last()
                                            .Equals(context.Settings.InnerErrorComplexTypeName, StringComparison.Ordinal))
                                continue;
                            
                            schemas.Add(fullTypeName, context.CreateSchemaTypeSchema(reference));
                        }
                        break;
                }
            }

            // append the Edm.Spatial
            foreach(var schema in context.CreateSpatialSchemas())
            {
                schemas[schema.Key] = schema.Value;
            }

            // append the OData errors
            foreach(var schema in context.CreateODataErrorSchemas())
            {
                schemas[schema.Key] = schema.Value;
            }

            if(context.Settings.EnableDollarCountPath)
                schemas[Constants.DollarCountSchemaName] = new OpenApiSchema {
                    Type = "integer",
                    Format = "int32"
                };

            schemas = schemas.Concat(context.GetAllCollectionEntityTypes()
                                        .Select(x => new KeyValuePair<string, OpenApiSchema>(
                                                            $"{(x is IEdmEntityType eType ? eType.FullName() : x.FullTypeName())}{Constants.CollectionSchemaSuffix}",
                                                            CreateCollectionSchema(context, x)))
                                        .Where(x => !schemas.ContainsKey(x.Key)))
                            .Concat(context.GetAllCollectionComplexTypes()
                                        .Select(x => new KeyValuePair<string, OpenApiSchema>(
                                                            $"{x.FullTypeName()}{Constants.CollectionSchemaSuffix}",
                                                            CreateCollectionSchema(context, x)))
                                        .Where(x => !schemas.ContainsKey(x.Key)))
                            .ToDictionary(x => x.Key, x => x.Value);
            
            if(context.HasAnyNonContainedCollections())                                        
            {
                schemas[$"String{Constants.CollectionSchemaSuffix}"] = CreateCollectionSchema(context, new OpenApiSchema { Type = Constants.StringType }, Constants.StringType);
            }

            schemas[Constants.ReferenceUpdateSchemaName] = new()
            {
                Type = Constants.ObjectType,
                Properties = new Dictionary<string, OpenApiSchema>
                    {
                        {Constants.OdataId, new OpenApiSchema { Type = Constants.StringType, Nullable = false }},
                        {Constants.OdataType, new OpenApiSchema { Type = Constants.StringType, Nullable = true }},
                    }
            };

            schemas[Constants.ReferenceCreateSchemaName] = new()
            {
                Type = Constants.ObjectType,
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    {Constants.OdataId, new OpenApiSchema { Type = Constants.StringType, Nullable = false }}
                },
                AdditionalProperties = new OpenApiSchema { Type = Constants.ObjectType }
            };

            schemas[Constants.ReferenceNumericName] = new()
            {
                Type = Constants.StringType,
                Nullable = true,
                Enum =
                [
                    new OpenApiString("-INF"),
                    new OpenApiString("INF"),
                    new OpenApiString("NaN")
                ]
            };

            if (context.Settings.EnableODataAnnotationReferencesForResponses)
            {
                // @odata.nextLink + @odata.count
                if (context.Settings.EnablePagination || context.Settings.EnableCount)
                {
                    schemas[Constants.BaseCollectionPaginationCountResponse] = new()
                    {
                        Title = "Base collection pagination and count responses",
                        Type = Constants.ObjectType,
                    };

                    if (context.Settings.EnableCount)
                        schemas[Constants.BaseCollectionPaginationCountResponse].Properties.Add(ODataConstants.OdataCount);
                    if (context.Settings.EnablePagination)
                        schemas[Constants.BaseCollectionPaginationCountResponse].Properties.Add(ODataConstants.OdataNextLink);
                }

                // @odata.nextLink + @odata.deltaLink
                if (context.Model.SchemaElements.OfType<IEdmFunction>().Any(static x => x.IsDeltaFunction()))
                {
                    schemas[Constants.BaseDeltaFunctionResponse] = new()
                    {
                        Title = "Base delta function response",
                        Type = Constants.ObjectType
                    };
                    schemas[Constants.BaseDeltaFunctionResponse].Properties.Add(ODataConstants.OdataNextLink);
                    schemas[Constants.BaseDeltaFunctionResponse].Properties.Add(ODataConstants.OdataDeltaLink);
                }
            }

            return schemas;
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

        private static OpenApiSchema CreateCollectionSchema(ODataContext context, IEdmStructuredType structuredType)
        {
            OpenApiSchema schema = null;
            var entityType = structuredType as IEdmEntityType;

            if (context.Settings.EnableDerivedTypesReferencesForResponses && entityType != null)
            {
                schema = EdmModelHelper.GetDerivedTypesReferenceSchema(entityType, context.Model);
            }

            if (schema == null)
            {
                schema = new OpenApiSchema
                {
                    UnresolvedReference = true,
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.Schema,
                        Id = entityType?.FullName() ?? structuredType.FullTypeName()
                    }
                };
            }
            return CreateCollectionSchema(context, schema, entityType?.Name ?? structuredType.FullTypeName());
        }
        private static OpenApiSchema CreateCollectionSchema(ODataContext context, OpenApiSchema schema, string typeName)
        {
            var properties = new Dictionary<string, OpenApiSchema>
            {
                {
                    "value",
                    new OpenApiSchema
                    {
                        Type = "array",
                        Items = schema
                    }
                }
            };

            OpenApiSchema baseSchema = new()
            {
                Type = Constants.ObjectType,
                Properties = properties
            };

            OpenApiSchema collectionSchema;
            if (context.Settings.EnablePagination || context.Settings.EnableCount)
            {
                if (context.Settings.EnableODataAnnotationReferencesForResponses)
                {
                    // @odata.nextLink + @odata.count
                    OpenApiSchema paginationCountSchema = new()
                    {
                        UnresolvedReference = true,
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.Schema,
                            Id = Constants.BaseCollectionPaginationCountResponse
                        }
                    };

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
            collectionSchema.Type = Constants.ObjectType;
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
                Type = Constants.StringType,

                // containing the OpenAPI Specification enum keyword.
                Enum = new List<IOpenApiAny>(),

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
                            context.Settings.OpenApiSpecVersion == OpenApiSpecVersion.OpenApi3_0 ) &&
                            context.Settings.AddEnumDescriptionExtension ? 
                                new OpenApiEnumValuesDescriptionExtension {
                                    EnumName = enumType.Name,
                                } : 
                                null;

            // Enum value is an array that contains a string with the member name for each enumeration member.
            foreach (IEdmEnumMember member in enumType.Members)
            {
                schema.Enum.Add(new OpenApiString(member.Name));
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
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        public static OpenApiSchema CreateStructuredTypeSchema(this ODataContext context, IEdmStructuredType structuredType)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(structuredType, nameof(structuredType));

            return context.CreateStructuredTypeSchema(structuredType, true, true);
        }

        /// <summary>
        /// Create a <see cref="OpenApiSchema"/> for a <see cref="IEdmProperty"/>.
        /// Each structural property and navigation property is represented as a name/value pair of the
        /// standard OpenAPI properties object. The name is the property name,
        /// the value is a Schema Object describing the allowed values of the property.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="property">The Edm property.</param>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        public static OpenApiSchema CreatePropertySchema(this ODataContext context, IEdmProperty property)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(property, nameof(property));

            OpenApiSchema schema = context.CreateEdmTypeSchema(property.Type);

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
        /// <returns>The created map of <see cref="OpenApiSchema"/>.</returns>
        public static IDictionary<string, OpenApiSchema> CreateStructuredTypePropertiesSchema(this ODataContext context, IEdmStructuredType structuredType)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(structuredType, nameof(structuredType));

            // The name is the property name, the value is a Schema Object describing the allowed values of the property.
            IDictionary<string, OpenApiSchema> properties = new Dictionary<string, OpenApiSchema>();

            // structure properties
            foreach (var property in structuredType.DeclaredStructuralProperties())
            {
                OpenApiSchema propertySchema = context.CreatePropertySchema(property);
                propertySchema.Description = context.Model.GetDescriptionAnnotation(property);
                propertySchema.Extensions.AddCustomAttributesToExtensions(context, property);
                properties.Add(property.Name, propertySchema);
            }

            // navigation properties
            foreach (var property in structuredType.DeclaredNavigationProperties())
            {
                OpenApiSchema propertySchema = context.CreateEdmTypeSchema(property.Type);
                propertySchema.Description = context.Model.GetDescriptionAnnotation(property);
                propertySchema.Extensions.AddCustomAttributesToExtensions(context, property);
                propertySchema.Extensions.Add(Constants.xMsNavigationProperty, new OpenApiBoolean(true));
                properties.Add(property.Name, propertySchema);
            }

            return properties;
        }

        public static OpenApiSchema CreateSchemaTypeDefinitionSchema(this ODataContext context, IEdmTypeDefinition typeDefinition)
        {
            return context.CreateSchema(typeDefinition.UnderlyingType);
        }

        internal static OpenApiSchema CreateSchemaTypeSchema(this ODataContext context, IEdmType edmType)
        {
            Debug.Assert(context != null);
            Debug.Assert(edmType != null);

            switch (edmType.TypeKind)
            {
                case EdmTypeKind.Complex: // complex type
                case EdmTypeKind.Entity: // entity type
                    return context.CreateStructuredTypeSchema((IEdmStructuredType)edmType, true, true);

                case EdmTypeKind.Enum: // enum type
                    return context.CreateEnumTypeSchema((IEdmEnumType)edmType);

                case EdmTypeKind.TypeDefinition: // type definition
                    return context.CreateSchemaTypeDefinitionSchema((IEdmTypeDefinition)edmType);

                case EdmTypeKind.None:
                default:
                    throw Error.NotSupported(String.Format(SRResource.NotSupportedEdmTypeKind, edmType.TypeKind));
            }
        }

        private static OpenApiSchema CreateStructuredTypeSchema(this ODataContext context, IEdmStructuredType structuredType, bool processBase, bool processExample,
            IEnumerable<IEdmStructuredType> derivedTypes = null)
        {
            Debug.Assert(context != null);
            Debug.Assert(structuredType != null);

            IOpenApiAny example = null;
            if (context.Settings.ShowSchemaExamples)
            {
                example = CreateStructuredTypePropertiesExample(context, structuredType);
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
                        { Constants.xMsDiscriminatorValue, new OpenApiString("#" + structuredType.FullTypeName()) }
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
                        new OpenApiSchema
                        {
                            UnresolvedReference = true,
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.Schema,
                                Id = structuredType.BaseType.FullTypeName()
                            }
                        },

                        // 2. a Schema Object describing the derived type
                        context.CreateStructuredTypeSchema(structuredType, false, false, derivedTypes)
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
                        .ToDictionary(x => $"#{x.FullTypeName()}", x => new OpenApiSchema
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.Schema,
                                Id = x.FullTypeName()
                            }
                        }.Reference.ReferenceV3);

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

                    Type = Constants.ObjectType,

                    Discriminator = discriminator,

                    // Each structural property and navigation property is represented
                    // as a name/value pair of the standard OpenAPI properties object.
                    Properties = context.CreateStructuredTypePropertiesSchema(structuredType),

                    // make others null
                    AllOf = null,
                    OneOf = null,
                    AnyOf = null
                };

                if (context.Settings.EnableDiscriminatorValue)
                {
                    OpenApiString defaultValue = null;
                    bool isBaseTypeEntity = Constants.EntityName.Equals(structuredType.BaseType?.FullTypeName().Split('.').Last(), StringComparison.OrdinalIgnoreCase);
                    bool isBaseTypeAbstractNonEntity = (structuredType.BaseType?.IsAbstract ?? false) && !isBaseTypeEntity;

                    if (!context.Settings.EnableTypeDisambiguationForDefaultValueOfOdataTypeProperty ||
                        isBaseTypeAbstractNonEntity ||
                        context.Model.IsBaseTypeReferencedAsTypeInModel(structuredType.BaseType))
                    {
                        defaultValue = new("#" + structuredType.FullTypeName());
                    }

                    if (!schema.Properties.TryAdd(Constants.OdataType, new OpenApiSchema()
                    {
                        Type = Constants.StringType,
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

        private static IOpenApiAny CreateStructuredTypePropertiesExample(ODataContext context, IEdmStructuredType structuredType)
        {
            OpenApiObject example = new OpenApiObject();

            IEdmEntityType entityType = structuredType as IEdmEntityType;

            // properties
            foreach (var property in structuredType.Properties())
            {
               // IOpenApiAny item;
                IEdmTypeReference propertyType = property.Type;

                IOpenApiAny item = GetTypeNameForExample(context, propertyType);

                EdmTypeKind typeKind = propertyType.TypeKind();
                if (typeKind == EdmTypeKind.Primitive && item is OpenApiString)
                {
                    OpenApiString stringAny = item as OpenApiString;
                    string value = stringAny.Value;
                    if (entityType != null && entityType.Key().Any(k => k.Name == property.Name))
                    {
                        value += " (identifier)";
                    }
                    if (propertyType.IsDateTimeOffset() || propertyType.IsDate() || propertyType.IsTimeOfDay())
                    {
                        value += " (timestamp)";
                    }
                    item = new OpenApiString(value);
                }

                example.Add(property.Name, item);
            }

            return example;
        }

        private static IOpenApiAny GetTypeNameForExample(ODataContext context, IEdmTypeReference edmTypeReference)
        {
            switch (edmTypeReference.TypeKind())
            {
                case EdmTypeKind.Primitive:
                    IEdmPrimitiveType primitiveType = edmTypeReference.AsPrimitive().PrimitiveDefinition();
                    OpenApiSchema schema = context.CreateSchema(primitiveType);

                    if (edmTypeReference.IsBoolean())
                    {
                        return new OpenApiBoolean(true);
                    }
                    else
                    {
                        if (schema.Reference != null)
                        {
                            return new OpenApiString(schema.Reference.Id);
                        }
                        else
                        {
                            return new OpenApiString(schema.Type ??
                                (schema.AnyOf ?? Enumerable.Empty<OpenApiSchema>())
                                .Union(schema.AllOf ?? Enumerable.Empty<OpenApiSchema>())
                                .Union(schema.OneOf ?? Enumerable.Empty<OpenApiSchema>())
                                .FirstOrDefault(static x => !string.IsNullOrEmpty(x.Format))?.Format ?? schema.Format);
                        }
                    }

                case EdmTypeKind.Entity:
                case EdmTypeKind.Complex:
                case EdmTypeKind.Enum:
                    OpenApiObject obj = new OpenApiObject();
                    obj[Constants.OdataType] = new OpenApiString(edmTypeReference.FullName());
                    return obj;

                case EdmTypeKind.Collection:
                    OpenApiArray array = new OpenApiArray();
                    IEdmTypeReference elementType = edmTypeReference.AsCollection().ElementType();
                    array.Add(GetTypeNameForExample(context, elementType));
                    return array;

                case EdmTypeKind.Untyped:
                    return new OpenApiObject();

                case EdmTypeKind.TypeDefinition:
                case EdmTypeKind.EntityReference:
                default:
                    throw new OpenApiException("Not support for the type kind " + edmTypeReference.TypeKind());
            }
        }

        private static IOpenApiAny CreateDefault(this IEdmStructuralProperty property)
        {
            if (property == null ||
                property.DefaultValueString == null)
            {
                return null;
            }

            if (property.Type.IsEnum())
            {
                return new OpenApiString(property.DefaultValueString);
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
