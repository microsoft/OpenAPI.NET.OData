// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.OpenApi.OData.Capabilities
{
    /// <summary>
    /// Complex type: Org.OData.Capabilities.V1.SelectSupportType
    /// </summary>
    internal class SelectSupportType
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
    }
}
