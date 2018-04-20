// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.Annotations
{
    /// <summary>
    /// Org.OData.Core.V1.Example
    /// </summary>
    internal abstract class Example
    {
        /// <summary>
        /// Description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Init the <see cref="Example"/>.
        /// </summary>
        /// <param name="record">The input record.</param>
        public virtual void Init(IEdmRecordExpression record)
        {
            Utils.CheckArgumentNull(record, nameof(record));

            // Description
            Description = record.GetString("Description");
        }

        /// <summary>
        /// Creat the corresponding example object.
        /// </summary>
        /// <param name="record">The input record.</param>
        /// <returns>The created example object.</returns>
        public static Example CreateExample(IEdmRecordExpression record)
        {
            if (record == null || record.DeclaredType == null)
            {
                return null;
            }

            IEdmComplexType complexType = record.DeclaredType.Definition as IEdmComplexType;
            if (complexType == null)
            {
                return null;
            }

            Example example = null;
            switch (complexType.FullTypeName())
            {
                case "Org.OData.Core.V1.ExternalExample":
                    example = new ExternalExample();
                    break;

                case "Org.OData.Core.V1.InlineExample":
                    example = new InlineExample();
                    break;

                default:
                    break;
            }

            if (example != null)
            {
                example.Init(record);
            }

            return example;
        }
    }
}
