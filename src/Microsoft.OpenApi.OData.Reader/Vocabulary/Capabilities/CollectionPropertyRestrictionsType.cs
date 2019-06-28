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
    /// Complex Type: Org.OData.Capabilities.V1.CollectionPropertyRestrictionsType
    /// </summary>
    [Term("Org.OData.Capabilities.V1.CollectionPropertyRestrictions")]
    internal class CollectionPropertyRestrictionsType : IRecord
    {
        /// <summary>
        /// Gets the Restricted Collection-valued property.
        /// </summary>
        public string CollectionProperty { get; private set; }

        /// <summary>
        /// Gets the List of functions and operators supported in filter expressions..
        /// </summary>
        public IList<string> FilterFunctions { get; private set; }

        /// <summary>
        /// Gets Restrictions on filter expressions.
        /// </summary>
        public FilterRestrictionsType FilterRestrictions { get; private set; }

        /// <summary>
        /// Gets Restrictions on search expressions.
        /// </summary>
        public SearchRestrictionsType SearchRestrictions { get; private set; }

        /// <summary>
        /// Gets Restrictions on orderby expressions.
        /// </summary>
        public SortRestrictionsType SortRestrictions { get; private set; }

        /// <summary>
        /// Gets Supports $top.
        /// </summary>
        public bool? TopSupported { get; private set; }

        /// <summary>
        /// Gets Supports $skip.
        /// </summary>
        public bool? SkipSupported { get; private set; }

        /// <summary>
        /// Gets Support for $select.
        /// </summary>
        public SelectSupportType SelectSupport { get; private set; }

        /// <summary>
        /// Gets the collection supports positional inserts.
        /// </summary>
        public bool? Insertable { get; private set; }

        /// <summary>
        /// Gets the Members of this ordered collection can be updated by ordinal.
        /// </summary>
        public bool? Updatable { get; private set; }

        /// <summary>
        /// Gets the Members of this ordered collection can be deleted by ordinal.
        /// </summary>
        public bool? Deletable { get; private set; }

        /// <summary>
        /// Init the <see cref="CollectionPropertyRestrictionsType"/>.
        /// </summary>
        /// <param name="record">The input record.</param>
        public virtual void Initialize(IEdmRecordExpression record)
        {
            Utils.CheckArgumentNull(record, nameof(record));

            // CollectionProperty
            CollectionProperty = record.GetPropertyPath("CollectionProperty");

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

            // Insertable
            Insertable = record.GetBoolean("Insertable");

            // Updatable
            Updatable = record.GetBoolean("Updatable");

            // Deletable
            Deletable = record.GetBoolean("Deletable");
        }
    }
}
