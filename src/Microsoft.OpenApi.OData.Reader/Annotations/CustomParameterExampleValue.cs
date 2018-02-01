// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Capabilities;

namespace Microsoft.OpenApi.OData.Annotations
{
    /// <summary>
    /// Org.Graph.Vocab.HttpRequests
    /// </summary>
    internal class CustomParameterExampleValue
    {
        public string Value { get; set; }

        public string Description { get; set; }

        public void Init(IEdmRecordExpression record)
        {
            Value = record.GetString("Value");
            Description = record.GetString("Description");
        }
    }
}
