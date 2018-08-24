// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;

namespace Microsoft.OpenApi.OData.Capabilities
{
    /// <summary>
    /// Org.OData.Capabilities.V1.SortRestrictions
    /// </summary>
    internal class SortRestrictions : CapabilitiesRestrictions
    {
        /// <summary>
        /// The Term type name.
        /// </summary>
        public override CapabilitesTermKind Kind => CapabilitesTermKind.SortRestrictions;

        /// <summary>
        /// Gets the Sortable value.
        /// </summary>
        public bool? Sortable { get; private set; }

        /// <summary>
        /// Gets the properties which can only be used for sorting in Ascending order.
        /// </summary>
        public IList<string> AscendingOnlyProperties { get; private set; }

        /// <summary>
        /// Gets the properties which can only be used for sorting in Descending order.
        /// </summary>
        public IList<string> DescendingOnlyProperties { get; private set; }

        /// <summary>
        /// Gets the properties which cannot be used in $orderby expressions.
        /// </summary>
        public IList<string> NonSortableProperties { get; private set; }

        /// <summary>
        /// Gets a boolean value indicating whether the target supports $orderby.
        /// </summary>
        public bool IsSortable => Sortable == null || Sortable.Value == true;

        /// <summary>
        /// Test the input property is Ascending only.
        /// </summary>
        /// <param name="propertyPath">The input property path.</param>
        /// <returns>True/False.</returns>
        public bool IsAscendingOnlyProperty(string propertyPath)
        {
            return AscendingOnlyProperties != null ? AscendingOnlyProperties.Any(a => a == propertyPath) : false;
        }

        /// <summary>
        /// Test the input property is Descending only.
        /// </summary>
        /// <param name="propertyPath">The input property path.</param>
        /// <returns>True/False.</returns>
        public bool IsDescendingOnlyProperty(string propertyPath)
        {
            return DescendingOnlyProperties != null ? DescendingOnlyProperties.Any(a => a == propertyPath) : false;
        }

        /// <summary>
        /// Test the input property cannot be used in $orderby expressions.
        /// </summary>
        /// <param name="propertyPath">The input property path.</param>
        /// <returns>True/False.</returns>
        public bool IsNonSortableProperty(string propertyPath)
        {
            return NonSortableProperties != null ? NonSortableProperties.Any(a => a == propertyPath) : false;
        }

        protected override bool Initialize(IEdmVocabularyAnnotation annotation)
        {
            if (annotation == null ||
               annotation.Value == null ||
               annotation.Value.ExpressionKind != EdmExpressionKind.Record)
            {
                return false;
            }

            IEdmRecordExpression record = (IEdmRecordExpression)annotation.Value;

            // Sortable
            Sortable = record.GetBoolean("Sortable");

            // AscendingOnlyProperties
            AscendingOnlyProperties = record.GetCollectionPropertyPath("AscendingOnlyProperties");

            // DescendingOnlyProperties
            DescendingOnlyProperties = record.GetCollectionPropertyPath("DescendingOnlyProperties");

            // NonSortablePropeties
            NonSortableProperties = record.GetCollectionPropertyPath("NonSortableProperties");

            return true;
        }
    }
}
