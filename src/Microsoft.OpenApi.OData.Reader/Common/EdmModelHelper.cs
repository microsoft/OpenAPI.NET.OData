// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
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
            if(structuredType is not IEdmSchemaElement schemaElement) throw new ArgumentException("The type is not a schema element.", nameof(structuredType));

            IEnumerable<IEdmSchemaElement> derivedTypes = edmModel.FindDirectlyDerivedTypes(structuredType).OfType<IEdmSchemaElement>();

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
            if (restrictionProperty != null && restrictionProperty.Navigability != null && restrictionProperty.Navigability.Value == NavigationType.None)
            {
                return false;
            }

            if (restrictionProperty?.Navigability == null)
            {
                // if the individual has no navigability setting, use the global navigability setting
                if (restrictionType != null && !restrictionType.IsNavigable)
                {
                    // Default navigability for all navigation properties of the annotation target.
                    // Individual navigation properties can override this value via `RestrictedProperties/Navigability`.
                    return false;
                }
            }

            return true;
        }
    }
}