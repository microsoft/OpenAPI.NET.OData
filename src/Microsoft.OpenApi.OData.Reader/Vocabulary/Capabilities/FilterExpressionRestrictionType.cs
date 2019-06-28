// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.Vocabulary.Capabilities
{
    /// <summary>
    /// Complex Type: Org.OData.Capabilities.V1.FilterExpressionRestrictionType
    /// </summary>
    internal class FilterExpressionRestrictionType : IRecord
    {
        /// <summary>
        /// Gets the Path to the restricted property.
        /// </summary>
        public string Property { get; private set; }

        /// <summary>
        /// Gets the RequiresFilter value.
        /// <Property Name="AllowedExpressions" Type="Capabilities.FilterExpressionType">
        /// <TypeDefinition Name="FilterExpressionType" UnderlyingType="Edm.String">
        /// </summary>
        public string AllowedExpressions { get; private set; }

        /// <summary>
        /// Init the <see cref="FilterExpressionRestrictionType"/>.
        /// </summary>
        /// <param name="record">The input record.</param>
        public void Initialize(IEdmRecordExpression record)
        {
            Utils.CheckArgumentNull(record, nameof(record));

            // Property
            Property = record.GetPropertyPath("Property");

            // AllowedExpressions
            AllowedExpressions = record.GetString("AllowedExpressions");
        }
    }
}
