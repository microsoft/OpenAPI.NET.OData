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
    /// Complex Type: Org.OData.Capabilities.V1.ReadRestrictionsBase
    /// </summary>
    internal abstract class ReadRestrictionsBase : IRecord
    {
        /// <summary>
        /// Get the Entities can be retrieved.
        /// </summary>
        public bool? Readable { get; private set; }

        /// <summary>
        /// Gets the List of required scopes to invoke an action or function
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
        /// Gets A brief description of the request.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Gets A lengthy description of the request.
        /// </summary>
        public string LongDescription { get; private set; }

        /// <summary>
        /// Test the target supports read.
        /// </summary>
        /// <returns>True/false.</returns>
        public bool IsReadable => Readable == null || Readable.Value;

        /// <summary>
        /// Init the <see cref="ReadRestrictionsBase"/>.
        /// </summary>
        /// <param name="record">The input record.</param>
        public virtual void Initialize(IEdmRecordExpression record)
        {
            Utils.CheckArgumentNull(record, nameof(record));

            // Readable
            Readable = record.GetBoolean("Readable");

            // Permissions
            Permissions = record.GetCollection<PermissionType>("Permissions");

            // CustomHeaders
            CustomHeaders = record.GetCollection<CustomParameter>("CustomHeaders");

            // CustomQueryOptions
            CustomQueryOptions = record.GetCollection<CustomParameter>("CustomQueryOptions");

            // Description
            Description = record.GetString("Description");

            // LongDescription
            LongDescription = record.GetString("LongDescription");
        }


        /// <summary>
        /// Merges properties of the specified <see cref="ReadRestrictionsBase"/> object into this instance if they are null.
        /// </summary>
        /// <param name="source">The <see cref="ReadRestrictionsBase"/> object containing properties to merge.</param>
        public void MergePropertiesIfNull(ReadRestrictionsBase source)
        {
            if (source == null)
                return;

            Readable ??= source.Readable;

            Permissions ??= source.Permissions;

            CustomHeaders ??= source.CustomHeaders;

            CustomQueryOptions ??= source.CustomQueryOptions;

            Description ??= source.Description;

            LongDescription ??= source.LongDescription;
        }
    }

    /// <summary>
    /// Complex Type: Org.OData.Capabilities.V1.ReadByKeyRestrictionsType
    /// Restrictions for retrieving an entity by key
    /// </summary>
    internal class ReadByKeyRestrictions : ReadRestrictionsBase
    {
        // nothing here
    }

    /// <summary>
    /// Complex Type: Org.OData.Capabilities.V1.ReadRestrictionsType
    /// </summary>
    [Term("Org.OData.Capabilities.V1.ReadRestrictions")]
    internal class ReadRestrictionsType : ReadRestrictionsBase
    {
        /// <summary>
        /// Gets the Restrictions for retrieving an entity by key
        /// </summary>
        public ReadByKeyRestrictions ReadByKeyRestrictions { get; set; }

        /// <summary>
        /// Init the <see cref="ReadRestrictionsType"/>.
        /// </summary>
        /// <param name="record">The input record.</param>
        public override void Initialize(IEdmRecordExpression record)
        {
            // Load base
            base.Initialize(record);

            // ReadByKeyRestrictions
            ReadByKeyRestrictions = record.GetRecord<ReadByKeyRestrictions>("ReadByKeyRestrictions");
        }

        /// <summary>
        /// Merges properties of the specified <see cref="ReadRestrictionsType"/> object into this instance if they are null.
        /// </summary>
        /// <param name="source">The <see cref="ReadRestrictionsType"/> object containing properties to merge.</param>
        public void MergePropertiesIfNull(ReadRestrictionsType source)
        {
            base.MergePropertiesIfNull(source);

            if (source == null)
                return;

            ReadByKeyRestrictions ??= source.ReadByKeyRestrictions;
        }
    }
}
