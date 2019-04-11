// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData.Edm.Vocabularies;

namespace Microsoft.OpenApi.OData.Capabilities
{
    /// <summary>
    /// Term: Org.OData.Capabilities.V1.CustomQueryOptions
    /// </summary>
    internal class CustomQueryOptions : CapabilitiesRestrictions
    {
        /// <summary>
        /// The Term type kind.
        /// </summary>
        public override CapabilitesTermKind Kind => CapabilitesTermKind.CustomQueryOptions;

        /// <summary>
        /// Collection(Capabilities.CustomParameter)
        /// </summary>
        public IList<CustomParameter> Parameters { get; private set; }

        protected override bool Initialize(IEdmVocabularyAnnotation annotation)
        {
            throw new System.NotImplementedException();
        }
    }
}
