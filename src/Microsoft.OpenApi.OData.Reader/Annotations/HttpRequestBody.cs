// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Capabilities;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.OpenApi.OData.Annotations
{
    /// <summary>
    /// Org.Graph.Vocab.HttpRequests
    /// </summary>
    internal class HttpRequestBody
    {
        /// <summary>
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// </summary>
        public IList<CustomParameter> Parameters { get; set; }

        public void Init(IEdmRecordExpression record)
        {
            Description = record.GetString("Description");

            IEdmCollectionExpression collection = GetCollection(record, "Parameters");
            if (collection != null)
            {
                Parameters = new List<CustomParameter>();
                foreach(var item in collection.Elements)
                {
                    CustomParameter p = new CustomParameter();
                    p.Init(item as IEdmRecordExpression);
                    Parameters.Add(p);
                }
            }
        }

        public static IEdmCollectionExpression GetCollection(IEdmRecordExpression record, string propertyName)
        {
            IEdmPropertyConstructor property = record.Properties.FirstOrDefault(e => e.Name == propertyName);
            if (property != null)
            {
                return property.Value as IEdmCollectionExpression;
            }

            return null;
        }

        public static IEdmRecordExpression GetRecord(IEdmRecordExpression record, string propertyName)
        {
            IEdmPropertyConstructor property = record.Properties.FirstOrDefault(e => e.Name == propertyName);
            if (property != null)
            {
                return property.Value as IEdmRecordExpression;
            }

            return null;
        }
    }
}
