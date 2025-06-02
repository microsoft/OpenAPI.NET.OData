// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;

namespace Microsoft.OpenApi.OData.Common
{
    internal static class EdmModelHelper
    {
        /// <summary>
        /// Adds the derived types references together with their base type reference in the OneOf property of an OpenAPI schema.
        /// </summary>
        /// <returns>The OpenAPI schema with the list of derived types references and their base type references set in the OneOf property.</returns>
        internal static OpenApiSchema? GetDerivedTypesReferenceSchema(IEdmStructuredType structuredType, IEdmModel edmModel, OpenApiDocument document)
        {
            Utils.CheckArgumentNull(structuredType, nameof(structuredType));
            Utils.CheckArgumentNull(edmModel, nameof(edmModel));
            if (structuredType is not IEdmSchemaElement schemaElement) throw new ArgumentException("The type is not a schema element.", nameof(structuredType));

            IEnumerable<IEdmSchemaElement> derivedTypes = edmModel.FindAllDerivedTypes(structuredType).OfType<IEdmSchemaElement>();

            if (!derivedTypes.Any())
            {
                return null;
            }

            OpenApiSchema schema = new()
            {
                OneOf = []
            };

            var baseTypeSchema = new OpenApiSchemaReference(schemaElement.FullName(), document);
            schema.OneOf.Add(baseTypeSchema);

            foreach (IEdmSchemaElement derivedType in derivedTypes)
            {
                var derivedTypeSchema = new OpenApiSchemaReference(derivedType.FullName(), document);
                schema.OneOf.Add(derivedTypeSchema);
            }

            return schema;
        }

        /// <summary>
        /// Verifies whether the provided navigation restrictions allow for navigability of a navigation property. 
        /// </summary>
        /// <param name="restrictionType">The <see cref="NavigationRestrictionsType"/>.</param>
        /// <param name="restrictionProperty">The <see cref="NavigationPropertyRestriction"/>.</param>
        /// <returns>true, if navigability is allowed, otherwise false.</returns>
        internal static bool NavigationRestrictionsAllowsNavigability(
            NavigationRestrictionsType? restrictionType,
            NavigationPropertyRestriction? restrictionProperty)
        {
            // Verify using individual navigation restriction first
            if (restrictionProperty?.Navigability != null && restrictionProperty.Navigability.Value == NavigationType.None)
            {
                return false;
            }

            // if the individual has no navigability setting, use the global navigability setting
            // Default navigability for all navigation properties of the annotation target.
            // Individual navigation properties can override this value via `RestrictedProperties/Navigability`.
            return restrictionProperty?.Navigability != null || restrictionType == null || restrictionType.IsNavigable;
        }

        /// <summary>
        /// Generates the operation id from a navigation property path.
        /// </summary>
        /// <param name="path">The target <see cref="ODataPath"/>.</param>
        /// <param name="context">The OData context.</param>
        /// <param name="prefix">Optional: Identifier indicating whether it is a collection-valued non-indexed or single-valued navigation property.</param>
        /// <returns>The operation id generated from the given navigation property path.</returns>
        internal static string? GenerateNavigationPropertyPathOperationId(ODataPath path, ODataContext context, string? prefix = null)
        {
            IList<string> items = RetrieveNavigationPropertyPathsOperationIdSegments(path, context);

            if (!items.Any())
                return null;

            int lastItemIndex = items[^1].StartsWith('-') ? items.Count - 2 : items.Count - 1;
            
            if (!string.IsNullOrEmpty(prefix))
            {
                items[lastItemIndex] = prefix + Utils.UpperFirstChar(items[lastItemIndex]);
            }
            else if (Utils.UpperFirstChar(items[lastItemIndex]) is string lastIdentifier)
            {
                items[lastItemIndex] = lastIdentifier;
            }

            return GenerateNavigationPropertyPathOperationId(items);
        }

