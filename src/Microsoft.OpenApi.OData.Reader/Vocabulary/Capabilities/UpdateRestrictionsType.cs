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
    /// Enumerates HTTP methods that can be used to update entities
    /// </summary>
    internal enum HttpMethod
    {
        /// <summary>
        /// The HTTP PATCH Method
        /// </summary>
        PATCH,

        /// <summary>
        /// The HTTP PUT Method
        /// </summary>
        PUT
    }
    /// <summary>
    /// Complex Type: Org.OData.Capabilities.V1.UpdateRestrictionsType
    /// </summary>
    [Term("Org.OData.Capabilities.V1.UpdateRestrictions")]
    internal class UpdateRestrictionsType : IRecord
    {
        /// <summary>
        /// Gets the Updatable value, if true, entities can be updated.
        /// The default value is true;
        /// </summary>
        public bool? Updatable { get; private set; }

        /// <summary>
        /// Gets the value indicating Entities can be upserted.
        /// </summary>
        public bool? Upsertable { get; private set; }

        /// <summary>
        /// Gets the value indicating Entities can be inserted, updated, and deleted via a PATCH request with a delta payload.
        /// </summary>
        public bool? DeltaUpdateSupported { get; private set; }

        /// <summary>
        /// Gets the value indicating the HTTP Method (PUT or PATCH) for updating an entity. 
        /// If null, PATCH should be supported and PUT MAY be supported.
        /// </summary>
        public HttpMethod? UpdateMethod { get; private set; }

        /// <summary>
        /// Gets the value indicating Members of collections can be updated via a PATCH request with a '/$filter(...)/$each' segment.
        /// </summary>
        public bool? FilterSegmentSupported { get; private set; }

        /// <summary>
        /// Gets the value indicating Members of collections can be updated via a PATCH request with a type-cast segment and a '/$each' segment.
        /// </summary>
        public bool? TypecastSegmentSupported { get; private set; }

        /// <summary>
        /// Gets the navigation properties which do not allow rebinding.
        /// </summary>
        public IList<string> NonUpdatableNavigationProperties { get; private set; }

        /// <summary>
        /// Gets the maximum number of navigation properties that can be traversed when addressing the collection or entity to update.
        /// A value of -1 indicates there is no restriction.
        /// </summary>
        public long? MaxLevels { get; private set; }

        /// <summary>
        /// Gets the Required permissions. One of the specified sets of scopes is required to perform the update.
        /// </summary>
        public IList<PermissionType> Permissions { get; private set; }

        /// <summary>
        /// Gets/sets the support for query options with update requests.
        /// </summary>
        public ModificationQueryOptionsType QueryOptions { get; private set; }

        /// <summary>
        /// Gets/sets the supported or required custom headers.
        /// </summary>
        public IList<CustomParameter> CustomHeaders { get; private set; }

        /// <summary>
        /// Gets/sets the supported or required custom query options.
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
        /// Test the target supports update.
        /// </summary>
        /// <returns>True/false.</returns>
        public bool IsUpdatable => Updatable == null || Updatable.Value;

        /// <summary>
        /// Test the input navigation property do not allow rebinding.
        /// </summary>
        /// <param name="navigationPropertyPath">The input navigation property path.</param>
        /// <returns>True/False.</returns>
        public bool IsNonUpdatableNavigationProperty(string navigationPropertyPath)
        {
            return NonUpdatableNavigationProperties != null ?
                NonUpdatableNavigationProperties.Any(a => a == navigationPropertyPath) :
                false;
        }

        /// <summary>
        /// Tests whether the update method for the entity has been explicitly specified as PUT
        /// </summary>
        public bool IsUpdateMethodPut => UpdateMethod.HasValue && UpdateMethod.Value == HttpMethod.PUT;

        /// <summary>
        /// Init the <see cref="UpdateRestrictionsType"/>.
        /// </summary>
        /// <param name="record">The input record.</param>
        public void Initialize(IEdmRecordExpression record)
        {
            Utils.CheckArgumentNull(record, nameof(record));

            // Updatable
            Updatable = record.GetBoolean("Updatable");

            // Upsertable
            Upsertable = record.GetBoolean("Upsertable");

            // DeltaUpdateSupported
            DeltaUpdateSupported = record.GetBoolean("DeltaUpdateSupported");

            // UpdateMethod
            UpdateMethod = record.GetEnum<HttpMethod>("UpdateMethod");

            // FilterSegmentSupported
            FilterSegmentSupported = record.GetBoolean("FilterSegmentSupported");

            // TypecastSegmentSupported
            TypecastSegmentSupported = record.GetBoolean("TypecastSegmentSupported");

            // NonUpdatableNavigationProperties
            NonUpdatableNavigationProperties = record.GetCollectionPropertyPath("NonUpdatableNavigationProperties");

            // MaxLevels
            MaxLevels = record.GetInteger("MaxLevels");

            // Permissions
            Permissions = record.GetCollection<PermissionType>("Permissions");

            // QueryOptions
            QueryOptions = record.GetRecord<ModificationQueryOptionsType>("QueryOptions");

            // CustomHeaders
            CustomHeaders = record.GetCollection<CustomParameter>("CustomHeaders");

            // CustomQueryOptions
            CustomQueryOptions = record.GetCollection<CustomParameter>("CustomQueryOptions");

            // Description
            Description = record.GetString("Description");

            // LongDescription
            LongDescription = record.GetString("LongDescription");
        }
    }
}
