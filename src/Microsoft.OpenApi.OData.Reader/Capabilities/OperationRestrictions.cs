// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;

namespace Microsoft.OpenApi.OData.Capabilities
{
    /// <summary>
    /// Org.OData.Capabilities.V1.OperationRestrictions
    /// </summary>
    internal class OperationRestrictions : CapabilitiesRestrictions
    {
        /// <summary>
        /// The Term type name.
        /// </summary>
        public override CapabilitesTermKind Kind => CapabilitesTermKind.OperationRestrictions;

        /// <summary>
        /// Gets the List of required scopes to invoke an action or function.
        /// </summary>
        public PermissionType Permission { get; private set; }

        /// <summary>
        /// Gets the Supported or required custom headers.
        /// </summary>
        public IList<CustomParameter> CustomHeaders { get; private set; }

        /// <summary>
        /// Gets the Supported or required custom query options.
        /// </summary>
        public IList<CustomParameter> CustomQueryOptions { get; private set; }

        protected override bool Initialize(IEdmVocabularyAnnotation annotation)
        {
            if (annotation == null ||
                annotation.Value == null ||
                annotation.Value.ExpressionKind != EdmExpressionKind.Record)
            {
                return false;
            }

            IEdmRecordExpression record = (IEdmRecordExpression)annotation.Value;


            return true;
        }
    }
}
