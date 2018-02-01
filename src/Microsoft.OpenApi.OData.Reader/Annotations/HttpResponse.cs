// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Capabilities;
using System.Collections.Generic;

namespace Microsoft.OpenApi.OData.Annotations
{
    /// <summary>
    /// </summary>
    internal class HttpResponse
    {
        /// <summary>
        /// </summary>
        public string ResponseCode { get; set; }

        /// <summary>
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// </summary>
        public IList<HttpStatusCodeCondition> Conditions { get; set; }

        public void Init(IEdmRecordExpression record)
        {
            ResponseCode = record.GetString("ResponseCode");
            Description = record.GetString("Description");

            IEdmCollectionExpression collection = HttpRequest.GetCollection(record, "Conditions");
            if (collection != null)
            {
                Conditions = new List<HttpStatusCodeCondition>();
                foreach (var item in collection.Elements)
                {
                    HttpStatusCodeCondition p = new HttpStatusCodeCondition();
                    p.Init(item as IEdmRecordExpression);
                    Conditions.Add(p);
                }
            }
        }
    }

    internal class HttpStatusCodeCondition
    {
        public string ODataErrorCode { get; set; }

        /// <summary>
        /// </summary>
        public string Description { get; set; }

        public void Init(IEdmRecordExpression record)
        {
            ODataErrorCode = record.GetString("ODataErrorCode");
            Description = record.GetString("Description");
        }
    }
}
