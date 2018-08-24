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
        /// <summary>
        /// Get the Supported boolean value.
        /// </summary>
        public bool? Supported { get; private set; }

        /// <summary>
        /// Test the target supports the corresponding restriction.
        /// </summary>
        /// <returns>True/false.</returns>
        public bool IsSupported => Supported == null || Supported.Value == true;

        protected override bool Initialize(IEdmVocabularyAnnotation annotation)
        {
            if (annotation == null ||
                annotation.Value == null ||
                annotation.Value.ExpressionKind != EdmExpressionKind.BooleanConstant)
            {
                return false;
            }

            // supported
            IEdmBooleanConstantExpression boolConstant = (IEdmBooleanConstantExpression)annotation.Value;
            if (boolConstant != null)
            {
                Supported = boolConstant.Value;
            }

            return true;
        }
    }
}
