// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Common;

namespace Microsoft.OpenApi.OData.Annotations
{
    /// <summary>
    /// Complex type Org.OData.Core.V1.InlineExample
    /// </summary>
    internal class InlineExample : Example
    {
        /// <summary>
        /// InlineValue
        /// </summary>
        public string InlineValue { get; set; }

        /// <summary>
        /// Init the <see cref="InlineExample"/>.
        /// </summary>
        /// <param name="record">The record.</param>
        public override void Init(IEdmRecordExpression record)
        {
            base.Init(record);

            // InlineValue
            InlineValue = record.GetString("InlineValue");
        }
    }
}
