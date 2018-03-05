// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Common;

namespace Microsoft.OpenApi.OData.Annotations
{
    /// <summary>
    /// Complex type Org.OData.Core.V1.ExternalExample
    /// </summary>
    internal class ExternalExample : Example
    {
        /// <summary>
        /// ExternalValue
        /// </summary>
        public string ExternalValue { get; set; }

        public override void Init(IEdmRecordExpression record)
        {
            base.Init(record);

            // ExternalValue
            ExternalValue = record.GetString("ExternalValue");
        }
    }
}
