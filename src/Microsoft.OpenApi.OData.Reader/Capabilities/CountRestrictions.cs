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
        /// The Term type kind.
        /// </summary>
        public override CapabilitesTermKind Kind => CapabilitesTermKind.CountRestrictions;

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
        /// Test the target supports count.
        /// </summary>
        /// <returns>True/false.</returns>
        public bool IsCountable => Countable == null || Countable.Value;

        /// <summary>
        /// Test the input property path which do not allow /$count segments.
        /// </summary>
        /// <param name="propertyPath">The input property path. "property1/property2"</param>
        /// <returns>True/False.</returns>
        public bool IsNonCountableProperty(string propertyPath)
        {
            return NonCountableProperties != null ? NonCountableProperties.Any(a => a == propertyPath) : false;
        }

        /// <summary>
        /// Test the input navigation property which do not allow /$count segments.
        /// </summary>
        /// <param name="navigationPropertyPath">The input navigation property path.</param>
        /// <returns>True/False.</returns>
        public bool IsNonCountableNavigationProperty(string navigationPropertyPath)
        {
            return NonCountableNavigationProperties != null ?
                NonCountableNavigationProperties.Any(a => a == navigationPropertyPath) :
                false;
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

            // Countable
            Countable = record.GetBoolean("Countable");

            // NonCountableProperties
            NonCountableProperties = record.GetCollectionPropertyPath("NonCountableProperties");

            // NonCountableNavigationProperties
            NonCountableNavigationProperties = record.GetCollectionPropertyPath("NonCountableNavigationProperties");

            return true;
        }
    }
}
