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

        public bool? Required { get; set; }

        /// <summary>
        /// </summary>
        public IList<MediaType> ResponseContent { get; set; }

        public void Init(IEdmRecordExpression record)
        {
            ResponseCode = record.GetString("ResponseCode");
            Description = record.GetString("Description");
            Required = record.GetBoolean("Required");

            IEdmCollectionExpression collection = HttpRequest.GetCollection(record, "ResponseContent");
            if (collection != null)
            {
                ResponseContent = new List<MediaType>();
                foreach (var item in collection.Elements)
                {
                    MediaType p = new MediaType();
                    p.Init(item as IEdmRecordExpression);
                    ResponseContent.Add(p);
                }
            }
        }
    }

    internal class MediaType
    {
        public string Name { get; set; }

        /// <summary>
        /// </summary>
        public string Type { get; set; }

        public IList<CustomParameterExampleValue> ExampleValues { get; set; }

        public void Init(IEdmRecordExpression record)
        {
            Name = record.GetString("Name");
            Type = record.GetString("Type");

            IEdmCollectionExpression collection = HttpRequest.GetCollection(record, "ExampleValues");
            if (collection != null)
            {
                ExampleValues = new List<CustomParameterExampleValue>();
                foreach (var item in collection.Elements)
                {
                    CustomParameterExampleValue p = new CustomParameterExampleValue();
                    p.Init(item as IEdmRecordExpression);
                    ExampleValues.Add(p);
                }
            }
        }
    }
}