        /// <summary>
        /// Generates the operation id from a complex property path.
        /// </summary>
        /// <param name="path">The target <see cref="ODataPath"/>.</param>
        /// <param name="context">The OData context.</param>
        /// <param name="prefix">Optional: Identifier indicating whether it is a collection-valued or single-valued complex property.</param>
        /// <returns>The operation id generated from the given complex property path.</returns>
        internal static string? GenerateComplexPropertyPathOperationId(ODataPath path, ODataContext context, string? prefix = null)
        {
            IList<string> items = RetrieveNavigationPropertyPathsOperationIdSegments(path, context);

            if (!items.Any())
                return null;

            if (path.Segments.Skip(1).OfType<ODataComplexPropertySegment>()?.Last()?.Identifier is string lastSegmentIdentifier)
                if (!string.IsNullOrEmpty(prefix))
                {
                    items.Add(prefix + Utils.UpperFirstChar(lastSegmentIdentifier));
                }
                else if (Utils.UpperFirstChar(lastSegmentIdentifier) is string lastIdentifier)
                {
                    items.Add(lastIdentifier);
                }

            return GenerateNavigationPropertyPathOperationId(items);
        }

        /// <summary>
        /// Generates a navigation property operation id from a list of string values.
        /// </summary>
        /// <param name="items">The list of string values.</param>
        /// <returns>The generated navigation property operation id.</returns>
        private static string? GenerateNavigationPropertyPathOperationId(IList<string> items)
        {
            if (!items.Any())
                return null;

            return string.Join(".", items).Replace(".-", "-", StringComparison.OrdinalIgnoreCase); // Format any hashed value appropriately (this will be the last value)
        }

        /// <summary>
        /// Retrieves the segments of an operation id generated from a navigation property path.
        /// </summary>
        /// <param name="path">The target <see cref="ODataPath"/>.</param>
        /// <param name="context">The OData context.</param>
        /// <returns>The segments of an operation id generated from the given navigation property path.</returns>
        internal static IList<string> RetrieveNavigationPropertyPathsOperationIdSegments(ODataPath path, ODataContext context)
        {
            Utils.CheckArgumentNull(path, nameof(path));

            if (path.FirstSegment is not ODataNavigationSourceSegment {NavigationSource: IEdmNavigationSource navigationSource})
                throw new InvalidOperationException("The first segment of the path is not a navigation source segment.");

            var items = new List<string>
            {
                navigationSource.Name
            };

            // For navigation property paths with odata type cast segments
            // the OData type cast segments identifiers will be used in the operation id
            // The same applies for navigation property paths with operation segments.
            IEnumerable<ODataSegment> segments = path.Segments.Skip(1)
                .Where(static s => 
                s is ODataNavigationPropertySegment ||
                s is ODataTypeCastSegment ||
                s is ODataOperationSegment ||
                s is ODataKeySegment);
            Utils.CheckArgumentNull(segments, nameof(segments));

            string? previousTypeCastSegmentId = null;
            string pathHash = string.Empty;

            foreach (var segment in segments)
            {
                if (segment is ODataNavigationPropertySegment navPropSegment)
                {
                    items.Add(navPropSegment.NavigationProperty.Name);
                }
                else if (segment is ODataTypeCastSegment typeCastSegment
                    && path.Kind != ODataPathKind.TypeCast // ex: ~/NavSource/NavProp/TypeCast
                    && !(path.Kind == ODataPathKind.DollarCount && path.Segments[path.Segments.Count - 2]?.Kind == ODataSegmentKind.TypeCast)) // ex: ~/NavSource/NavProp/TypeCast/$count
                {
                    // Only the last OData type cast segment identifier is added to the operation id
                    if (!string.IsNullOrEmpty(previousTypeCastSegmentId))
                        items.Remove(previousTypeCastSegmentId);
                    if (typeCastSegment.StructuredType is IEdmSchemaElement schemaElement)
                    {
                        previousTypeCastSegmentId = "As" + Utils.UpperFirstChar(schemaElement.Name);
                        items.Add(previousTypeCastSegmentId);
                    }
                }
                else if (segment is ODataOperationSegment operationSegment && !string.IsNullOrEmpty(operationSegment.Identifier))
                {
                    // Navigation property generated via composable function
                    if (operationSegment.Operation is IEdmFunction function && context.Model.IsOperationOverload(function))
                    {
                        // Hash the segment to avoid duplicate operationIds
                        pathHash = string.IsNullOrEmpty(pathHash)
                            ? operationSegment.GetPathHash(context.Settings)
                            : (pathHash + operationSegment.GetPathHash(context.Settings)).GetHashSHA256()[..4];
                    }

                    items.Add(operationSegment.Identifier);
                }
                else if (segment is ODataKeySegment keySegment && keySegment.IsAlternateKey)
                {
                    // We'll consider alternate keys in the operation id to eliminate potential duplicates with operation id of primary path                    
                    if (segment == segments.Last())
                    {                        
                        items.Add("By" + string.Join("", keySegment.Identifier.Split(',').Select(static x => Utils.UpperFirstChar(x))));
                    }
                    else
                    {
                        items.Add(keySegment.Identifier);
                    }
                }
            }

            if (!string.IsNullOrEmpty(pathHash))
            {
                items.Add("-" + pathHash);
            }

            return items;
        }

