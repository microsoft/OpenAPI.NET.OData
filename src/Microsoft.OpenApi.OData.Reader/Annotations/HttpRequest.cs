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
    internal class HttpRequest
    {
        /// <summary>
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// </summary>
        public IList<CustomParameter> CustomQueryOptions { get; set; }

        /// <summary>
        /// Gets the navigation properties which do not allow /$count segments.
        /// </summary>
        public IList<CustomParameter> CustomHeaders { get; set; }

        public HttpRequestBody RequestBody { get; set; }

        public IList<HttpResponse> HttpResponse { get; set; }

        public void Init(IEdmRecordExpression record)
        {
            Description = record.GetString("Description");
            Method = record.GetString("Method");

            IEdmCollectionExpression collection = GetCollection(record, "CustomQueryOptions");
            if (collection != null)
            {
                CustomQueryOptions = new List<CustomParameter>();
                foreach(var item in collection.Elements)
                {
                    CustomParameter p = new CustomParameter();
                    p.Init(item as IEdmRecordExpression);
                    CustomQueryOptions.Add(p);
                }
            }

            collection = GetCollection(record, "CustomHeaders");
            if (collection != null)
            {
                CustomHeaders = new List<CustomParameter>();
                foreach (var item in collection.Elements)
                {
                    CustomParameter p = new CustomParameter();
                    p.Init(item as IEdmRecordExpression);
                    CustomHeaders.Add(p);
                }
            }

            IEdmRecordExpression requestBodyRecord = GetRecord(record, "RequestBody");
            if (collection != null)
            {
                RequestBody = new HttpRequestBody();
                RequestBody.Init(requestBodyRecord);
            }

            collection = GetCollection(record, "HttpResponse");
            if (collection != null)
            {
                HttpResponse = new List<HttpResponse>();
                foreach (var item in collection.Elements)
                {
                    HttpResponse p = new HttpResponse();
                    p.Init(item as IEdmRecordExpression);
                    HttpResponse.Add(p);
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
