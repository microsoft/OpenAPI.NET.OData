// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.OpenApi.OData.Vocabulary.Capabilities
{
    /// <summary>
    /// Enumerates the navigation type can apply on navigation restrictions.
    /// </summary>
    internal enum NavigationType
    {
        /// <summary>
        /// Navigation properties can be recursively navigated.
        /// </summary>
        Recursive,

        /// <summary>
        /// Navigation properties can be navigated to a single level.
        /// </summary>
        Single,

        /// <summary>
        /// Navigation properties are not navigable.
        /// </summary>
        None
    }

    /// <summary>
    /// Complex Type: Org.OData.Capabilities.V1.NavigationPropertyRestriction
    /// </summary>
    internal class NavigationPropertyRestriction : IRecord
    {
        /// <summary>
        /// Navigation properties can be navigated
        /// </summary>
        public string NavigationProperty { get; set; }

        /// <summary>
        /// Navigation properties can be navigated to this level.
        /// </summary>
        public NavigationType? Navigability { get; set; }

        /// <summary>
        /// List of functions and operators supported in filter expressions.
        /// </summary>
        public IList<string> FilterFunctions { get; set; }

        /// <summary>
        /// Restrictions on filter expressions.
        /// </summary>
        public FilterRestrictionsType FilterRestrictions { get; set; }

        /// <summary>
        /// Restrictions on search expressions.
        /// </summary>
        public SearchRestrictionsType SearchRestrictions { get; set; }

        /// <summary>
        /// Restrictions on orderby expressions.
        /// </summary>
        public SortRestrictionsType SortRestrictions { get; set; }

        /// <summary>
        /// Supports $top.
        /// </summary>
        public bool? TopSupported { get; set; }

        /// <summary>
        /// Supports $skip.
        /// </summary>
        public bool? SkipSupported { get; set; }

        /// <summary>
        /// Supports $select.
        /// </summary>
        public SelectSupportType SelectSupport { get; set; }

        /// <summary>
        /// Supports key values according to OData URL conventions.
        /// </summary>
        public bool? IndexableByKey { get; set; }

        /// <summary>
        /// Restrictions on insert operations.
        /// </summary>
        public InsertRestrictionsType InsertRestrictions { get; set; }

        /// <summary>
        /// Deep Insert Support of the annotated resource (the whole service, an entity set, or a collection-valued resource).
        /// </summary>
        public DeepInsertSupportType DeepInsertSupport { get; set; }

        /// <summary>
        /// Restrictions on update operations.
        /// </summary>
        public UpdateRestrictionsType UpdateRestrictions { get; set; }

        /// <summary>
        /// Deep Update Support of the annotated resource (the whole service, an entity set, or a collection-valued resource).
        /// </summary>
        public DeepUpdateSupportType DeepUpdateSupport { get; set; }

        /// <summary>
        /// Restrictions on delete operations.
        /// </summary>
        public DeleteRestrictionsType DeleteRestrictions { get; set; }

        /// <summary>
        /// Data modification (including insert) along this navigation property requires the use of ETags.
        /// </summary>
        public bool? OptimisticConcurrencyControl { get; set; }

        /// <summary>
        /// Restrictions for retrieving entities.
        /// </summary>
        public ReadRestrictionsType ReadRestrictions { get; set; }
        
        /// <summary>
        /// Init the <see cref="NavigationPropertyRestriction"/>.
        /// </summary>
        /// <param name="record">The input record.</param>
        public void Initialize(IEdmRecordExpression record)
        {
            Utils.CheckArgumentNull(record, nameof(record));

            // NavigationProperty
            NavigationProperty = record.GetPropertyPath("NavigationProperty"); // maybe call "GetNavigationPropertyPath

            // Navigability
            Navigability = record.GetEnum<NavigationType>("Navigability");

            // FilterFunctions
            FilterFunctions = record.GetCollection("FilterFunctions");

            // FilterRestrictions
            FilterRestrictions = record.GetRecord<FilterRestrictionsType>("FilterRestrictions");

            // SearchRestrictions
            SearchRestrictions = record.GetRecord<SearchRestrictionsType>("SearchRestrictions");

            // SortRestrictions
            SortRestrictions = record.GetRecord<SortRestrictionsType>("SortRestrictions");

            // TopSupported
            TopSupported = record.GetBoolean("TopSupported");

            // SkipSupported
            SkipSupported = record.GetBoolean("SkipSupported");

            // SelectSupport
            SelectSupport = record.GetRecord<SelectSupportType>("SelectSupport");

            // IndexableByKey
            IndexableByKey = record.GetBoolean("IndexableByKey");

            // InsertRestrictions
            InsertRestrictions = record.GetRecord<InsertRestrictionsType>("InsertRestrictions");

            // DeepInsertSupport
            DeepInsertSupport = record.GetRecord<DeepInsertSupportType>("DeepInsertSupport");

            // UpdateRestrictions
            UpdateRestrictions = record.GetRecord<UpdateRestrictionsType>("UpdateRestrictions");

            // DeepUpdateSupport
            DeepUpdateSupport = record.GetRecord<DeepUpdateSupportType>("DeepUpdateSupport");

            // DeleteRestrictions
            DeleteRestrictions = record.GetRecord<DeleteRestrictionsType>("DeleteRestrictions");

            // OptimisticConcurrencyControl
            OptimisticConcurrencyControl = record.GetBoolean("OptimisticConcurrencyControl");

            // ReadRestrictions
            ReadRestrictions = record.GetRecord<ReadRestrictionsType>("ReadRestrictions");
        }
    }

    /// <summary>
    /// Complex Type: Org.OData.Capabilities.V1.NavigationRestrictionsType
    /// </summary>
    [Term("Org.OData.Capabilities.V1.NavigationRestrictions")]
    internal class NavigationRestrictionsType : IRecord
    {
        /// <summary>
        /// Gets the Navigability value.
        /// </summary>
        public NavigationType? Navigability { get; private set; }

        /// <summary>
        /// Gets the navigation properties which has navigation restrictions.
        /// </summary>
        public IList<NavigationPropertyRestriction> RestrictedProperties { get; private set; }

        /// <summary>
        /// Gets the navigation property referenceable value.
        /// </summary>
        public bool? Referenceable { get; set; }

        /// <summary>
        /// Gets a value indicating the target is navigable or not.
        /// </summary>
        public bool IsNavigable => Navigability == null || Navigability.Value != NavigationType.None;

        /// <summary>
        /// Test the input navigation property which has navigation restrictions.
        /// </summary>
        /// <param name="navigationPropertyPath">The input navigation property path.</param>
        /// <returns>True/False.</returns>
        public bool IsRestrictedProperty(string navigationPropertyPath)
        {
            return RestrictedProperties != null ?
                RestrictedProperties.Where(a => a.NavigationProperty == navigationPropertyPath)
                .Any(b => b.Navigability != null && b.Navigability.Value == NavigationType.None) :
                false;
        }

        /// <summary>
        /// Init the <see cref="NavigationRestrictionsType"/>.
        /// </summary>
        /// <param name="record">The input record.</param>
        public void Initialize(IEdmRecordExpression record)
        {
            Utils.CheckArgumentNull(record, nameof(record));

            // Navigability
            Navigability = record.GetEnum<NavigationType>("Navigability");

            // RestrictedProperties
            RestrictedProperties = record.GetCollection<NavigationPropertyRestriction>("RestrictedProperties");

            // Referenceable
            Referenceable = record.GetBoolean("Referenceable");
        }

        /// <summary>
        /// Merges properties of the specified <see cref="NavigationRestrictionsType"/> object into this instance if they are null.
        /// </summary>
        /// <param name="source">The <see cref="NavigationRestrictionsType"/> object containing properties to merge.</param>
        public void MergePropertiesIfNull(NavigationRestrictionsType source)
        {
            if (source == null)
                return;

            Navigability ??= source.Navigability;

            RestrictedProperties ??= source.RestrictedProperties;

            Referenceable ??= source.Referenceable;
        }
    }
}
