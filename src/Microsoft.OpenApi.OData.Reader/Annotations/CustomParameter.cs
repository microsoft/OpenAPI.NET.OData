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
    internal class CustomParameter
    {
        /// <summary>
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// </summary>
        public string DocumentationURL { get; set; }

        public bool? Required { get; set; }

        public IList<CustomParameterExampleValue> ExampleValues { get; set; }

        public void Init(IEdmRecordExpression record)
        {
            Name = record.GetString("Name");
            Type = record.GetString("Type");
            Description = record.GetString("Description");
            DocumentationURL = record.GetString("DocumentationURL");
            Required = record.GetBoolean("Required");

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
