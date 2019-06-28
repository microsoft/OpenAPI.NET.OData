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
        /// Gets the navigation properties which do not allow rebinding.
        /// </summary>
        public IList<string> NonUpdatableNavigationProperties { get; private set; }

        /// <summary>
        /// Gets the maximum number of navigation properties that can be traversed when addressing the collection or entity to update.
        /// A value of -1 indicates there is no restriction.
        /// </summary>
        public long? MaxLevels { get; private set; }

        /// <summary>
        /// Gets/sets the required scopes to perform update.
        /// </summary>
        public PermissionType Permission { get; private set; }

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
        /// Init the <see cref="UpdateRestrictionsType"/>.
        /// </summary>
        /// <param name="record">The input record.</param>
        public void Initialize(IEdmRecordExpression record)
        {
            Utils.CheckArgumentNull(record, nameof(record));

            // Updatable
            Updatable = record.GetBoolean("Updatable");

            // NonUpdatableNavigationProperties
            NonUpdatableNavigationProperties = record.GetCollectionPropertyPath("NonUpdatableNavigationProperties");

            // MaxLevels
            MaxLevels = record.GetInteger("MaxLevels");

            // Permission
            Permission = record.GetRecord<PermissionType>("Permission");

            // QueryOptions
            QueryOptions = record.GetRecord<ModificationQueryOptionsType>("QueryOptions");

            // CustomHeaders
            CustomHeaders = record.GetCollection<CustomParameter>("CustomHeaders");

            // CustomQueryOptions
            CustomQueryOptions = record.GetCollection<CustomParameter>("CustomQueryOptions");
        }
    }
}
