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
    /// Complex type: Org.OData.Capabilities.V1.SelectSupportType
    /// </summary>
    [Term("Org.OData.Capabilities.V1.SelectSupport")]
    internal class SelectSupportType : IRecord
    {
        /// <summary>
        /// Gets the Supports $select.
        /// </summary>
        public bool? Supported { get; private set; }

        /// <summary>
        /// Gets the $expand within $select is supported.
        /// </summary>
        public bool? Expandable { get; private set; }

        /// <summary>
        /// Gets the $filter within $select is supported.
        /// </summary>
        public bool? Filterable { get; private set; }

        /// <summary>
        /// Gets the $search within $select is supported.
        /// </summary>
        public bool? Searchable { get; private set; }

        /// <summary>
        /// Gets the $top within $select is supported.
        /// </summary>
        public bool? TopSupported { get; private set; }

        /// <summary>
        /// Gets the $skip within $select is supported.
        /// </summary>
        public bool? SkipSupported { get; private set; }

        /// <summary>
        /// Gets the $compute within $select is supported.
        /// </summary>
        public bool? ComputeSupported { get; private set; }

        /// <summary>
        /// Gets the $count within $select is supported.
        /// </summary>
        public bool? Countable { get; private set; }

        /// <summary>
        /// Gets the orderby within $select is supported.
        /// </summary>
        public bool? Sortable { get; private set; }

        /// <summary>
        /// Init the <see cref="SelectSupportType"/>.
        /// </summary>
        /// <param name="record">The input record.</param>
        public void Initialize(IEdmRecordExpression record)
        {
            Utils.CheckArgumentNull(record, nameof(record));

            // Supported
            Supported = record.GetBoolean("Supported");

            // Expandable
            Expandable = record.GetBoolean("Expandable");

            // Filterable
            Filterable = record.GetBoolean("Filterable");

            // Searchable
            Searchable = record.GetBoolean("Searchable");

            // TopSupported
            TopSupported = record.GetBoolean("TopSupported");

            // SkipSupported
            SkipSupported = record.GetBoolean("SkipSupported");

            // ComputeSupported
            ComputeSupported = record.GetBoolean("ComputeSupported");

            // Countable
            Countable = record.GetBoolean("Countable");

            // Sortable
            Sortable = record.GetBoolean("Sortable");
        }
    }
}
