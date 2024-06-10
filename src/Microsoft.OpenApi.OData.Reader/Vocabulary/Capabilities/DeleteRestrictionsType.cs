// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.Vocabulary.Capabilities
{
    /// <summary>
    /// Complex Type: Org.OData.Capabilities.V1.DeleteRestrictionsType
    /// </summary>
    [Term("Org.OData.Capabilities.V1.DeleteRestrictions")]
    internal class DeleteRestrictionsType : IRecord
    {
        /// <summary>
        /// Gets the Deletable value.
        /// </summary>
        public bool? Deletable { get; private set; }

        /// <summary>
        /// Gets the navigation properties which do not allow DeleteLink requests.
        /// </summary>
        public IList<string> NonDeletableNavigationProperties { get; private set; }

        /// <summary>
        /// Gets the maximum number of navigation properties that can be traversed.
        /// </summary>
        public int? MaxLevels { get; private set; }

        /// <summary>
        /// Gets the Members of collections can be updated via a PATCH request with a `/$filter(...)/$each` segment.
        /// </summary>
        public bool? FilterSegmentSupported { get; private set; }

        /// <summary>
        /// Gets the Members of collections can be updated via a PATCH request with a type-cast segment and a `/$each` segment.
        /// </summary>
        public bool? TypecastSegmentSupported { get; private set; }

        /// <summary>
        /// Gets the required scopes to perform the insert.
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
        /// Test the target supports delete.
        /// </summary>
        /// <returns>True/false.</returns>
        public bool IsDeletable => Deletable == null || Deletable.Value;

        /// <summary>
        /// Test the input navigation property do not allow DeleteLink requests.
        /// </summary>
        /// <param name="navigationPropertyPath">The input navigation property path.</param>
        /// <returns>True/False.</returns>
        public bool IsNonDeletableNavigationProperty(string navigationPropertyPath)
        {
            return NonDeletableNavigationProperties != null ?
                NonDeletableNavigationProperties.Any(a => a == navigationPropertyPath) :
                false;
        }

        /// <summary>
        /// Init the <see cref="DeleteRestrictionsType"/>.
        /// </summary>
        /// <param name="record">The input record.</param>
        public void Initialize(IEdmRecordExpression record)
        {
            Utils.CheckArgumentNull(record, nameof(record));

            // Deletable
            Deletable = record.GetBoolean("Deletable");

            // NonDeletableNavigationProperties
            NonDeletableNavigationProperties = record.GetCollectionPropertyPath("NonDeletableNavigationProperties");

            // MaxLevels
            MaxLevels = (int?)record.GetInteger("MaxLevels");

            // FilterSegmentSupported
            FilterSegmentSupported = record.GetBoolean("FilterSegmentSupported");

            // TypecastSegmentSupported
            TypecastSegmentSupported = record.GetBoolean("TypecastSegmentSupported");

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
        /// Merges properties of the specified <see cref="DeleteRestrictionsType"/> object into this instance if they are null.
        /// </summary>
        /// <param name="source">The <see cref="DeleteRestrictionsType"/> object containing properties to merge.</param>
        public void MergePropertiesIfNull(DeleteRestrictionsType source)
        {
            if (source == null)
                return;

            Deletable ??= source.Deletable;

            NonDeletableNavigationProperties ??= source.NonDeletableNavigationProperties;

            MaxLevels ??= source.MaxLevels;

            FilterSegmentSupported ??= source.FilterSegmentSupported;

            TypecastSegmentSupported ??= source.TypecastSegmentSupported;

            Permissions ??= source.Permissions;

            CustomHeaders ??= source.CustomHeaders;

            CustomQueryOptions ??= source.CustomQueryOptions;

            Description ??= source.Description;

            LongDescription ??= source.LongDescription;
        }
    }
}
