// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;

namespace Microsoft.OpenApi.OData.Capabilities
{
    /// <summary>
    /// Base class the supported restrictions.
    /// </summary>
    internal abstract class SupportedRestrictions : CapabilitiesRestrictions
    {
        private bool _supported = true;

        /// <summary>
        /// Get the Supported boolean value.
        /// </summary>
        public bool Supported
        {
            get
            {
                Initialize();
                return _supported;
            }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="SupportedRestrictions"/> class.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="target">The Edm annotation target.</param>
        public SupportedRestrictions(IEdmModel model, IEdmVocabularyAnnotatable target)
            : base(model, target)
        {
        }

        protected override void Initialize(IEdmVocabularyAnnotation annotation)
        {
            if (annotation == null ||
                annotation.Value == null ||
                annotation.Value.ExpressionKind != EdmExpressionKind.BooleanConstant)
            {
                return;
            }

            IEdmBooleanConstantExpression boolConstant = (IEdmBooleanConstantExpression)annotation.Value;
            if (boolConstant != null)
            {
                _supported = boolConstant.Value;
            }
        }
    }
}
