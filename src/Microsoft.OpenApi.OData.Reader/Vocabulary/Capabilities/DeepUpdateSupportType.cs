// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.Vocabulary.Capabilities
{
    /// <summary>
    /// Complex Type: Org.OData.Capabilities.V1.DeepUpdateSupportType
    /// </summary>
    [Term("Org.OData.Capabilities.V1.DeepUpdateSupport")]
    internal class DeepUpdateSupportType : IRecord
    {
        /// <summary>
        /// Gets Annotation target supports deep updates
        /// </summary>
        public bool? Supported { get; private set; }

        /// <summary>
        /// Gets Annotation target supports accepting and returning nested entities annotated with the `Core.ContentID` instance annotation.
        /// </summary>
        public bool? ContentIDSupported { get; private set; }

        /// <summary>
        /// Init the <see cref="DeepUpdateSupportType"/>.
        /// </summary>
        /// <param name="record">The input record.</param>
        public void Initialize(IEdmRecordExpression record)
        {
            Utils.CheckArgumentNull(record, nameof(record));

            // Supported
            Supported = record.GetBoolean("Supported");

            // NonInsertableNavigationProperties
            ContentIDSupported = record.GetBoolean("ContentIDSupported");
        }
    }
}