        /// <summary>
        /// Generates the tag name from a navigation property path.
        /// </summary>
        /// <param name="path">The target <see cref="ODataPath"/>.</param>
        /// <param name="context">The <see cref="ODataContext"/>.</param>
        /// <returns>The tag name generated from the given navigation property path.</returns>
        internal static string GenerateNavigationPropertyPathTagName(ODataPath path, ODataContext context)
        {
            Utils.CheckArgumentNull(path, nameof(path));
            Utils.CheckArgumentNull(context, nameof(context));

            if (path.FirstSegment is not ODataNavigationSourceSegment {NavigationSource: IEdmNavigationSource navigationSource })
                throw new InvalidOperationException("The first segment of the path is not a navigation source segment.");
            
            var items = new List<string>
            {
                navigationSource.Name
            };

            if (path.OfType<ODataNavigationPropertySegment>()?.Last()?.NavigationProperty is not IEdmNavigationProperty navigationProperty)
                throw new InvalidOperationException("The last segment of the path is not a navigation property segment.");

            foreach (var segment in path.Segments.Skip(1).OfType<ODataNavigationPropertySegment>().Select(static x => x.NavigationProperty))
            {
                if (segment == navigationProperty)
                {
                    items.Add(navigationProperty.ToEntityType().Name);
                    break;
                }
                else
                {
                    if (items.Count >= context.Settings.TagDepth - 1)
                    {
                        items.Add(segment.ToEntityType().Name);
                        break;
                    }
                    else
                    {
                        items.Add(segment.Name);
                    }
                }
            }

            return string.Join(".", items);
        }

        /// <summary>
        /// Generates the tag name from a complex property path.
        /// </summary>
        /// <param name="path">The target <see cref="ODataPath"/>.</param>
        /// <param name="context">The <see cref="ODataContext"/>.</param>
        /// <returns>The tag name generated from the given complex property path.</returns>
        internal static string GenerateComplexPropertyPathTagName(ODataPath path, ODataContext context)
        {
            Utils.CheckArgumentNull(path, nameof(path));
            Utils.CheckArgumentNull(context, nameof(context));

            if (path.Segments.OfType<ODataComplexPropertySegment>()?.Last() is not ODataComplexPropertySegment complexSegment)
                throw new InvalidOperationException("The last segment of the path is not a complex property segment.");

            // Get the segment before the last complex type segment
            int complexSegmentIndex = path.Segments.IndexOf(complexSegment);
            ODataSegment preComplexSegment = path.Segments[complexSegmentIndex - 1];
            int preComplexSegmentIndex = path.Segments.IndexOf(preComplexSegment);

            while (preComplexSegment is ODataTypeCastSegment)
            {
                // Skip this segment,
                // Tag names don't include OData type cast segment identifiers 
                preComplexSegmentIndex--;
                preComplexSegment = path.Segments[preComplexSegmentIndex];
            }

            string? tagName = null;

            if (preComplexSegment is ODataNavigationSourceSegment sourceSegment)
            {
                tagName = $"{sourceSegment.NavigationSource.Name}";
            }
            else if (preComplexSegment is ODataNavigationPropertySegment)
            {
                tagName = GenerateNavigationPropertyPathTagName(path, context);
            }
            else if (preComplexSegment is ODataKeySegment)
            {
                var prevKeySegment = path.Segments[preComplexSegmentIndex - 1];
                if (prevKeySegment is ODataNavigationPropertySegment)
                {
                    tagName = GenerateNavigationPropertyPathTagName(path, context);
                }
                else if (prevKeySegment is ODataNavigationSourceSegment sourceSegment1)
                {
                    tagName = $"{sourceSegment1.NavigationSource.Name}";
                }
            }

            List<string> tagNameItems = tagName?.Split('.').ToList() ?? [];
            
            if (tagNameItems.Count < context.Settings.TagDepth && complexSegment.ComplexType is not null)
            {
                tagNameItems.Add(complexSegment.ComplexType.Name);
            }

            return string.Join(".", tagNameItems);
        }

