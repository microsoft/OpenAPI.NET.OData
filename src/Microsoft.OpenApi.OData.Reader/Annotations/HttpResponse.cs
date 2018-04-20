// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.Annotations
{
    /// <summary>
    /// The Org.OData.Core.v1.HttpResponse
    /// </summary>
    internal class HttpResponse
    {
        /// <summary>
        /// Description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// ResponseCode
        /// </summary>
        public string ResponseCode { get; set; }

        /// <summary>
        /// Examples
        /// </summary>
        public IEnumerable<Example> Examples { get; set; }

        /// <summary>
        /// Int the <see cref="HttpResponse"/>.
        /// </summary>
        /// <param name="record">The input record.</param>
        public void Init(IEdmRecordExpression record)
        {
            // ResponseCode
            ResponseCode = record.GetString("ResponseCode");

            // Description
            Description = record.GetString("Description");

            // Examples
            Examples = record.GetCollection("Examples", r =>
            {
                IEdmRecordExpression itemRecord = r as IEdmRecordExpression;
                if (itemRecord != null)
                {
                    return Example.CreateExample(itemRecord);
                }

                return null;
            });
        }
    }
}
