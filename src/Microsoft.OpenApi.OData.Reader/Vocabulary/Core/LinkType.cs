// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using System;

namespace Microsoft.OpenApi.OData.Vocabulary.Core
{
    /// <summary>
    /// Complex Type: Org.OData.Core.V1.Link
    /// </summary>
    [Term("Org.OData.Core.V1.Links")]
    internal class LinkType : IRecord
    {
        /// <summary>
        /// The link relation type.
        /// </summary>
        public string Rel { get; private set; }

        /// <summary>
        /// The link to the documentation.
        /// </summary>
        public Uri Href { get; private set; }

        /// <summary>
        /// Init the <see cref="LinkType"/>.
        /// </summary>
        /// <param name="record"></param>
        public virtual void Initialize(IEdmRecordExpression record)
        {
            Utils.CheckArgumentNull(record, nameof(record));
            Rel = record.GetString("rel");
            Href = new Uri(record.GetString("href"));
        }
    }
}
