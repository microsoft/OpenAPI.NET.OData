// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm.Vocabularies;

namespace Microsoft.OpenApi.OData.Vocabulary
{
    /// <summary>
    /// The interface for <see cref="IEdmRecordExpression"/>
    /// </summary>
    internal interface IRecord
    {
        /// <summary>
        /// Initialize the instance using <see cref="IEdmRecordExpression"/>.
        /// </summary>
        /// <param name="record">The Edm record expression.</param>
        void Initialize(IEdmRecordExpression record);
    }
}
