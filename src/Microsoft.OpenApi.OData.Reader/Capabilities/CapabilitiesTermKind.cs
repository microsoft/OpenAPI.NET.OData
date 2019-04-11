// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.OpenApi.OData.Capabilities
{
    /// <summary>
    /// The Enum kind for Capabiliites annotation
    /// </summary>
    internal enum CapabilitesTermKind
    {
        /// <summary>
        /// Media types of supported formats, including format parameters
        /// </summary>
        SupportedFormats,

        /// <summary>
        /// List of acceptable compression methods for ($batch) requests, e.g. gzip
        /// </summary>
        AcceptableEncodings,

        /// <summary>
        /// Supports key values according to OData URL conventions
        /// </summary>
        IndexableByKey,

        /// <summary>
        /// Supported odata.isolation levels
        /// </summary>
        Isolation,

        /// <summary>
        /// Supports key as segment
        /// </summary>
        KeyAsSegmentSupported,

        /// <summary>
        /// Supports $top
        /// </summary>
        TopSupported,

        /// <summary>
        /// Supports $skip
        /// </summary>
        SkipSupported,

        /// <summary>
        /// Service supports the asynchronous request preference
        /// </summary>
        AsynchronousRequestsSupported,

        /// <summary>
        /// Supports $batch requests
        /// </summary>
        BatchSupported,

        /// <summary>
        /// Service supports the continue on error preference
        /// </summary>
        BatchContinueOnErrorSupported,

        /// <summary>
        /// List of functions supported in $filter
        /// </summary>
        FilterFunctions,

        /// <summary>
        /// Supports callbacks for the specified protocols
        /// </summary>
        CallbackSupported,

        /// <summary>
        /// Supports cross joins for the entity sets in this container
        /// </summary>
        CrossJoinSupported,

        /// <summary>
        /// Change tracking capabilities of this service or entity set
        /// </summary>
        ChangeTracking,

        /// <summary>
        /// Restrictions on /$count path suffix and $count=true system query option
        /// </summary>
        CountRestrictions,

        /// <summary>
        /// Restrictions on navigating properties according to OData URL conventions
        /// </summary>
        NavigationRestrictions,

        /// <summary>
        /// Restrictions on $filter expressions
        /// </summary>
        FilterRestrictions,

        /// <summary>
        /// Restrictions on $orderby expressions
        /// </summary>
        SortRestrictions,

        /// <summary>
        /// Restrictions on $expand expressions
        /// </summary>
        ExpandRestrictions,

        /// <summary>
        /// Restrictions on $search expressions
        /// </summary>
        SearchRestrictions,

        /// <summary>
        /// Restrictions on insert operations
        /// </summary>
        InsertRestrictions,

        /// <summary>
        /// Deep Insert Support of the annotated resource (the whole service, an entity set, or a collection-valued resource
        /// </summary>
        DeepInsertSupport,

        /// <summary>
        /// Restrictions on update operations
        /// </summary>
        UpdateRestrictions,

        /// <summary>
        /// Deep Update Support of the annotated resource (the whole service, an entity set, or a collection-valued resource)
        /// </summary>
        DeepUpdateSupported,

        /// <summary>
        /// Restrictions on delete operations
        /// </summary>
        DeleteRestrictions,

        /// <summary>
        /// Describes restrictions on operations applied to collection-valued structural properties.
        /// </summary>
        CollectionPropertyRestrictions,

        /// <summary>
        /// Restrictions for function or action operation.
        /// </summary>
        OperationRestrictions,

        /// <summary>
        /// Restrictions for retrieving a collection of entities, retrieving a singleton instance, invoking a function
        /// </summary>
        ReadRestrictions,

        /// <summary>
        /// Custom headers that are supported/required for the annotated resource
        /// </summary>
        CustomHeaders,

        /// <summary>
        /// Custom query options that are supported/required for the annotated resource
        /// </summary>
        CustomQueryOptions
    }
}
