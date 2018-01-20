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
        private bool _searchable = true;
        private SearchExpressions? _unsupportedExpressions;

        /// <summary>
        /// The Term type name.
        /// </summary>
        public override string QualifiedName => CapabilitiesConstants.SearchRestrictions;

        /// <summary>
        /// Gets the Searchable value.
        /// </summary>
        public bool Searchable
        {
            get
            {
                Initialize();
                return _searchable;
            }
        }

        /// <summary>
        /// Gets the search expressions which can is supported in $search.
        /// </summary>
        public SearchExpressions? UnsupportedExpressions
        {
            get
            {
                Initialize();
                return _unsupportedExpressions;
            }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="SearchRestrictions"/> class.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="target">The Edm annotation target.</param>
        public SearchRestrictions(IEdmModel model, IEdmVocabularyAnnotatable target)
            : base(model, target)
        {
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

            _searchable = SetBoolProperty(record, "Searchable", true);

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
                        if (_unsupportedExpressions == null)
                        {
                            _unsupportedExpressions = SearchExpressions.none;
                        }

                        if (Enum.TryParse(v.Name, out result))
                        {
                            _unsupportedExpressions = _unsupportedExpressions | result;
                        }
                    }
                }
            }
        }
    }
}
