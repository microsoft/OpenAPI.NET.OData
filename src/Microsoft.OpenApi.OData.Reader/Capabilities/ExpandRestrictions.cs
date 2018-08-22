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
        /// The Term type name.
        /// </summary>
        public override string QualifiedName => CapabilitiesConstants.ExpandRestrictions;

        /// <summary>
        /// Gets the Expandable value.
        /// </summary>
        public bool? Expandable { get; private set; }

        /// <summary>
        /// Gets the properties which cannot be used in $expand expressions.
        /// </summary>
        public IList<string> NonExpandableProperties { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="ExpandRestrictions"/> class.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="entityStargetet">The Edm annotation target.</param>
        public ExpandRestrictions(IEdmModel model, IEdmVocabularyAnnotatable target)
            : base(model, target)
        {
        }

        /// <summary>
        /// Test the target supports $expand.
        /// </summary>
        /// <returns>True/false.</returns>
        public bool IsExpandable => Expandable == null || Expandable.Value;

        /// <summary>
        /// Test the input property cannot be used in $orderby expressions.
        /// </summary>
        /// <param name="property">The input navigation property.</param>
        /// <returns>True/False.</returns>
        public bool IsNonExpandableProperty(IEdmNavigationProperty property)
        {
            return NonExpandableProperties != null ? NonExpandableProperties.Any(a => a == property.Name) : false;
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

            // Expandable
            Expandable = record.GetBoolean("Expandable");

            // NonExpandableProperties
            NonExpandableProperties = record.GetCollectionPropertyPath("NonExpandableProperties");
        }
    }
}
