//---------------------------------------------------------------------
// <copyright file="OpenApiReference.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

namespace Microsoft.OData.OpenAPI
{
    /// <summary>
    /// Reference Object.
    /// </summary>
    internal class OpenApiReference : IOpenApiElement, IOpenApiWritable
    {
        /// <summary>
        /// REQUIRED. The reference string.
        /// </summary>
        public string Ref { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenApiReference"/> class.
        /// </summary>
        /// <param name="ref"></param>
        public OpenApiReference(string @ref)
        {
            if (string.IsNullOrWhiteSpace(@ref))
            {
                throw Error.ArgumentNullOrEmpty(nameof(@ref));
            }

            Ref = @ref;
        }

        /// <summary>
        /// Write Open API reference object to the given writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public virtual void Write(IOpenApiWriter writer)
        {
            if (writer == null)
            {
                throw Error.ArgumentNull("writer");
            }

            // { for JSON, empty for YAML
            writer.WriteStartObject();

            // $ref
            writer.WriteRequiredProperty(OpenApiConstants.OpenApiDocDollarRef, Ref);

            // } for JSON, empty for YAML
            writer.WriteEndObject();
        }
    }
}