        /// <summary>
        /// Generates the operation id prefix from an OData type cast path.
        /// </summary>
        /// <param name="path">The target <see cref="ODataPath"/>.</param>
        /// <param name="context">The OData context.</param>
        /// <param name="includeListOrGetPrefix">Optional: Whether to include the List or Get prefix to the generated operation id.</param>
        /// <returns>The operation id prefix generated from the OData type cast path.</returns>
        internal static string? GenerateODataTypeCastPathOperationIdPrefix(ODataPath path, ODataContext context, bool includeListOrGetPrefix = true)
        {
            // Get the segment before the last OData type cast segment
            if (path.Segments.OfType<ODataTypeCastSegment>()?.Last() is not ODataTypeCastSegment typeCastSegment)
                throw new InvalidOperationException("The last segment of the path is not a type cast segment.");

            int typeCastSegmentIndex = path.Segments.IndexOf(typeCastSegment);
            
            // The segment 1 place before the last OData type cast segment
            ODataSegment secondLastSegment = path.Segments[typeCastSegmentIndex - 1];

            bool isIndexedCollValuedNavProp = false;
            if (secondLastSegment is ODataKeySegment)
            {
                // The segment 2 places before the last OData type cast segment
                ODataSegment thirdLastSegment = path.Segments[typeCastSegmentIndex - 2];
                if (thirdLastSegment is ODataNavigationPropertySegment)
                {
                    isIndexedCollValuedNavProp = true;
                }
            }

            ODataNavigationSourceSegment? navigationSourceSegment = path.FirstSegment as ODataNavigationSourceSegment;
            IEdmSingleton? singleton = navigationSourceSegment?.NavigationSource as IEdmSingleton;
            IEdmEntitySet? entitySet = navigationSourceSegment?.NavigationSource as IEdmEntitySet;

            string? operationId = null;
            if (secondLastSegment is ODataComplexPropertySegment complexSegment)
            {
                string? listOrGet = includeListOrGetPrefix ? (complexSegment.Property.Type.IsCollection() ? "List" : "Get") : null;
                operationId = GenerateComplexPropertyPathOperationId(path, context, listOrGet);
            }
            else if (secondLastSegment is ODataNavigationPropertySegment navPropSegment)
            {
                string? prefix = null;
                if (includeListOrGetPrefix)
                {
                    prefix = navPropSegment?.NavigationProperty.TargetMultiplicity() == EdmMultiplicity.Many ? "List" : "Get";
                }

                operationId = GenerateNavigationPropertyPathOperationId(path, context, prefix);
            }
            else if (secondLastSegment is ODataKeySegment keySegment)
            {
                if (isIndexedCollValuedNavProp)
                {
                    operationId = GenerateNavigationPropertyPathOperationId(path, context, "Get");
                }
                else
                {
                    string entityTypeName = keySegment.EntityType.Name;
                    string? getPrefix = includeListOrGetPrefix ? "Get" : null;
                    string operationName = $"{getPrefix}{Utils.UpperFirstChar(entityTypeName)}";
                    if (keySegment.IsAlternateKey)
                    {
                        string alternateKeyName = string.Join("", keySegment.Identifier.Split(',').Select(static x => Utils.UpperFirstChar(x)));
                        operationName = $"{operationName}By{alternateKeyName}";
                    }
                    if (entitySet != null)
                    {
                        operationId = entitySet.Name;
                    }
                    else if (singleton != null)
                    {
                        operationId = singleton.Name;
                    }
                    operationId += $".{entityTypeName}.{operationName}";
                }
            }
            else if (secondLastSegment is ODataNavigationSourceSegment)
            {
                if (entitySet != null)
                {
                    operationId = entitySet.Name + "." + entitySet.EntityType.Name + $".{(includeListOrGetPrefix ? "List" : null)}" + Utils.UpperFirstChar(entitySet.EntityType.Name);
                }
                else if (singleton != null)
                {
                    operationId = singleton.Name + "." + singleton.EntityType.Name + $".{(includeListOrGetPrefix ? "Get" : null)}" + Utils.UpperFirstChar(singleton.EntityType.Name);
                }
            }

            return operationId;
        }

