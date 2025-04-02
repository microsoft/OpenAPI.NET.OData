// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Common;

namespace Microsoft.OpenApi.OData.Vocabulary.Capabilities
{
    /// <summary>
    /// Enumerates the search expressions.
    /// </summary>
    [Flags]
    internal enum SearchExpressions
    {
        /// <summary>
        /// none.
        /// </summary>
        none = 0,

        /// <summary>
        /// AND.
        /// </summary>
        AND = 1,

        /// <summary>
        /// OR.
        /// </summary>
        OR = 2,

        /// <summary>
        /// NOT.
        /// </summary>
        NOT = 4,

        /// <summary>
        /// phrase.
        /// </summary>
        phrase = 8,

        /// <summary>
        /// group.
        /// </summary>
        group = 16
    }

    /// <summary>
    /// Complex Type: Org.OData.Capabilities.V1.SearchRestrictionsType
    /// </summary>
    [Term("Org.OData.Capabilities.V1.SearchRestrictions")]
    internal class SearchRestrictionsType : IRecord
    {
        /// <summary>
        /// Gets the Searchable value.
        /// </summary>
        public bool? Searchable { get; private set; }

        /// <summary>
        /// Gets the search expressions which can is supported in $search.
        /// </summary>
        public SearchExpressions? UnsupportedExpressions { get; private set; }

        /// <summary>
        /// Test the target supports search.
        /// </summary>
        /// <returns>True/false.</returns>
        public bool IsSearchable => Searchable == null || Searchable.Value == true;

        /// <summary>
        /// Test the input expression supported or not.
        /// </summary>
        /// <param name="expression">The input expression</param>
        /// <returns>True/false.</returns>
        public bool IsUnsupportedExpressions(SearchExpressions expression)
        {
            if (UnsupportedExpressions == null || UnsupportedExpressions.Value == SearchExpressions.none)
            {
                return false;
            }

            if ((UnsupportedExpressions.Value & expression) == expression)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Init the <see cref="SearchRestrictionsType"/>.
        /// </summary>
        /// <param name="record">The input record.</param>
        public void Initialize(IEdmRecordExpression record)
        {
            Utils.CheckArgumentNull(record, nameof(record));

            // Searchable
            Searchable = record.GetBoolean("Searchable");

            // read the "UnsupportedExpressions"
            if (record.Properties.FirstOrDefault(e => e.Name == "UnsupportedExpressions") is {Value: IEdmEnumMemberExpression {EnumMembers: not null} value})
            {
				foreach (var v in value.EnumMembers)
				{
					if (Enum.TryParse(v.Name, out SearchExpressions result))
					{
						if (UnsupportedExpressions == null)
						{
							UnsupportedExpressions = result;
						}
						else
						{
							UnsupportedExpressions |= result;
						}
					}
				}
			}
        }
    }
}
