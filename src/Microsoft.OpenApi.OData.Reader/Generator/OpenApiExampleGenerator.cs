// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        public static void AddExamplesToDocument(this ODataContext context, OpenApiDocument document)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(document, nameof(document));


            // Each entity type, complex type, enumeration type, and type definition directly
            // or indirectly used in the paths field is represented as a name / value pair of the schemas map.
            // Ideally this would be driven off the types used in the paths, but in practice, it is simply
            // all of the types present in the model.
            var elements = context.Model.GetAllElements()
                                            .Where(static x => x.SchemaElementKind is EdmSchemaElementKind.TypeDefinition)
                                            .OfType<IEdmType>();

            foreach (var element in elements)
            {
                if (context.CreateExample(element) is OpenApiExample example)
                {
                    document.AddComponent(element.FullTypeName(), example);
                }
            }
        }

        private static OpenApiExample CreateExample(this ODataContext context, IEdmType edmType)
        {
            Debug.Assert(context != null);
            Debug.Assert(edmType != null);

            return edmType.TypeKind switch
            {
                // complex type
                EdmTypeKind.Complex or EdmTypeKind.Entity when edmType is IEdmStructuredType edmStructuredType => new()
                {
                    Value = OpenApiSchemaGenerator.CreateStructuredTypePropertiesExample(context, edmStructuredType),
                },
                _ => null,
            };
        }
    }
}
