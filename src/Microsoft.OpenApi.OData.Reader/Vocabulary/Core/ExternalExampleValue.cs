// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.Vocabulary.Core
{
    /// <summary>
    /// Complex type: Org.OData.Core.V1.ExternalExampleValue.
    /// </summary>
    internal class ExternalExampleValue : ExampleValue
    {
        /// <summary>
        /// Gets the Url reference to the value in its literal format
        /// </summary>
        public string ExternalValue { get; set; }

        /// <summary>
        /// Init the <see cref="ExternalExampleValue"/>
        /// </summary>
        /// <param name="record">The input record.</param>
        public override void Initialize(IEdmRecordExpression record)
        {
            // Load ExampleValue
            base.Initialize(record);

            // ExternalValue
            ExternalValue = record.GetString("ExternalValue");
        }
    }
}
