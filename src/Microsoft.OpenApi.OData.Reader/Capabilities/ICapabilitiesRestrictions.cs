// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;

namespace Microsoft.OpenApi.OData.Capabilities
{
    /// <summary>
    /// Interface for the capabilities restrictions
    /// </summary>
    internal interface ICapablitiesRestrictions
    {
        /// <summary>
        /// The Capablities Kind.
        /// </summary>
        CapabilitesTermKind Kind { get; }

        /// <summary>
        /// Load the annotation value.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="target">The target.</param>
        /// <returns>True/False</returns>
        bool Load(IEdmModel model, IEdmVocabularyAnnotatable target);
    }
}
