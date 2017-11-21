// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;

namespace Microsoft.OpenApi.OData.Generator
{
    /// <summary>
    /// Class to create <see cref="OpenApiSchema"/> by <see cref="IEdmModel"/>.
    /// </summary>
    internal class OpenApiSchemaGenerator : OpenApiGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenApiSchemaGenerator" /> class.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="settings">The Open Api convert settings.</param>
        public OpenApiSchemaGenerator(IEdmModel model, OpenApiConvertSettings settings)
            : base(model, settings)
        {
        }

        /// <summary>
        /// Create the dictionary of <see cref="OpenApiSchema"/> object.
        /// The name of each pair is the namespace-qualified name of the type. It uses the namespace instead of the alias.
        /// The value of each pair is a <see cref="OpenApiSchema"/>.
        /// </summary>
        /// <returns>The string/schema dictionary.</returns>
        public IDictionary<string, OpenApiSchema> CreateSchemas()
        {
            IDictionary<string, OpenApiSchema> schemas = new Dictionary<string, OpenApiSchema>();

            // Each entity type, complex type, enumeration type, and type definition directly
            // or indirectly used in the paths field is represented as a name / value pair of the schemas map.
            foreach (var element in Model.SchemaElements)
            {
                switch (element.SchemaElementKind)
                {
                    case EdmSchemaElementKind.TypeDefinition: // Type definition
                        {
                            IEdmType reference = (IEdmType)element;
                            schemas.Add(reference.FullTypeName(), Model.CreateEdmTypeSchema(reference));
                        }
                        break;
                }
            }

            schemas.AppendODataErrors();

            return schemas;
        }
    }
}
