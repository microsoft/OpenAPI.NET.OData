// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Vocabulary;

namespace Microsoft.OpenApi.OData.Common
{
    /// <summary>
    /// Utilities methods
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Get the term qualified name when using the type of <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The type of the term.</typeparam>
        /// <returns>The qualified name.</returns>
        public static string GetTermQualifiedName<T>()
        {
            object[] attributes = typeof(T).GetCustomAttributes(typeof(TermAttribute), false);
            if (attributes == null && attributes.Length == 0)
            {
                return null;
            }

            TermAttribute term = (TermAttribute)attributes[0];
            return term.QualifiedName;
        }

        /// <summary>
        /// Upper the first character of the string.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>The changed string.</returns>
        public static string UpperFirstChar(string input)
        {
            if (input == null)
            {
                return input;
            }

            char first = char.ToUpper(input[0]);
            return first + input.Substring(1);
        }

        /// <summary>
        /// Get an unique name.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <param name="set">The input set.</param>
        /// <returns>The changed string.</returns>
        public static string GetUniqueName(string input, HashSet<string> set)
        {
            if (!set.Contains(input))
            {
                set.Add(input);
                return input;
            }

            int index = 1;
            string newInput;
            do
            {
                newInput = input + index.ToString();
                index++;
            }
            while (set.Contains(newInput));

            set.Add(newInput);
            return newInput;
        }

        /// <summary>
        /// Check the input argument whether its value is null or not.
        /// </summary>
        /// <typeparam name="T">The input value type.</typeparam>
        /// <param name="value">The input value</param>
        /// <param name="parameterName">The input parameter name.</param>
        /// <returns>The input value.</returns>
        internal static T CheckArgumentNull<T>(T value, string parameterName) where T : class
        {
            if (null == value)
            {
                throw new ArgumentNullException(parameterName);
            }

            return value;
        }

        /// <summary>
        /// Check the input string null or empty.
        /// </summary>
        /// <param name="value">The input string</param>
        /// <param name="parameterName">The input parameter name.</param>
        /// <returns>The input value.</returns>
        internal static string CheckArgumentNullOrEmpty(string value, string parameterName)
        {
            if (String.IsNullOrEmpty(value))
            {
                throw Error.ArgumentNullOrEmpty(parameterName);
            }

            return value;
        }

        /// <summary>
        /// Lowers the first character of the string.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>The changed string.</returns>
        internal static string ToFirstCharacterLowerCase(this string input)
            => string.IsNullOrEmpty(input) ? input : $"{char.ToLowerInvariant(input.FirstOrDefault())}{input.Substring(1)}";

        /// <summary>
        /// Gets the navigation path.
        /// </summary>
        /// <param name="path">The <see cref="ODataPath"/>.</param>
        /// <param name="navigationPropertyName">Optional: The navigation property name.</param>
        internal static string NavigationPropertyPath(this ODataPath path, string navigationPropertyName = null)
        {
            string value = string.Join("/",
                path.Segments.OfType<ODataNavigationPropertySegment>().Select(e => e.Identifier));
            return navigationPropertyName == null ? value : $"{value}/{navigationPropertyName}";
        }

        /// <summary>
        /// Adds a mapping of custom extension values against custom attribute values for a given element to the provided
        /// extensions object.
        /// </summary>
        /// <param name="extensions">The target extensions object in which the mapped extensions and custom attribute
        /// values will be added to.</param>
        /// <param name="context">The OData context.</param>
        /// <param name="element">The target element.</param>
        internal static void AddCustomAttributesToExtensions(this IDictionary<string, IOpenApiExtension> extensions, ODataContext context, IEdmElement element)
        {
            if (extensions  == null ||
                context == null ||
                element == null)
            {
                return;
            }

            Dictionary<string, string> atrributesValueMap = GetCustomXMLAttributesValueMapping(context.Model, element, context.Settings.CustomXMLAttributesMapping);

            if (atrributesValueMap?.Any() ?? false)
            {
                foreach (var item in atrributesValueMap)
                {
                    extensions.TryAdd(item.Key, new OpenApiString(item.Value));
                }
            }
        }

