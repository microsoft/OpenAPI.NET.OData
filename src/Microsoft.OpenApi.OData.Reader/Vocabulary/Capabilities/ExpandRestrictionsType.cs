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
    /// Org.OData.Capabilities.V1.ExpandRestrictions
    /// </summary>
    [Term("Org.OData.Capabilities.V1.ExpandRestrictions")]
    internal class ExpandRestrictionsType : IRecord
    {
        /// <summary>
        /// Gets the Expandable value.
        /// </summary>
        public bool? Expandable { get; private set; }

        /// <summary>
        /// Gets the properties which cannot be used in $expand expressions.
        /// </summary>
        public IList<string> NonExpandableProperties { get; private set; }

        /// <summary>
        /// Gets the maximum number of levels that can be expanded in a expand expression.
        /// </summary>
        public long? MaxLevels { get; private set; }

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

        public void Initialize(IEdmRecordExpression record)
        {
            Utils.CheckArgumentNull(record, nameof(record));

            // Expandable
            Expandable = record.GetBoolean("Expandable");

            // NonExpandableProperties
            NonExpandableProperties = record.GetCollectionPropertyPath("NonExpandableProperties");

            // MaxLevels
            MaxLevels = record.GetInteger("MaxLevels");
        }
    }
}
