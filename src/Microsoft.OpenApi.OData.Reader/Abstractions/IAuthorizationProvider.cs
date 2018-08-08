// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;

namespace Microsoft.OpenApi.OData.Abstractions
{
    /// <summary>
    /// The <see cref="IAuthorization"/> provider interface.
    /// </summary>
    public interface IAuthorizationProvider
    {
        /// <summary>
        /// Provide the <see cref="IAuthorization"/>.
        /// </summary>
        /// <returns>The <see cref="IAuthorization"/>.</returns>
        //IEnumerable<IAuthorization> GetAuthorizations(IEdmModel model, IEdmVocabularyAnnotatable target);
    }
}