        /// <summary>
        /// Strips or aliases namespace prefixes from an element name.
        /// </summary>
        /// <param name="element">The target element.</param>
        /// <param name="model">Optional: The Edm model. Used for searching for the namespace alias.</param>
        /// <param name="settings">The OpenAPI convert settings.</param>
        /// <returns>The element name, alias-prefixed or namespace-stripped if applicable.</returns>
        internal static string StripOrAliasNamespacePrefix(IEdmSchemaElement element, OpenApiConvertSettings settings, IEdmModel? model = null)
        {
            Utils.CheckArgumentNull(element, nameof(element));
            Utils.CheckArgumentNull(settings, nameof(settings));

            string namespaceAlias = string.Empty;
            string namespaceName = element.Namespace;
            string segmentName = element.FullName();        

            if (!string.IsNullOrEmpty(namespaceName) && model != null)
            {
                namespaceAlias = model.GetNamespaceAlias(namespaceName);
            }         

            if (element is IEdmStructuredType && settings.EnableAliasForTypeCastSegments && !string.IsNullOrEmpty(namespaceAlias))
            {
                // Alias type cast segment name
                segmentName = namespaceAlias.TrimEnd('.') + "." + element.Name;
            }
            
            if (element is IEdmOperation)
            {                
                if (settings.EnableAliasForOperationSegments && !string.IsNullOrEmpty(namespaceAlias))
                {
                    // Alias operation segment name 
                    segmentName = namespaceAlias.TrimEnd('.') + "." + element.Name;
                }
                
                if (!string.IsNullOrEmpty(settings.NamespacePrefixToStripForInMethodPaths) && 
                    element.Namespace.Equals(settings.NamespacePrefixToStripForInMethodPaths, StringComparison.OrdinalIgnoreCase))
                {
                    // Strip specified namespace from operation segment name.
                    // If the namespace prefix to strip matches the namespace name,
                    // and the alias has been appended, the alias will be stripped.
                    segmentName = element.Name;
                }
            }

            return segmentName;
        }

        /// <summary>
        /// Checks whether an operation is allowed on a model element.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="edmOperation">The target operation.</param>
        /// <param name="annotatable">The model element.</param>
        /// <param name="operationAllowed">Optional: Default is true.
        /// The operation will be allowed by default if the annotation Org.OData.Core.V1.RequiresExplicitBinding is undefined for the given operation. </param>
        /// <returns>true if the operation is allowed, otherwise false.</returns>
        internal static bool IsOperationAllowed(IEdmModel model, IEdmOperation edmOperation, IEdmVocabularyAnnotatable annotatable, bool operationAllowed = true)
        {
            Utils.CheckArgumentNull(model, nameof(model));
            Utils.CheckArgumentNull(edmOperation, nameof(edmOperation));
            Utils.CheckArgumentNull(annotatable, nameof(annotatable));

            var requiresExplicitBinding = model.FindVocabularyAnnotations(edmOperation).FirstOrDefault(x => x.Term.Name == CapabilitiesConstants.RequiresExplicitBindingName);
            if (requiresExplicitBinding == null)
            {
                return operationAllowed;
            }
            
            var boundOperations = model.GetCollection(annotatable, CapabilitiesConstants.ExplicitOperationBindings)?.ToList();
            return boundOperations != null && boundOperations.Contains(edmOperation.FullName());
        }
    }
}