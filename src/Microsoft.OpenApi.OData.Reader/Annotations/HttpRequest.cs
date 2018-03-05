// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Authorizations;
using Microsoft.OpenApi.OData.Common;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.OpenApi.OData.Annotations
{
    /// <summary>
    /// Org.OData.Core.V1.HttpRequest
    /// </summary>
    internal class HttpRequest
    {
        /// <summary>
        /// The description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The Method.
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// The Custom Query Options.
        /// </summary>
        public IList<CustomParameter> CustomQueryOptions { get; set; }

        /// <summary>
        /// The custom Headers.
        /// </summary>
        public IList<CustomParameter> CustomHeaders { get; set; }

        /// <summary>
        /// The http responses.
        /// </summary>
        public IList<HttpResponse> HttpResponses { get; set; }

        /// <summary>
        /// The security sechems.
        /// </summary>
        public IList<SecurityScheme> SecuritySchemes { get; set; }

        /// <summary>
        /// Init the <see cref="HttpRequest"/>.
        /// </summary>
        /// <param name="record">The input record.</param>
        public virtual void Init(IEdmRecordExpression record)
        {
            Utils.CheckArgumentNull(record, nameof(record));

            // Description.
            Description = record.GetString("Description");

            // Method.
            Method = record.GetString("Method");

            // CustomQueryOptions
            CustomQueryOptions = record.GetCollection<CustomParameter>("CustomQueryOptions", (s, r) => s.Init(r as IEdmRecordExpression));

            // CustomHeaders
            CustomHeaders = record.GetCollection<CustomParameter>("CustomHeaders", (s, r) => s.Init(r as IEdmRecordExpression));

            // HttpResponses
            HttpResponses = record.GetCollection<HttpResponse>("HttpResponses", (s, r) => s.Init(r as IEdmRecordExpression));

            // SecuritySchemes
            SecuritySchemes = record.GetCollection<SecurityScheme>("SecuritySchemes", (s, r) => s.Init(r as IEdmRecordExpression));
        }
    }
}
