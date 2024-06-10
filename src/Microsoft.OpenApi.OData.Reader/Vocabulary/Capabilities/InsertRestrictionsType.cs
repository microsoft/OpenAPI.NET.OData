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
    /// Complex Type: Org.OData.Capabilities.V1.InsertRestrictionsType
    /// </summary>
    [Term("Org.OData.Capabilities.V1.InsertRestrictions")]
    internal class InsertRestrictionsType : IRecord
    {
        /// <summary>
        /// Gets the Insertable value.
        /// </summary>
        public bool? Insertable { get; private set; }

        /// <summary>
        /// Gets the structural properties cannot be specified on insert.
        /// </summary>
        public IList<string> NonInsertableProperties { get; private set; }

        /// <summary>
        /// Gets the navigation properties which do not allow deep inserts.
        /// </summary>
        public IList<string> NonInsertableNavigationProperties { get; private set; }

        /// <summary>
        /// Gets the maximum number of navigation properties that can be traversed.
        /// </summary>
        public long? MaxLevels { get; private set; }

        /// <summary>
        /// Gets the Entities of a specific derived type can be created by specifying a type-cast segment.
        /// </summary>
        public bool? TypecastSegmentSupported { get; private set; }

        /// <summary>
        /// Gets the required scopes to perform the insert.
        /// </summary>
        public IList<PermissionType> Permissions { get; private set; }

        /// <summary>
        /// Gets the Support for query options with insert requests.
        /// </summary>
        public ModificationQueryOptionsType QueryOptions { get; private set; }

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
        /// Test the target supports insert.
        /// </summary>
        /// <returns>True/false.</returns>
        public bool IsInsertable => Insertable == null || Insertable.Value == true;

        /// <summary>
        /// Lists the media types acceptable for the request content
        /// </summary>
        /// <remarks>This is not an official OASIS standard property.</remarks>
        public IList<string> RequestContentTypes { get; private set; }

        /// <summary>
        /// Lists the media types acceptable for the response content
        /// </summary>
        /// <remarks>This is not an official OASIS standard property.</remarks>
        public IList<string> ResponseContentTypes { get; private set; }

        /// <summary>
        /// Test the input navigation property do not allow deep insert.
        /// </summary>
        /// <param name="navigationPropertyPath">The input navigation property path.</param>
        /// <returns>True/False.</returns>
        public bool IsNonInsertableNavigationProperty(string navigationPropertyPath)
        {
            return NonInsertableNavigationProperties != null ?
                NonInsertableNavigationProperties.Any(a => a == navigationPropertyPath) :
                false;
        }

        /// <summary>
        /// Initialize the capabilities with the vocabulary annotation.
        /// </summary>
        /// <param name="record">The input vocabulary record annotation.</param>
        public void Initialize(IEdmRecordExpression record)
        {
            Utils.CheckArgumentNull(record, nameof(record));

            // Insertable
            Insertable = record.GetBoolean("Insertable");

            // NonInsertableNavigationProperties
            NonInsertableNavigationProperties = record.GetCollectionPropertyPath("NonInsertableNavigationProperties");

            // MaxLevels
            MaxLevels = record.GetInteger("MaxLevels");

            // TypecastSegmentSupported
            TypecastSegmentSupported = record.GetBoolean("TypecastSegmentSupported");

            // Permissions
            Permissions = record.GetCollection<PermissionType>("Permissions");

            // QueryOptions
            QueryOptions = record.GetRecord<ModificationQueryOptionsType>("QueryOptions");

            // CustomHeaders
            CustomHeaders = record.GetCollection<CustomParameter>("CustomHeaders");

            // CustomHeaders
            CustomQueryOptions = record.GetCollection<CustomParameter>("CustomQueryOptions");

            // Description
            Description = record.GetString("Description");

            // LongDescription
            LongDescription = record.GetString("LongDescription");

            // RequestContentTypes
            RequestContentTypes = record.GetCollection("RequestContentTypes");

            // ResponseContentTypes
            ResponseContentTypes = record.GetCollection("ResponseContentTypes");
        }

        /// <summary>
        /// Merges properties of the specified <see cref="InsertRestrictionsType"/> object into this instance if they are null.
        /// </summary>
        /// <param name="source">The <see cref="InsertRestrictionsType"/> object containing properties to merge.</param>
        public void MergePropertiesIfNull(InsertRestrictionsType source)
        {
            if (source == null)
                return;

            Insertable ??= source.Insertable;

            NonInsertableNavigationProperties ??= source.NonInsertableNavigationProperties;

            MaxLevels ??= source.MaxLevels;

            TypecastSegmentSupported ??= source.TypecastSegmentSupported;

            Permissions ??= source.Permissions;

            QueryOptions ??= source.QueryOptions;

            CustomHeaders ??= source.CustomHeaders;

            CustomQueryOptions ??= source.CustomQueryOptions;

            Description ??= source.Description;

            LongDescription ??= source.LongDescription;

            RequestContentTypes ??= source.RequestContentTypes;

            ResponseContentTypes ??= source.ResponseContentTypes;
        }
    }
}
