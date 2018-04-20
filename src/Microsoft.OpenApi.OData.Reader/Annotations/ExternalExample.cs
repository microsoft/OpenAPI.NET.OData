// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Edm;

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

        /// <summary>
        /// Init the <see cref="ExternalExample"/>.
        /// </summary>
        /// <param name="record">The record.</param>
        public override void Init(IEdmRecordExpression record)
        {
            base.Init(record);

            // ExternalValue
            ExternalValue = record.GetString("ExternalValue");
        }
    }
}
