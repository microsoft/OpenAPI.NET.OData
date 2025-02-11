// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;

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
        public const string ApplicationJsonMediaType = "application/json";

        /// <summary>
        /// application/xml
        /// </summary>
        public const string ApplicationXmlMediaType = "application/xml";

        /// <summary>
        /// application/octet-stream
        /// </summary>
        public const string ApplicationOctetStreamMediaType = "application/octet-stream";

        /// <summary>
        /// Status code class: 2XX
        /// </summary>
        public const string StatusCodeClass2XX = "2XX";

        /// <summary>
        /// Status code: 200
        /// </summary>
        public const string StatusCode200 = "200";

        /// <summary>
        /// Status code: 201
        /// </summary>
        public const string StatusCode201 = "201";

        /// <summary>
        /// Status code: 204
        /// </summary>
        public const string StatusCode204 = "204";

        /// <summary>
        /// Status code: default
        /// </summary>
        public const string StatusCodeDefault = "default";

        /// <summary>
        /// Status code class: 4XX
        /// </summary>
        public const string StatusCodeClass4XX = "4XX";

        /// <summary>
        /// Status code class: 5XX
        /// </summary>
        public const string StatusCodeClass5XX = "5XX";

        /// <summary>
        /// Edm model error extension key.
        /// </summary>
        public const string xMsEdmModelError = "x-ms-edm-error-";

        /// <summary>
        /// extension for toc (table of content) type
        /// </summary>
        public const string xMsTocType = "x-ms-docs-toc-type";

        /// <summary>
        /// extension for key type
        /// </summary>
        public const string xMsKeyType = "x-ms-docs-key-type";

        /// <summary>
        /// extension for operation type
        /// </summary>
        public const string xMsDosOperationType = "x-ms-docs-operation-type";

        /// <summary>
        /// extension for group type
        /// </summary>
        public const string xMsDosGroupPath = "x-ms-docs-grouped-path";

        /// <summary>
        /// extension for paging
        /// </summary>
        public const string xMsPageable = "x-ms-pageable";

        /// <summary>
        /// extension for discriminator value support
        /// </summary>
        public const string xMsDiscriminatorValue = "x-ms-discriminator-value";

        /// <summary>
        /// extension for navigation property
        /// </summary>
        public const string xMsNavigationProperty = "x-ms-navigationProperty";

        /// <summary>
        /// Name used for the OpenAPI referenced schema for OData Count operations responses.
        /// </summary>
        public const string DollarCountSchemaName = "ODataCountResponse";

        /// <summary>
        /// Suffix used for collection response schemas.
        /// </summary>
        public const string CollectionSchemaSuffix = "CollectionResponse";

        /// <summary>
        /// Suffix used for the base collection pagination response schema and count response schemas.
        /// </summary>
        public const string BaseCollectionPaginationCountResponse = "BaseCollectionPaginationCountResponse";

        /// <summary>
        /// Suffix used for the base delta function response schemas.
        /// </summary>
        public const string BaseDeltaFunctionResponse = "BaseDeltaFunctionResponse";

        /// <summary>
        /// Name used for reference update.
        /// </summary>
        public const string ReferenceUpdateSchemaName = "ReferenceUpdate";

        /// <summary>
        /// Name used for reference update.
        /// </summary>
        public const string ReferenceCreateSchemaName = "ReferenceCreate";

        /// <summary>
        /// Name used for reference request POST body.
        /// </summary>
        public const string ReferencePostRequestBodyName = "refPostBody";

        /// <summary>
        /// Name used for reference request PUT body.
        /// </summary>
        public const string ReferencePutRequestBodyName = "refPutBody";

        /// <summary>
        /// Name used to reference INF, -INF and NaN
        /// </summary>
        public const string ReferenceNumericName = "ReferenceNumeric";
        
        /// <summary>
        /// The odata type name.
        /// </summary>
        public const string OdataType = "@odata.type";

        /// <summary>
        /// The odata id.
        /// </summary>
        public const string OdataId = "@odata.id";

        /// <summary>
        /// object type
        /// </summary>
        public const string ObjectType = "object";

        /// <summary>
        /// string type
        /// </summary>
        public const string StringType = "string";

        /// <summary>
        /// number type
        /// </summary>
        public const string NumberType = "number";

        /// <summary>
        /// int64 format
        /// </summary>
        public const string Int64Format = "int64";

        /// <summary>
        /// decimal format
        /// </summary>
        public const string DecimalFormat = "decimal";

        /// <summary>
        /// entity name
        /// </summary>
        public const string EntityName = "entity";

        /// <summary>
        /// count segment identifier
        /// </summary>
        public const string CountSegmentIdentifier = "count";

        /// <summary>
        /// content string
        /// </summary>
        public const string Content = "content";

        /// <summary>
        /// Success string
        /// </summary>
        public const string Success = "Success";

        /// <summary>
        /// Created string
        /// </summary>
        public const string Created = "Created";

        /// <summary>
        /// error string
        /// </summary>
        public const string Error = "error";
    }
}
