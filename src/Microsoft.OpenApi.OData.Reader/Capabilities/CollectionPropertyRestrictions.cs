// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;

namespace Microsoft.OpenApi.OData.Capabilities
{
    /// <summary>
    /// Org.OData.Capabilities.V1.CollectionPropertyRestrictions
    /// </summary>
    internal class CollectionPropertyRestrictions : CapabilitiesRestrictions
    {
        /// <summary>
        /// The Term type kind.
        /// </summary>
        public override CapabilitesTermKind Kind  => CapabilitesTermKind.CollectionPropertyRestrictions;

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
        public FilterRestrictions FilterRestrictions { get; private set; }

        /// <summary>
        /// Gets Restrictions on search expressions.
        /// </summary>
        public SearchRestrictions SearchRestrictions { get; private set; }

        /// <summary>
        /// Gets Restrictions on orderby expressions.
        /// </summary>
        public SortRestrictions SortRestrictions { get; private set; }

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

        protected override bool Initialize(IEdmVocabularyAnnotation annotation)
        {
            if (annotation == null ||
                annotation.Value == null ||
                annotation.Value.ExpressionKind != EdmExpressionKind.Record)
            {
                return false;
            }

            IEdmRecordExpression record = (IEdmRecordExpression)annotation.Value;

            // Deletable
            Deletable = record.GetBoolean("Deletable");

            return true;
        }
    }
}