        /// <summary>
        /// Correlates and retrieves custom attribute values for a given element in an Edm model
        /// from a provided dictionary mapping of attribute names and extension names.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="element">The target element.</param>
        /// <param name="customXMLAttributesMapping">The dictionary mapping of attribute names and extension names.</param>
        /// <returns>A dictionary of extension names mapped to the custom attribute values.</returns>
        private static Dictionary<string, string> GetCustomXMLAttributesValueMapping(IEdmModel model, IEdmElement element, Dictionary<string, string> customXMLAttributesMapping)
        {
            Dictionary<string, string> atrributesValueMap = new();

            if ((!customXMLAttributesMapping?.Any() ?? true) ||
                model == null ||
                element == null)
            {
                return atrributesValueMap;
            }

            foreach (var item in customXMLAttributesMapping)
            {
                string attributeName = item.Key.Split(':').Last(); // example, 'ags:IsHidden' --> 'IsHidden'
                string extensionName = item.Value;
                EdmStringConstant customXMLAttribute = model.DirectValueAnnotationsManager.GetDirectValueAnnotations(element)?
                                .Where(x => x.Name.Equals(attributeName, StringComparison.OrdinalIgnoreCase))?
                                .FirstOrDefault()?.Value as EdmStringConstant;
                string attributeValue = customXMLAttribute?.Value;

                if (!string.IsNullOrEmpty(attributeValue))
                {
                    atrributesValueMap.TryAdd(extensionName, attributeValue);
                }
            }

            return atrributesValueMap;
        }

        /// <summary>
        /// Checks whether the base type of an <see cref="IEdmStructuredType"/> is referenced as a type within the Edm model.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="baseType">The base type of the target <see cref="IEdmStructuredType"/>.</param>
        /// <param name="structuredTypes">Optional: The IEnumerable of <see cref="IEdmStructuredType"/> to check against.</param>
        /// <param name="actions">Optional: The IEnumerable of <see cref="IEdmAction"/> to check against.</param>
        /// <returns>True if reference is found, otherwise False.</returns>
        internal static bool IsBaseTypeReferencedAsTypeInModel(
            this IEdmModel model,
            IEdmStructuredType baseType,
            IEnumerable<IEdmStructuredType> structuredTypes = null,
            IEnumerable<IEdmAction> actions = null)
        {
            string baseTypeName = baseType?.FullTypeName();
            bool isBaseTypeEntity = Constants.EntityName.Equals(baseTypeName?.Split('.').Last(), StringComparison.OrdinalIgnoreCase);

            if (!string.IsNullOrEmpty(baseTypeName) && !isBaseTypeEntity)
            {
                structuredTypes ??= model.GetAllElements()
                        .Where(static x => x.SchemaElementKind == EdmSchemaElementKind.TypeDefinition)
                        .Where(static y => !y.Name.Equals(Constants.EntityName, StringComparison.OrdinalIgnoreCase))
                        .OfType<IEdmStructuredType>();

                actions ??= model.GetAllElements()
                        .Where(static x => x.SchemaElementKind == EdmSchemaElementKind.Action)
                        .OfType<IEdmAction>();

                // Is base type referenced as a type in any property within a structured type
                bool isReferencedInStructuredType = structuredTypes
                    .Any(x => x.DeclaredProperties.Where(y => y.Type.TypeKind() == EdmTypeKind.Entity ||
                                                            y.Type.TypeKind() == EdmTypeKind.Collection ||
                                                            y.Type.TypeKind() == EdmTypeKind.Complex)
                    .Any(z => z.Type.FullName().Equals(baseTypeName, StringComparison.OrdinalIgnoreCase)));
                if (isReferencedInStructuredType) return true;

                // Is base type referenced as a type in any parameter in an action
                bool isReferencedInAction = actions.Any(x => x.Parameters.Any(x => x.Type.FullName().Equals(baseTypeName, StringComparison.OrdinalIgnoreCase)));
                if (isReferencedInAction) return true;

                // Recursively check the base type
                return model.IsBaseTypeReferencedAsTypeInModel(baseType.BaseType, structuredTypes, actions);
            }

            return false;
        }

        /// <summary>
        /// Gets the entity type of the target <paramref name="segment"/>.
        /// </summary>
        /// <param name="segment">The target <see cref="ODataSegment"/>.</param>
        /// <returns>The entity type of the target <paramref name="segment"/>.</returns>
        internal static IEdmEntityType EntityTypeFromPathSegment(this ODataSegment segment)
        {
            CheckArgumentNull(segment, nameof(segment));

            switch (segment)
            {
                case ODataNavigationPropertySegment navPropSegment:
                    return navPropSegment.EntityType;
                case ODataNavigationSourceSegment navSourceSegment when navSourceSegment.NavigationSource is IEdmEntitySet entitySet:
                    return entitySet.EntityType;
                case ODataNavigationSourceSegment navSourceSegment when navSourceSegment.NavigationSource is IEdmSingleton singleton:
                    return singleton.EntityType;
                case ODataKeySegment keySegment:
                    return keySegment.EntityType;
                case ODataOperationSegment:
                    return segment.EntityTypeFromOperationSegment();
                default:
                    return null;
            }
        }

