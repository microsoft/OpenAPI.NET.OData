//---------------------------------------------------------------------
// <copyright file="EdmOpenApiGenerator.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using Microsoft.OData.Edm;

namespace Microsoft.OData.OpenAPI
{
    /// <summary>
    /// Base class for Edm to Open Api generator.
    /// </summary>
    internal abstract class EdmOpenApiGenerator
    {
        /// <summary>
        /// The Edm model.
        /// </summary>
        protected IEdmModel Model { get; }

        /// <summary>
        /// The Open Api writer setting.
        /// </summary>
        public OpenApiWriterSettings Settings { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmOpenApiGenerator" /> class.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="settings">The Open Api writer settings.</param>
        protected EdmOpenApiGenerator(IEdmModel model, OpenApiWriterSettings settings)
        {
            Model = model ?? throw Error.ArgumentNull(nameof(model));
            Settings = settings ?? throw Error.ArgumentNull(nameof(settings));
        }
    }
}
