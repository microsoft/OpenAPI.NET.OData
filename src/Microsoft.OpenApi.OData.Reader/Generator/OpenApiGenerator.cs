// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;

namespace Microsoft.OpenApi.OData.Generator
{
    /// <summary>
    /// Base class for Edm to Open Api generator.
    /// </summary>
    internal abstract class OpenApiGenerator
    {
        /// <summary>
        /// The Edm model.
        /// </summary>
        public IEdmModel Model { get; }

        /// <summary>
        /// The Open Api convert setting.
        /// </summary>
        public OpenApiConvertSettings Settings { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenApiGenerator" /> class.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="settings">The Open Api convert settings.</param>
        public OpenApiGenerator(IEdmModel model, OpenApiConvertSettings settings)
        {
            Model = model ?? throw Error.ArgumentNull(nameof(model));
            Settings = settings ?? throw Error.ArgumentNull(nameof(settings));
        }
    }
}
