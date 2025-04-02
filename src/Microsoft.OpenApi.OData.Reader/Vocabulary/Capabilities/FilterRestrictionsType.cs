// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.Vocabulary.Capabilities
{
    /// <summary>
    /// Complex Type: Org.OData.Capabilities.V1.FilterRestrictionsType
    /// </summary>
    [Term("Org.OData.Capabilities.V1.FilterRestrictions")]
    internal class FilterRestrictionsType : IRecord
    {
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
        public IList<string>? RequiredProperties { get; private set; }

        /// <summary>
        /// Gets the properties which cannot be used in $filter expressions.
        /// </summary>
        public IList<string>? NonFilterableProperties { get; private set; }

        /// <summary>
        /// Gets The maximum number of levels (including recursion) that can be traversed in a filter expression. A value of -1 indicates there is no restriction.
        /// </summary>
        public long? MaxLevels { get; private set; }

        /// <summary>
        /// Gets These properties only allow a subset of filter expressions.
        /// A valid filter expression for a single property can be enclosed in parentheses and combined by `and` with valid expressions for other properties.
        /// </summary>
        public IList<FilterExpressionRestrictionType>? FilterExpressionRestrictions { get; private set; }

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
            return RequiredProperties != null && RequiredProperties.Any(a => a == propertyPath);
        }

        /// <summary>
        /// Test the input property which cannot be used in $filter expressions.
        /// </summary>
        /// <param name="propertyPath">The input property path.</param>
        /// <returns>True/False.</returns>
        public bool IsNonFilterableProperty(string propertyPath)
        {
            return NonFilterableProperties != null && NonFilterableProperties.Any(a => a == propertyPath);
        }

        /// <summary>
        /// Init the <see cref="FilterRestrictionsType"/>.
        /// </summary>
        /// <param name="record">The input record.</param>
        public void Initialize(IEdmRecordExpression record)
        {
            Utils.CheckArgumentNull(record, nameof(record));

            // Filterable
            Filterable = record.GetBoolean("Filterable");

            // RequiresFilter
            RequiresFilter = record.GetBoolean("RequiresFilter");

            // RequiredProperties
            RequiredProperties = record.GetCollectionPropertyPath("RequiredProperties");

            // NonFilterableProperties
            NonFilterableProperties = record.GetCollectionPropertyPath("NonFilterableProperties");

            // MaxLevels
            MaxLevels = record.GetInteger("MaxLevels");

            // FilterExpressionRestrictions
            FilterExpressionRestrictions = record.GetCollection<FilterExpressionRestrictionType>("FilterExpressionRestrictions");
        }
    }
}
