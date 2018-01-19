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
        private bool _filterable = true;
        private bool? _requiresFilter;
        private IList<string> _requiredProperties;
        private IList<string> _nonFilterableProperties;

        /// <summary>
        /// The Term type name.
        /// </summary>
        public override string QualifiedName => CapabilitiesConstants.FilterRestrictions;

        /// <summary>
        /// Gets the Filterable value.
        /// </summary>
        public bool Filterable
        {
            get
            {
                Initialize();
                return _filterable;
            }
        }

        /// <summary>
        /// Gets the RequiresFilter value.
        /// </summary>
        public bool? RequiresFilter
        {
            get
            {
                Initialize();
                return _requiresFilter;
            }
        }

        /// <summary>
        /// Gets the properties which must be specified in the $filter clause.
        /// </summary>
        public IList<string> RequiredProperties
        {
            get
            {
                Initialize();
                return _requiredProperties;
            }
        }

        /// <summary>
        /// Gets the properties which cannot be used in $filter expressions.
        /// </summary>
        public IList<string> NonFilterableProperties
        {
            get
            {
                Initialize();
                return _nonFilterableProperties;
            }
        }

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
        /// Test the input property which must be specified in the $filter clause.
        /// </summary>
        /// <param name="property">The input property.</param>
        /// <returns>True/False.</returns>
        public bool IsRequiredProperty(IEdmProperty property)
        {
            return RequiredProperties.Any(a => a == property.Name);
        }

        /// <summary>
        /// Test the input property which cannot be used in $filter expressions.
        /// </summary>
        /// <param name="property">The input property.</param>
        /// <returns>True/False.</returns>
        public bool IsNonFilterableProperty(IEdmProperty property)
        {
            return NonFilterableProperties.Any(a => a == property.Name);
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

            _filterable = SetBoolProperty(record, "Filterable", true);

            _requiresFilter = SetBoolProperty(record, "RequiresFilter");

            _requiredProperties = GetCollectProperty(record, "RequiredProperties");

            _nonFilterableProperties = GetCollectNavigationProperty(record, "NonFilterableProperties");
        }
    }
}
