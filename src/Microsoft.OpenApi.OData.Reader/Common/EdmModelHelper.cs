// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.Models;
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
        internal static OpenApiSchema GetDerivedTypesReferenceSchema(IEdmStructuredType structuredType, IEdmModel edmModel)
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
                OneOf = new List<OpenApiSchema>()
            };

            OpenApiSchema baseTypeSchema = new()
            {
                UnresolvedReference = true,
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.Schema,
                    Id = schemaElement.FullName()
                }
            };
            schema.OneOf.Add(baseTypeSchema);

            foreach (IEdmSchemaElement derivedType in derivedTypes)
            {
                OpenApiSchema derivedTypeSchema = new()
                {
                    UnresolvedReference = true,
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.Schema,
                        Id = derivedType.FullName()
                    }
                };
                schema.OneOf.Add(derivedTypeSchema);
            };

            return schema;
        }

        /// <summary>
        /// Verifies whether the provided navigation restrictions allow for navigability of a navigation property. 
        /// </summary>
        /// <param name="restrictionType">The <see cref="NavigationRestrictionsType"/>.</param>
        /// <param name="restrictionProperty">The <see cref="NavigationPropertyRestriction"/>.</param>
        /// <returns>true, if navigability is allowed, otherwise false.</returns>
        internal static bool NavigationRestrictionsAllowsNavigability(
            NavigationRestrictionsType restrictionType,
            NavigationPropertyRestriction restrictionProperty)
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
        /// <param name="prefix">Optional: Identifier indicating whether it is a collection-valued non-indexed or single-valued navigation property.</param>
        /// <returns>The operation id generated from the given navigation property path.</returns>
        internal static string GenerateNavigationPropertyPathOperationId(ODataPath path, string prefix = null)
        {
            IList<string> items = RetrieveNavigationPropertyPathsOperationIdSegments(path);

            if (!items.Any())
                return null;

            int lastItemIndex = items.Count - 1;
            
            if (!string.IsNullOrEmpty(prefix))
            {
                items[lastItemIndex] = prefix + Utils.UpperFirstChar(items.Last());
            }
            else
            {
                items[lastItemIndex] = Utils.UpperFirstChar(items.Last());
            }

            return string.Join(".", items);
        }

        /// <summary>
        /// Generates the operation id from a complex property path.
        /// </summary>
        /// <param name="path">The target <see cref="ODataPath"/>.</param>
        /// <param name="prefix">Optional: Identifier indicating whether it is a collection-valued or single-valued complex property.</param>
        /// <returns>The operation id generated from the given complex property path.</returns>
        internal static string GenerateComplexPropertyPathOperationId(ODataPath path, string prefix = null)
        {
            IList<string> items = RetrieveNavigationPropertyPathsOperationIdSegments(path);

            if (!items.Any())
                return null;

            ODataComplexPropertySegment lastSegment = path.Segments.Skip(1).OfType<ODataComplexPropertySegment>()?.Last();
            Utils.CheckArgumentNull(lastSegment, nameof(lastSegment));

            if (!string.IsNullOrEmpty(prefix))
            {
                items.Add(prefix + Utils.UpperFirstChar(lastSegment?.Identifier));
            }
            else
            {
                items.Add(Utils.UpperFirstChar(lastSegment?.Identifier));
            }

            return string.Join(".", items);
        }

        /// <summary>
        /// Retrieves the segments of an operation id generated from a navigation property path.
        /// </summary>
        /// <param name="path">The target <see cref="ODataPath"/>.</param>
        /// <returns>The segments of an operation id generated from the given navigation property path.</returns>
        internal static IList<string> RetrieveNavigationPropertyPathsOperationIdSegments(ODataPath path)
        {
            Utils.CheckArgumentNull(path, nameof(path));

            IEdmNavigationSource navigationSource = (path.FirstSegment as ODataNavigationSourceSegment)?.NavigationSource;
            Utils.CheckArgumentNull(navigationSource, nameof(navigationSource));

            IList<string> items = new List<string>
            {
                navigationSource.Name
            };

            IEnumerable<ODataNavigationPropertySegment> navPropSegments = path.Segments.Skip(1).OfType<ODataNavigationPropertySegment>();
            Utils.CheckArgumentNull(navPropSegments, nameof(navPropSegments));

            foreach (var segment in navPropSegments)
            {
                if (segment == navPropSegments.Last())
                {
                    items.Add(segment.NavigationProperty.Name);
                    break;
                }
                else
                {
                    items.Add(segment.NavigationProperty.Name);
                }
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

            IEdmNavigationSource navigationSource = (path.FirstSegment as ODataNavigationSourceSegment)?.NavigationSource;
            
            IList<string> items = new List<string>
            {
                navigationSource.Name
            };

            IEdmNavigationProperty navigationProperty = path.OfType<ODataNavigationPropertySegment>()?.Last()?.NavigationProperty;
            Utils.CheckArgumentNull(navigationProperty, nameof(navigationProperty));

            foreach (var segment in path.Segments.Skip(1).OfType<ODataNavigationPropertySegment>())
            {
                if (segment.NavigationProperty == navigationProperty)
                {
                    items.Add(navigationProperty.ToEntityType().Name);
                    break;
                }
                else
                {
                    if (items.Count >= context.Settings.TagDepth - 1)
                    {
                        items.Add(segment.NavigationProperty.ToEntityType().Name);
                        break;
                    }
                    else
                    {
                        items.Add(segment.NavigationProperty.Name);
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

            // Get the segment before the last complex type segment
            ODataComplexPropertySegment complexSegment = path.Segments.OfType<ODataComplexPropertySegment>()?.Last();
            Utils.CheckArgumentNull(complexSegment, nameof(complexSegment));

            int complexSegmentIndex = path.Segments.IndexOf(complexSegment);
            ODataSegment preComplexSegment = path.Segments.ElementAt(complexSegmentIndex - 1);
            string tagName = null;

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
                var thirdLastSegment = path.Segments.ElementAt(complexSegmentIndex - 2);
                if (thirdLastSegment is ODataNavigationPropertySegment)
                {
                    tagName = GenerateNavigationPropertyPathTagName(path, context);
                }
                else if (thirdLastSegment is ODataNavigationSourceSegment sourceSegment1)
                {
                    tagName = $"{sourceSegment1.NavigationSource.Name}";
                }
            }

            List<string> tagNameItems = tagName?.Split('.').ToList();
            
            if (tagNameItems.Count < context.Settings.TagDepth)
            {
                tagNameItems.Add(complexSegment.ComplexType.Name);
            }

            return string.Join(".", tagNameItems);
        }

        /// <summary>
        /// Generates the operation id prefix from an OData type cast path.
        /// </summary>
        /// <param name="path">The target <see cref="ODataPath"/>.</param>
        /// <param name="includeListOrGetPrefix">Optional: Whether to include the List or Get prefix to the generated operation id.</param>
        /// <returns>The operation id prefix generated from the OData type cast path.</returns>
        internal static string GenerateODataTypeCastPathOperationIdPrefix(ODataPath path, bool includeListOrGetPrefix = true)
        {
            // Get the segment before the last OData type cast segment
            ODataTypeCastSegment typeCastSegment = path.Segments.OfType<ODataTypeCastSegment>()?.Last();
            Utils.CheckArgumentNull(typeCastSegment, nameof(typeCastSegment));

            int typeCastSegmentIndex = path.Segments.IndexOf(typeCastSegment);
            
            // The segment 1 place before the last OData type cast segment
            ODataSegment secondLastSegment = path.Segments.ElementAt(typeCastSegmentIndex - 1);

            bool isIndexedCollValuedNavProp = false;
            if (secondLastSegment is ODataKeySegment)
            {
                // The segment 2 places before the last OData type cast segment
                ODataSegment thirdLastSegment = path.Segments.ElementAt(typeCastSegmentIndex - 2);
                if (thirdLastSegment is ODataNavigationPropertySegment)
                {
                    isIndexedCollValuedNavProp = true;
                }
            }

            ODataNavigationSourceSegment navigationSourceSegment = path.FirstSegment as ODataNavigationSourceSegment;
            IEdmSingleton singleton = navigationSourceSegment?.NavigationSource as IEdmSingleton;
            IEdmEntitySet entitySet = navigationSourceSegment?.NavigationSource as IEdmEntitySet;

            string operationId = null;
            if (secondLastSegment is ODataComplexPropertySegment complexSegment)
            {
                string listOrGet = includeListOrGetPrefix ? (complexSegment.Property.Type.IsCollection() ? "List" : "Get") : null;
                operationId = GenerateComplexPropertyPathOperationId(path, listOrGet);
            }
            else if (secondLastSegment as ODataNavigationPropertySegment is not null || isIndexedCollValuedNavProp)
            {
                string listOrGet = null;
                if (includeListOrGetPrefix)
                {
                    if (!isIndexedCollValuedNavProp &&
                    (secondLastSegment as ODataNavigationPropertySegment)?.NavigationProperty.TargetMultiplicity() == EdmMultiplicity.Many)
                    {
                        listOrGet = "List";
                    }
                    else
                    {
                        listOrGet = "Get";
                    }
                }
                else
                {
                    listOrGet = null;
                }

                operationId = GenerateNavigationPropertyPathOperationId(path, listOrGet);
            }
            else if (secondLastSegment is ODataKeySegment keySegment && !isIndexedCollValuedNavProp)
            {
                string entityTypeName = keySegment.EntityType.Name;
                string getPrefix = includeListOrGetPrefix ? "Get" : null;
                string operationName = $"{getPrefix}{Utils.UpperFirstChar(entityTypeName)}";
                if (keySegment.IsAlternateKey)
                {
                    string alternateKeyName = string.Join("", keySegment.Identifier.Split(',').Select(static x => Utils.UpperFirstChar(x)));
                    operationName = $"{operationName}By{alternateKeyName}";
                }
                operationId = (entitySet != null) ? entitySet.Name : singleton.Name;
                operationId += $".{entityTypeName}.{operationName}";
            }
            else if (secondLastSegment is ODataNavigationSourceSegment)
            {
                operationId = (entitySet != null)
                    ? entitySet.Name + "." + entitySet.EntityType().Name + $".{(includeListOrGetPrefix ? "List" : null)}" + Utils.UpperFirstChar(entitySet.EntityType().Name)
                    : singleton.Name + "." + singleton.EntityType().Name + $".{(includeListOrGetPrefix ? "Get" : null)}" + Utils.UpperFirstChar(singleton.EntityType().Name);
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
        internal static string StripOrAliasNamespacePrefix(IEdmSchemaElement element, OpenApiConvertSettings settings, IEdmModel model = null)
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
        /// <returns>true if the operation is allowed, otherwise false.</returns>
        internal static bool IsOperationAllowed(IEdmModel model, IEdmOperation edmOperation, IEdmVocabularyAnnotatable annotatable)
        {
            Utils.CheckArgumentNull(model, nameof(model));
            Utils.CheckArgumentNull(edmOperation, nameof(edmOperation));
            Utils.CheckArgumentNull(annotatable, nameof(annotatable));

            var requiresExplicitBinding = model.FindVocabularyAnnotations(edmOperation).FirstOrDefault(x => x.Term.Name == CapabilitiesConstants.RequiresExplicitBindingName);

            if (requiresExplicitBinding == null)
            {
                return true;
            }
            
            var boundOperations = model.GetCollection(annotatable, CapabilitiesConstants.ExplicitOperationBindings)?.ToList();
            return boundOperations != null && boundOperations.Contains(edmOperation.FullName());
        }
    }
}