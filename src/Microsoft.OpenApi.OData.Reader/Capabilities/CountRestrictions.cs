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
        private bool _countable = true;
        private IList<string> _nonCountableProperties = new List<string>();
        private IList<string> _nonCountableNavigationProperties = new List<string>();

        /// <summary>
        /// The Term type name.
        /// </summary>
        public override string QualifiedName => CapabilitiesConstants.CountRestrictions;

        /// <summary>
        /// Gets the Countable value.
        /// <Property Name="Countable" Type="Edm.Boolean" DefaultValue="true">
        /// </summary>
        public bool Countable
        {
            get
            {
                Initialize();
                return _countable;
            }
        }

        /// <summary>
        /// Gets the properties which do not allow /$count segments.
        /// </summary>
        public IList<string> NonCountableProperties
        {
            get
            {
                Initialize();
                return _nonCountableProperties;
            }
        }

        /// <summary>
        /// Gets the navigation properties which do not allow /$count segments.
        /// </summary>
        public IList<string> NonCountableNavigationProperties
        {
            get
            {
                Initialize();
                return _nonCountableNavigationProperties;
            }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="CountRestrictions"/> class.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="target">The Edm annotation target.</param>
        public CountRestrictions(IEdmModel model, IEdmVocabularyAnnotatable target)
            : base(model, target)
        {
        }

        /// <summary>
        /// Test the input property which do not allow /$count segments.
        /// </summary>
        /// <param name="property">The input property.</param>
        /// <returns>True/False.</returns>
        public bool IsNonCountableNavigationProperty(IEdmProperty property)
        {
            return NonCountableProperties.Any(a => a == property.Name);
        }

        /// <summary>
        /// Test the input navigation property which do not allow /$count segments.
        /// </summary>
        /// <param name="property">The input property.</param>
        /// <returns>True/False.</returns>
        public bool IsNonCountableNavigationProperty(IEdmNavigationProperty property)
        {
            return NonCountableNavigationProperties.Any(a => a == property.Name);
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

            _countable = SetBoolProperty(record, "Countable", true);

            _nonCountableProperties = GetCollectProperty(record, "NonCountableProperties");

            _nonCountableNavigationProperties = GetCollectNavigationProperty(record, "NonCountableNavigationProperties");
        }
    }
}
