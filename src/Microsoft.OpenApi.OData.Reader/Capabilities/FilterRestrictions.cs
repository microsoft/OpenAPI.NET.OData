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
    /// Org.OData.Capabilities.V1.FilterRestrictions
    /// </summary>
    internal class FilterRestrictions : CapabilitiesRestrictions
    {
        /// <summary>
        /// The Term type kind.
        /// </summary>
        public override CapabilitesTermKind Kind => CapabilitesTermKind.FilterRestrictions;

        /// <summary>
        /// Gets the Filterable value.
        /// </summary>
        public bool? Filterable { get; private set; }

        /// <summary>
        /// Gets the RequiresFilter value.
        /// </summary>
        public bool? RequiresFilter { get; private set; }

        /// <summary>
        /// Gets the properties which must be specified in the $filter clause.
        /// </summary>
        public IList<string> RequiredProperties { get; private set; }

        /// <summary>
        /// Gets the properties which cannot be used in $filter expressions.
        /// </summary>
        public IList<string> NonFilterableProperties { get; private set; }

        /// <summary>
        /// Test the target supports filter.
        /// </summary>
        /// <returns>True/false.</returns>
        public bool IsFilterable => Filterable == null || Filterable.Value;

        /// <summary>
        /// Test the input property which must be specified in the $filter clause.
        /// </summary>
        /// <param name="propertyPath">The input property path.</param>
        /// <returns>True/False.</returns>
        public bool IsRequiredProperty(string propertyPath)
        {
            return RequiredProperties != null ? RequiredProperties.Any(a => a == propertyPath) : false;
        }

        /// <summary>
        /// Test the input property which cannot be used in $filter expressions.
        /// </summary>
        /// <param name="propertyPath">The input property path.</param>
        /// <returns>True/False.</returns>
        public bool IsNonFilterableProperty(string propertyPath)
        {
            return NonFilterableProperties != null ? NonFilterableProperties.Any(a => a == propertyPath) : false;
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

            // Filterable
            Filterable = record.GetBoolean("Filterable");

            // RequiresFilter
            RequiresFilter = record.GetBoolean("RequiresFilter");

            // RequiredProperties
            RequiredProperties = record.GetCollectionPropertyPath("RequiredProperties");

            // NonFilterableProperties
            NonFilterableProperties = record.GetCollectionPropertyPath("NonFilterableProperties");

            return true;
        }
    }
}
