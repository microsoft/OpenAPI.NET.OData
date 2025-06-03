// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.Vocabulary.Authorization
{
    /// <summary>
    /// Enum type KeyLocation
    /// </summary>
    internal enum KeyLocation
    {
        /// <summary>
        /// API Key is passed in the header.
        /// </summary>
        Header,

        /// <summary>
        /// API Key is passed as a query option.
        /// </summary>
        QueryOption,

        /// <summary>
        /// API Key is passed as a cookie.
        /// </summary>
        Cookie
    }

    /// <summary>
    /// Complex type: 'Org.OData.Authorization.V1.ApiKey'
    /// </summary>
    internal class ApiKey : Authorization
    {
        /// <summary>
        /// The name of the header or query parameter.
        /// </summary>
        public string? KeyName { get; set; }

        /// <summary>
        /// Whether the API Key is passed in the header or as a query option.
        /// </summary>
        public KeyLocation? Location { get; set; }

        /// <summary>
        /// Gets the security scheme type.
        /// </summary>
        public override SecuritySchemeType SchemeType => SecuritySchemeType.ApiKey;

        /// <summary>
        /// Init <see cref="ApiKey"/>.
        /// </summary>
        /// <param name="record">the input record.</param>
        public override void Initialize(IEdmRecordExpression record)
        {
            // base checked.
            base.Initialize(record);

            // KeyName.
            KeyName = record.GetString("KeyName");

            // Location.
            Location = record.GetEnum<KeyLocation>("Location");
        }
    }
}