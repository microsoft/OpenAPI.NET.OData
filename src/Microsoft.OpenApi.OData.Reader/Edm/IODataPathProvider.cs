// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using System.Collections.Generic;

namespace Microsoft.OpenApi.OData.Edm
{
    /// <summary>
    /// The interface for <see cref="ODataPath"/> provider.
    /// </summary>
    public interface IODataPathProvider
    {
        /// <summary>
        /// Can filter the <see cref="IEdmElement"/> or not.
        /// </summary>
        /// <param name="element">The Edm element.</param>
        /// <returns>True/false.</returns>
        bool CanFilter(IEdmElement element);

        /// <summary>
        /// Generate the list of <see cref="ODataPath"/> based on the given <see cref="IEdmModel"/>.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="settings">The conversion settings.</param>
        /// <returns>The collection of built <see cref="ODataPath"/>.</returns>
        IEnumerable<ODataPath> GetPaths(IEdmModel model, OpenApiConvertSettings settings);
    }
}
