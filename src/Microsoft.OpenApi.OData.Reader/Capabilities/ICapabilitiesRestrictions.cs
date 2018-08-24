// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;

namespace Microsoft.OpenApi.OData.Capabilities
{
    internal interface ICapablitiesRestrictions
    {
        CapabilitesTermKind Kind { get; }

        bool Load(IEdmModel model, IEdmVocabularyAnnotatable target);
    }
}
