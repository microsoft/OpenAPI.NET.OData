// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.Vocabulary.Core
{
    /// <summary>
    /// Complex type: Org.OData.Core.V1.ExampleValue.
    /// </summary>
    internal class ExampleValue : IRecord
    {
        /// <summary>
        /// Gets the description of the example value.
        /// </summary>
        public string? Description { get; private set; }

        /// <summary>
        /// Init the <see cref="ExampleValue"/>
        /// </summary>
        /// <param name="record">The input record.</param>
        public virtual void Initialize(IEdmRecordExpression record)
        {
            Utils.CheckArgumentNull(record, nameof(record));

            // Description
            Description = record.GetString("Description");
        }
    }
}
