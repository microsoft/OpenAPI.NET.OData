// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Common;

namespace Microsoft.OpenApi.OData.Annotations
{
    /// <summary>
    /// Org.Graph.Vocab.HttpRequests
    /// </summary>
    internal abstract class Example
    {
      //  public string Value { get; set; }

        public string Description { get; set; }

        public virtual void Init(IEdmRecordExpression record)
        {
       //     Value = record.GetString("Value");
            Description = record.GetString("Description");
        }

        public static Example CreateExample(IEdmRecordExpression record)
        {
            if (record.DeclaredType == null)
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
            }

            if (example != null)
            {
                example.Init(record);
            }

            return example;
        }
    }

    internal class ExternalExample : Example
    {
        public string ExternalValue { get; set; }

        public override void Init(IEdmRecordExpression record)
        {
            ExternalValue = record.GetString("ExternalValue");

            base.Init(record);
        }
    }

    internal class InlineExample : Example
    {
        public string InlineValue { get; set; }

        public override void Init(IEdmRecordExpression record)
        {
            InlineValue = record.GetString("InlineValue");

            base.Init(record);
        }
    }
}
