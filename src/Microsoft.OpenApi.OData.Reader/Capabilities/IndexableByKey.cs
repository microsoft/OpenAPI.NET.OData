// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;

namespace Microsoft.OpenApi.OData.Capabilities
{
    /// <summary>
    /// Org.OData.Capabilities.V1.IndexableByKey
    /// </summary>
    internal class IndexableByKey : SupportedRestrictions
    {
        /// <summary>
        /// The Term type name.
        /// </summary>
        public override string QualifiedName => CapabilitiesConstants.IndexableByKey;

        /// <summary>
        /// Initializes a new instance of <see cref="IndexableByKey"/> class.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="target">The Edm annotation target.</param>
        public IndexableByKey(IEdmModel model, IEdmVocabularyAnnotatable target)
            : base(model, target)
        {
        }
    }
}