        /// <summary>
        /// Gets the entity type of the <paramref name="segment"/>.
        /// </summary>
        /// <param name="segment">The target <see cref="ODataOperationSegment"/>.</param>
        /// <returns>The entity type of the target <paramref name="segment"/>.</returns>
        private static IEdmEntityType EntityTypeFromOperationSegment(this ODataSegment segment)
        {
            CheckArgumentNull(segment, nameof(segment));

            if (segment is ODataOperationSegment operationSegment &&
            operationSegment.Operation.Parameters.FirstOrDefault() is IEdmOperationParameter bindingParameter)
            {
                IEdmTypeReference bindingType = bindingParameter.Type;

                if (bindingType.IsCollection())
                {
                    bindingType = bindingType.AsCollection().ElementType();
                }

                return bindingType.AsEntity().EntityDefinition();
            }

            return null;
        }

        /// <summary>
        /// Attempts to add the specified <paramref name="path"/> and <paramref name="pathItem"/> to the <paramref name="pathItems"/> dictionary. 
        /// </summary>
        /// <param name="pathItems">The target dictionary.</param>
        /// <param name="context">The OData context</param>
        /// <param name="path">The key to be added.</param>
        /// <param name="pathItem">The value to be added.</param>
        /// <returns>true when the key and/or value are successfully added/updated to the dictionary; 
        /// false when the dictionary already contains the specified key, and nothing gets added.</returns>
        internal static bool TryAddPath(this IDictionary<string, OpenApiPathItem> pathItems,
            ODataContext context,
            ODataPath path,
            OpenApiPathItem pathItem)
        {
            CheckArgumentNull(pathItems, nameof(pathItems));
            CheckArgumentNull(context, nameof(context));
            CheckArgumentNull(path, nameof(path));
            CheckArgumentNull(pathItem, nameof(pathItem));

            OpenApiConvertSettings settings = context.Settings.Clone();
            settings.EnableKeyAsSegment = context.KeyAsSegment;

            string pathName = path.PathTemplate ?? path.GetPathItemName(settings);

            if (!pathItems.TryAdd(pathName, pathItem))
            {
                if (path.LastSegment is not ODataOperationSegment lastSegment)
                {
                    Debug.WriteLine("Duplicate path: " + pathName);
                    return false;
                }

                int secondLastSegmentIndex = 2;
                if (path.Count < secondLastSegmentIndex)
                {
                    Debug.WriteLine($"Invalid path. Operation not bound to any entity. Path: {pathName}");
                    return false;
                }

                ODataSegment lastSecondSegment = path.Segments.ElementAt(path.Count - secondLastSegmentIndex);
                IEdmEntityType boundEntityType = lastSecondSegment?.EntityTypeFromPathSegment();

                IEdmEntityType operationEntityType = lastSegment.EntityTypeFromOperationSegment();
                IEnumerable<IEdmStructuredType> derivedTypes = (operationEntityType != null)
                    ? context.Model.FindAllDerivedTypes(operationEntityType)
                    : null;

                if (derivedTypes?.Any() ?? false)
                {
                    if (boundEntityType != null && !derivedTypes.Contains(boundEntityType))
                    {
                        Debug.WriteLine($"Duplicate paths present but entity type of binding parameter '{operationEntityType}' " +
                                        $"is not the base type of the bound entity type '{boundEntityType}'. Path: {pathName}");
                    }
                    return false;
                }
                else
                {
                    // Function bound to a derived type; what was added before was a function bound to a base type,
                    // update the existing dictionary entry.
                    pathItems[pathName] = pathItem;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Strips off a prefix value from a string.
        /// </summary>
        /// <param name="value">The target string value.</param>
        /// <param name="prefix">The prefix value to strip off.</param>
        /// <returns>The value with the prefix stripped off.</returns>
        internal static string StripNamespacePrefix(this string value, string prefix)
        {
            CheckArgumentNullOrEmpty(value, nameof(value));
            CheckArgumentNullOrEmpty(prefix, nameof(prefix));

            // Trim trailing '.' for uniformity
            prefix = prefix.TrimEnd('.');

            return value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                ? value.Substring(prefix.Length).TrimStart('.')
                : value;
        }
    }
}