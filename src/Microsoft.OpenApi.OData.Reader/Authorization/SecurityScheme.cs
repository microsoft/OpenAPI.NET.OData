// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Capabilities;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.OpenApi.OData.Authorization
{
    /// <summary>
    /// Constant values for Authorization Vocabulary
    /// </summary>
    internal class SecurityScheme
    {
        public string AuthorizationSchemeName { get; set; }

        public IList<string> RequiredScopes { get; set; }

        public void Init(IEdmRecordExpression record)
        {
            AuthorizationSchemeName = record.GetString("Name");

            IEdmCollectionExpression collection = GetCollection(record, "RequiredScopes");
            if (collection != null)
            {
                RequiredScopes = new List<string>();
                foreach (var item in collection.Elements)
                {
                    IEdmStringConstantExpression itemRecord = item as IEdmStringConstantExpression;
                    RequiredScopes.Add(itemRecord.Value);
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
    }
}