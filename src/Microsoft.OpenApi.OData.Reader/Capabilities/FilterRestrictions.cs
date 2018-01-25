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
        /// The Term type name.
        /// </summary>
        public override string QualifiedName => CapabilitiesConstants.FilterRestrictions;

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
        /// Initializes a new instance of <see cref="FilterRestrictions"/> class.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="target">The Edm annotation target.</param>
        public FilterRestrictions(IEdmModel model, IEdmVocabularyAnnotatable target)
            : base(model, target)
        {
        }

        /// <summary>
        /// Test the target supports filter.
        /// </summary>
        /// <returns>True/false.</returns>
        public bool IsFilterable()
        {
            return Filterable == null || Filterable.Value == true;
        }

        /// <summary>
        /// Test the input property which must be specified in the $filter clause.
        /// </summary>
        /// <param name="property">The input property.</param>
        /// <returns>True/False.</returns>
        public bool IsRequiredProperty(IEdmProperty property)
        {
            return RequiredProperties != null ? RequiredProperties.Any(a => a == property.Name) : false;
        }

        /// <summary>
        /// Test the input property which cannot be used in $filter expressions.
        /// </summary>
        /// <param name="property">The input property.</param>
        /// <returns>True/False.</returns>
        public bool IsNonFilterableProperty(IEdmProperty property)
        {
            return NonFilterableProperties != null ? NonFilterableProperties.Any(a => a == property.Name) : false;
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

            // Filterable
            Filterable = record.GetBoolean("Filterable");

            // RequiresFilter
            RequiresFilter = record.GetBoolean("RequiresFilter");

            // RequiredProperties
            RequiredProperties = record.GetCollectionPropertyPath("RequiredProperties");

            // NonFilterableProperties
            NonFilterableProperties = record.GetCollectionPropertyPath("NonFilterableProperties");
        }
    }
}
