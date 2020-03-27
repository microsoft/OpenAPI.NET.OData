using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.Common
{
    public static class Helpers
    {
        /// <summary>
        /// Adds the derived types references together with their base type reference in the OneOf property of an OpenAPI schema.
        /// </summary>
        /// <returns>The OpenAPI schema with the list of derived types references and their base type references set in the OneOf property.</returns>
        internal static OpenApiSchema GetDerivedTypesReferenceSchema(IEdmEntityType entityType, ODataContext context)
        {
            IEnumerable<IEdmEntityType> derivedTypes = context.Model.FindDirectlyDerivedTypes(entityType).OfType<IEdmEntityType>();

            if (!derivedTypes.Any())
            {
                return null;
            }

            OpenApiSchema schema = new OpenApiSchema
            {
                OneOf = new List<OpenApiSchema>()
            };

            OpenApiSchema baseTypeSchema = new OpenApiSchema
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.Schema,
                    Id = entityType.FullName()
                }
            };
            schema.OneOf.Add(baseTypeSchema);

            foreach (IEdmEntityType derivedType in derivedTypes)
            {
                OpenApiSchema derivedTypeSchema = new OpenApiSchema
                {
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
    }
}
