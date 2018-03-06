// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Common;

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
        public override string QualifiedName => CapabilitiesConstants.SortRestrictions;

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
        /// Initializes a new instance of <see cref="SortRestrictions"/> class.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="target">The Edm annotation target.</param>
        public SortRestrictions(IEdmModel model, IEdmVocabularyAnnotatable target)
            : base(model, target)
        {
        }

        /// <summary>
        /// Test the input property is Ascending only.
        /// </summary>
        /// <param name="property">The input property.</param>
        /// <returns>True/False.</returns>
        public bool IsAscendingOnlyProperty(IEdmProperty property)
        {
            return AscendingOnlyProperties != null ? AscendingOnlyProperties.Any(a => a == property.Name) : false;
        }

        /// <summary>
        /// Test the input property is Descending only.
        /// </summary>
        /// <param name="property">The input property.</param>
        /// <returns>True/False.</returns>
        public bool IsDescendingOnlyProperty(IEdmProperty property)
        {
            return DescendingOnlyProperties != null ? DescendingOnlyProperties.Any(a => a == property.Name) : false;
        }

        /// <summary>
        /// Test the input property cannot be used in $orderby expressions.
        /// </summary>
        /// <param name="property">The input property.</param>
        /// <returns>True/False.</returns>
        public bool IsNonSortableProperty(IEdmProperty property)
        {
            return NonSortableProperties != null ? NonSortableProperties.Any(a => a == property.Name) : false;
        }

        protected override void Initialize(IEdmVocabularyAnnotation annotation)
        {
            if (annotation == null ||
               annotation.Value == null ||
               annotation.Value.ExpressionKind != EdmExpressionKind.Record)
            {
                return;
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
        }
    }
}
