// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using System.Linq;

namespace Microsoft.OpenApi.OData.Capabilities
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
    /// Org.OData.Capabilities.V1.SearchRestrictions
    /// </summary>
    internal class SearchRestrictions : CapabilitiesRestrictions
    {
        /// <summary>
        /// The Term type kind.
        /// </summary>
        public override CapabilitesTermKind Kind => CapabilitesTermKind.SearchRestrictions;

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

        protected override bool Initialize(IEdmVocabularyAnnotation annotation)
        {
            if (annotation == null ||
               annotation.Value == null ||
               annotation.Value.ExpressionKind != EdmExpressionKind.Record)
            {
                return false;
            }

            IEdmRecordExpression record = (IEdmRecordExpression)annotation.Value;

            // Searchable
            Searchable = record.GetBoolean("Searchable");

            // read the "UnsupportedExpressions"
            IEdmPropertyConstructor property = record.Properties.FirstOrDefault(e => e.Name == "UnsupportedExpressions");
            if (property != null)
            {
                IEdmEnumMemberExpression value = property.Value as IEdmEnumMemberExpression;
                if (value != null && value.EnumMembers != null)
                {
                    SearchExpressions result;
                    foreach (var v in value.EnumMembers)
                    {
                        if (Enum.TryParse(v.Name, out result))
                        {
                            if (UnsupportedExpressions == null)
                            {
                                UnsupportedExpressions = result;
                            }
                            else
                            {
                                UnsupportedExpressions = UnsupportedExpressions | result;
                            }
                        }
                    }
                }
            }

            return true;
        }
    }
}
