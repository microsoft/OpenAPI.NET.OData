// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.OpenApi.OData.Common
{
    /// <summary>
    /// Constant strings
    /// </summary>
    internal static class Constants
    {
        /// <summary>
        /// application/json
        /// </summary>
        public static string ApplicationJsonMediaType = "application/json";

        /// <summary>
        /// application/xml
        /// </summary>
        public static string ApplicationXmlMediaType = "application/xml";

        /// <summary>
        /// application/octet-stream
        /// </summary>
        public static string ApplicationOctetStreamMediaType = "application/octet-stream";

        /// <summary>
        /// Status code: 200
        /// </summary>
        public static string StatusCode200 = "200";

        /// <summary>
        /// Status code: 201
        /// </summary>
        public static string StatusCode201 = "201";

        /// <summary>
        /// Status code: 204
        /// </summary>
        public static string StatusCode204 = "204";

        /// <summary>
        /// Status code: default
        /// </summary>
        public static string StatusCodeDefault = "default";

        /// <summary>
        /// Edm model error extension key.
        /// </summary>
        public static string xMsEdmModelError = "x-ms-edm-error-";

        /// <summary>
        /// extension for toc (table of content) type
        /// </summary>
        public static string xMsTocType = "x-ms-docs-toc-type";

        /// <summary>
        /// extension for key type
        /// </summary>
        public static string xMsKeyType = "x-ms-docs-key-type";

        /// <summary>
        /// extension for operation type
        /// </summary>
        public static string xMsDosOperationType = "x-ms-docs-operation-type";

        /// <summary>
        /// extension for group type
        /// </summary>
        public static string xMsDosGroupPath = "x-ms-docs-grouped-path";

        /// <summary>
        /// extension for paging
        /// </summary>
        public static string xMsPageable = "x-ms-pageable";

        /// <summary>
        /// extension for discriminator value support
        /// </summary>
        public static string xMsDiscriminatorValue = "x-ms-discriminator-value";
    }
}
