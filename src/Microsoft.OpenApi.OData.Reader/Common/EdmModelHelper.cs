// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
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
        /// <returns></returns>
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
        /// Generates the operation id for a navigation property path.
        /// </summary>
        /// <param name="path">The target <see cref="ODataPath"/>.</param>
        /// <param name="navigationSource">The <see cref="IEdmNavigationSource"/> of the target path.</param>
        /// <param name="prefix">Identifier indicating whether it is a collection-valued non-indexed navigation property.</param>
        /// <returns>The operation id name.</returns>
        internal static string GenerateNavigationPropertyPathOperationId(ODataPath path, IEdmNavigationSource navigationSource, string prefix = null)
        {
            if (path == null || navigationSource == null)
                return null;

            IList<string> items = new List<string>
            {
                navigationSource.Name
            };

            var lastpath = path.Segments.Last(c => c is ODataNavigationPropertySegment);
            foreach (var segment in path.Segments.Skip(1).OfType<ODataNavigationPropertySegment>())
            {
                if (segment == lastpath)
                {
                    if (prefix != null)
                    {
                        items.Add(prefix + Utils.UpperFirstChar(segment.NavigationProperty.Name));
                    }
                    else
                    {
                        items.Add(Utils.UpperFirstChar(segment.NavigationProperty.Name));
                    }

                    break;
                }
                else
                {
                    items.Add(segment.NavigationProperty.Name);
                }
            }

            return string.Join(".", items);
        }

        /// <summary>
        /// Generates the tag for a navigation property path.
        /// </summary>
        /// <param name="path">The target <see cref="ODataPath"/>.</param>
        /// <param name="navigationSource">The <see cref="IEdmNavigationSource"/> of the target path.</param>
        /// <param name="navigationProperty">The target <see cref="IEdmNavigationProperty"/>.</param>
        /// <param name="context">The <see cref="ODataContext"/>.</param>
        /// <returns>The tag name.</returns>
        internal static string GenerateNavigationPropertyPathTag(ODataPath path, IEdmNavigationSource navigationSource, IEdmNavigationProperty navigationProperty, ODataContext context)
        {
            if (path == null || navigationSource == null || navigationProperty == null)
                return null;

            IList<string> items = new List<string>
            {
                navigationSource.Name
            };

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
    }
}