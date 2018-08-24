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
    /// Org.OData.Capabilities.V1.ExpandRestrictions
    /// </summary>
    internal class ExpandRestrictions : CapabilitiesRestrictions
    {
        /// <summary>
        /// The Term type kind.
        /// </summary>
        public override CapabilitesTermKind Kind => CapabilitesTermKind.ExpandRestrictions;

        /// <summary>
        /// Gets the Expandable value.
        /// </summary>
        public bool? Expandable { get; private set; }

        /// <summary>
        /// Gets the properties which cannot be used in $expand expressions.
        /// </summary>
        public IList<string> NonExpandableProperties { get; private set; }

        /// <summary>
        /// Test the target supports $expand.
        /// </summary>
        /// <returns>True/false.</returns>
        public bool IsExpandable => Expandable == null || Expandable.Value;

        /// <summary>
        /// Test the input property cannot be used in $orderby expressions.
        /// </summary>
        /// <param name="navigationPropertyPath">The input navigation property path.</param>
        /// <returns>True/False.</returns>
        public bool IsNonExpandableProperty(string navigationPropertyPath)
        {
            return NonExpandableProperties != null ? NonExpandableProperties.Any(a => a == navigationPropertyPath) : false;
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

            // Expandable
            Expandable = record.GetBoolean("Expandable");

            // NonExpandableProperties
            NonExpandableProperties = record.GetCollectionPropertyPath("NonExpandableProperties");

            return true;
        }
    }
}
