// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Common;

namespace Microsoft.OpenApi.OData.Annotations
{
    /// <summary>
    /// </summary>
    internal class HttpResponse
    {
        /// <summary>
        /// </summary>
        public string Description { get; set; }

        public string ResponseCode { get; set; }

        /// <summary>
        /// </summary>
        public IList<Example> Examples { get; set; }

     //   public bool? Required { get; set; }

        /// <summary>
        /// </summary>
       // public IList<MediaType> ResponseContent { get; set; }

        public void Init(IEdmRecordExpression record)
        {
            ResponseCode = record.GetString("ResponseCode");
            Description = record.GetString("Description");
     //       Required = record.GetBoolean("Required");

            IEdmCollectionExpression collection = HttpRequest.GetCollection(record, "Examples");
            if (collection != null)
            {
                Examples = new List<Example>();
                foreach (var item in collection.Elements)
                {
                    IEdmRecordExpression itemRecord = item as IEdmRecordExpression;
                    if (itemRecord == null)
                    {
                        continue;
                    }

                    Example ex = Example.CreateExample(itemRecord);
                    if (ex != null)
                    {
                        Examples.Add(ex);
                    }
                }
            }
        }
    }
    /*
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
    }*/
}
