// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.Vocabulary.Capabilities
{
    /// <summary>
    /// Complex Type: Org.OData.Capabilities.V1.OperationRestrictionsType
    /// </summary>
    [Term("Org.OData.Capabilities.V1.OperationRestrictions")]
    internal class OperationRestrictionsType : IRecord
    {
        /// <summary>
        /// Gets the Bound action or function can be invoked on a collection-valued binding parameter path with a '/$filter(...)' segment.
        /// </summary>
        public bool? FilterSegmentSupported { get; private set; }

        /// <summary>
        /// Gets the List of required scopes to invoke an action or function.
        /// </summary>
        public IList<PermissionType> Permissions { get; private set; }

        /// <summary>
        /// Gets the Supported or required custom headers.
        /// </summary>
        public IList<CustomParameter> CustomHeaders { get; private set; }

        /// <summary>
        /// Gets the Supported or required custom query options.
        /// </summary>
        public IList<CustomParameter> CustomQueryOptions { get; private set; }

        /// <summary>
        /// Init the <see cref="OperationRestrictionsType"/>.
        /// </summary>
        /// <param name="record">The input record.</param>
        public void Initialize(IEdmRecordExpression record)
        {
            Utils.CheckArgumentNull(record, nameof(record));

            // FilterSegmentSupported
            FilterSegmentSupported = record.GetBoolean("FilterSegmentSupported");

            // Permissions
            Permissions = record.GetCollection<PermissionType>("Permissions");

            // CustomHeaders
            CustomHeaders = record.GetCollection<CustomParameter>("CustomHeaders");

            // CustomQueryOptions
            CustomQueryOptions = record.GetCollection<CustomParameter>("CustomQueryOptions");
        }

        /// <summary>
        /// Merges properties of the specified <see cref="OperationRestrictionsType"/> object into this instance if they are null.
        /// </summary>
        /// <param name="source">The <see cref="OperationRestrictionsType"/> object containing properties to merge.</param>
        public void MergePropertiesIfNull(OperationRestrictionsType source)
        {
            if (source == null)
                return;

            FilterSegmentSupported ??= source.FilterSegmentSupported;

            Permissions ??= source.Permissions;

            CustomHeaders ??= source.CustomHeaders;

            CustomQueryOptions ??= source.CustomQueryOptions;
        }
    }
}
