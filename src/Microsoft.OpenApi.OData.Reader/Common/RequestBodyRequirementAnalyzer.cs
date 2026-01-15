// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Vocabulary.Core;

namespace Microsoft.OpenApi.OData.Common
{
    /// <summary>
    /// Utility class for analyzing EDM types to determine if request bodies should be required.
    /// </summary>
    internal static class RequestBodyRequirementAnalyzer
    {
        /// <summary>
        /// Determines if a request body should be required for an OData action.
        /// </summary>
        /// <param name="action">The EDM action.</param>
        /// <returns>True if the request body should be required, false otherwise.</returns>
        public static bool ShouldRequestBodyBeRequired(IEdmAction action)
        {
            if (action == null)
            {
                return true; // Safe default
            }

            // Get non-binding parameters
            var parameters = action.IsBound
                ? action.Parameters.Skip(1)
                : action.Parameters;

            // If no parameters, body is already null (existing behavior handles this)
            if (!parameters.Any())
            {
                return true; // Won't matter since body will be null
            }

            // Check if all parameters are nullable or optional
            return !parameters.All(p => p.Type.IsNullable || p is IEdmOptionalParameter);
        }

        /// <summary>
        /// Determines if a request body should be required for an entity or complex type.
        /// </summary>
        /// <param name="structuredType">The EDM structured type.</param>
        /// <param name="isUpdateOperation">Whether this is an update operation (excludes key properties).</param>
        /// <param name="model">The EDM model for additional context.</param>
        /// <returns>True if the request body should be required, false otherwise.</returns>
        public static bool ShouldRequestBodyBeRequired(
            IEdmStructuredType structuredType,
            bool isUpdateOperation,
            IEdmModel? model = null)
        {
            if (structuredType == null)
            {
                return true; // Safe default
            }

            return !AreAllPropertiesOptional(structuredType, isUpdateOperation, model);
        }

        /// <summary>
        /// Checks if all properties in a structured type are optional.
        /// </summary>
        /// <param name="structuredType">The EDM structured type.</param>
        /// <param name="excludeKeyProperties">Whether to exclude key properties from analysis (for update operations).</param>
        /// <param name="model">The EDM model for additional context.</param>
        /// <returns>True if all properties are optional, false if any are required.</returns>
        private static bool AreAllPropertiesOptional(
            IEdmStructuredType structuredType,
            bool excludeKeyProperties,
            IEdmModel? model = null)
        {
            if (structuredType == null)
            {
                return false;
            }

            // Collect all properties including inherited ones
            var allProperties = new List<IEdmProperty>();

            // Get properties from current type and all base types
            IEdmStructuredType currentType = structuredType;
            while (currentType != null)
            {
                allProperties.AddRange(currentType.DeclaredStructuralProperties());
                allProperties.AddRange(currentType.DeclaredNavigationProperties());
                currentType = currentType.BaseType;
            }

            // If no properties, consider optional (empty body)
            if (!allProperties.Any())
            {
                return true;
            }

            // Get key property names if we need to exclude them
            HashSet<string>? keyNames = null;
            if (excludeKeyProperties && structuredType is IEdmEntityType entityType)
            {
                keyNames = new HashSet<string>(entityType.Key().Select(k => k.Name));
            }

            // Check if ALL remaining properties are optional
            foreach (var property in allProperties)
            {
                // Skip key properties if requested
                if (keyNames != null && keyNames.Contains(property.Name))
                {
                    continue;
                }

                // Skip computed properties (read-only)
                if (model != null && property is IEdmStructuralProperty &&
                    (model.GetBoolean(property, CoreConstants.Computed) ?? false))
                {
                    continue;
                }

                // If this property is required, the body must be required
                if (!IsPropertyOptional(property, model))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if an individual property is optional.
        /// </summary>
        /// <param name="property">The EDM property.</param>
        /// <param name="model">The EDM model for additional context.</param>
        /// <returns>True if the property is optional, false if required.</returns>
        private static bool IsPropertyOptional(IEdmProperty property, IEdmModel? model)
        {
            if (property == null)
            {
                return false;
            }

            // Structural properties (primitive, enum, complex)
            if (property is IEdmStructuralProperty structuralProp)
            {
                // Has default value = optional
                if (!string.IsNullOrEmpty(structuralProp.DefaultValueString))
                {
                    return true;
                }

                // Type is nullable = optional
                if (structuralProp.Type.IsNullable)
                {
                    return true;
                }

                // Otherwise required
                return false;
            }

            // Navigation properties
            if (property is IEdmNavigationProperty navProp)
            {
                // Navigation properties are optional if nullable
                return navProp.Type.IsNullable;
            }

            // Unknown property type, treat as required (safe default)
            return false;
        }
    }
}
