// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;

namespace Microsoft.OpenApi.OData.Capabilities
{
    internal abstract class ReadRestrictionsBase
    {
        /// <summary>
        /// Get the Entities can be retrieved.
        /// </summary>
        public bool? Readable { get; private set; }

        /// <summary>
        /// Gets the List of required scopes to invoke an action or function
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
    }

    /// <summary>
    /// Restrictions for retrieving an entity by key
    /// </summary>
    internal class ReadByKeyRestrictionsType : ReadRestrictionsBase
    {

    }

    /// <summary>
    /// Restrictions for retrieving an entity by key
    /// </summary>
    internal class ReadRestrictionsType : ReadRestrictionsBase
    {
        /// <summary>
        /// Gets the Restrictions for retrieving an entity by key.
        /// Only valid when applied to a collection. If a property of `ReadByKeyRestrictions`
        /// is not specified, the corresponding property value of `ReadRestrictions` applies.
        /// </summary>
        public ReadByKeyRestrictionsType ReadByKeyRestrictions { get; private set; }
    }

    /// <summary>
    /// Org.OData.Capabilities.V1.ReadRestrictions
    /// </summary>
    internal class ReadRestrictions : CapabilitiesRestrictions
    {
        /// <summary>
        /// The Term type name.
        /// </summary>
        public override CapabilitesTermKind Kind => CapabilitesTermKind.ReadRestrictions;

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
