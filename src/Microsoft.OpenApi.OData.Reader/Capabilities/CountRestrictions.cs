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
    /// Org.OData.Capabilities.V1.CountRestrictions
    /// </summary>
    internal class CountRestrictions : CapabilitiesRestrictions
    {
        /// <summary>
        /// The Term type name.
        /// </summary>
        public override string QualifiedName => CapabilitiesConstants.CountRestrictions;

        /// <summary>
        /// Gets the Countable value.
        /// </summary>
        public bool? Countable { get; private set; }

        /// <summary>
        /// Gets the properties which do not allow /$count segments.
        /// </summary>
        public IList<string> NonCountableProperties { get; private set; }

        /// <summary>
        /// Gets the navigation properties which do not allow /$count segments.
        /// </summary>
        public IList<string> NonCountableNavigationProperties { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="CountRestrictions"/> class.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="target">The Edm annotation target.</param>
        public CountRestrictions(IEdmModel model, IEdmVocabularyAnnotatable target)
            : base(model, target)
        {
        }

        /// <summary>
        /// Test the input property which do not allow /$count segments.
        /// </summary>
        /// <param name="property">The input property.</param>
        /// <returns>True/False.</returns>
        public bool IsNonCountableProperty(IEdmProperty property)
        {
            return NonCountableProperties != null ?
                NonCountableProperties.Any(a => a == property.Name) :
                false;
        }

        /// <summary>
        /// Test the input navigation property which do not allow /$count segments.
        /// </summary>
        /// <param name="property">The input navigation property.</param>
        /// <returns>True/False.</returns>
        public bool IsNonCountableNavigationProperty(IEdmNavigationProperty property)
        {
            return NonCountableNavigationProperties != null ?
                NonCountableNavigationProperties.Any(a => a == property.Name) :
                false;
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

            // Countable
            Countable = record.GetBoolean("Countable");

            // NonCountableProperties
            NonCountableProperties = record.GetCollectionPropertyPath("NonCountableProperties");

            // NonCountableNavigationProperties
            NonCountableNavigationProperties = record.GetCollectionPropertyPath("NonCountableNavigationProperties");
        }
    }
}
