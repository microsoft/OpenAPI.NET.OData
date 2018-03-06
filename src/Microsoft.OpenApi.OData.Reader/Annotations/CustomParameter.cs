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
    /// Complex type Org.OData.Core.V1.CustomParameter
    /// </summary>
    internal class CustomParameter
    {
        /// <summary>
        /// The Name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The Description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The DocumentationURL.
        /// </summary>
        public string DocumentationURL { get; set; }

        /// <summary>
        /// The Required.
        /// </summary>
        public bool? Required { get; set; }

        /// <summary>
        /// The ExampleValues.
        /// </summary>
        public IEnumerable<Example> ExampleValues { get; set; }

        /// <summary>
        /// Init the <see cref="CustomParameter"/>
        /// </summary>
        /// <param name="record">The input record.</param>
        public virtual void Init(IEdmRecordExpression record)
        {
            Utils.CheckArgumentNull(record, nameof(record));

            // Name
            Name = record.GetString("Name");

            // Description
            Description = record.GetString("Description");

            // DocumentationURL
            DocumentationURL = record.GetString("DocumentationURL");

            // Required
            Required = record.GetBoolean("Required");

            // ExampleValues
            ExampleValues = record.GetCollection("ExampleValues", r =>
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
