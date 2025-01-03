// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.OData.Edm;
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
        /// <param name="document">The Open API document.</param>
        /// <returns>The created <see cref="OpenApiExample"/> dictionary.</returns>
        public static IDictionary<string, OpenApiExample> CreateExamples(this ODataContext context, OpenApiDocument document)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(document, nameof(document));

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
                    case EdmSchemaElementKind.TypeDefinition when element is IEdmType reference: // Type definition
                        {
                            OpenApiExample example = context.CreateExample(reference, document);
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

        private static OpenApiExample CreateExample(this ODataContext context, IEdmType edmType, OpenApiDocument document)
        {
            Debug.Assert(context != null);
            Debug.Assert(edmType != null);

            return edmType.TypeKind switch
            {
                // complex type
                EdmTypeKind.Complex or EdmTypeKind.Entity when edmType is IEdmStructuredType edmStructuredType => new()
                {
                    Value = OpenApiSchemaGenerator.CreateStructuredTypePropertiesExample(context, edmStructuredType, document),
                },
                _ => null,
            };
        }
    }
}
