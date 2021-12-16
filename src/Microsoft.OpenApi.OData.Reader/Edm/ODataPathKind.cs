// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.OpenApi.OData.Edm
{
    /// <summary>
    /// Enum types for Edm path.
    /// </summary>
    public enum ODataPathKind
    {
        /// <summary>
        /// Represents an entity set path. for example: ~/users
        /// </summary>
        EntitySet,

        /// <summary>
        /// Represents an entity path, for example: ~/users/{id}
        /// </summary>
        Entity,

        /// <summary>
        /// Represents a singleton path, for example: ~/me
        /// </summary>
        Singleton,

        /// <summary>
        /// Represents an operation (function or action) path, for example: ~/users/NS.findRooms(roomId='{roomId}')
        /// </summary>
        Operation,

        /// <summary>
        /// Represents an operation import (function import or action import path), for example: ~/ResetData
        /// </summary>
        OperationImport,

        /// <summary>
        /// Represents an navigation property path, for example: ~/users/{id}/onedrive
        /// </summary>
        NavigationProperty,

        /// <summary>
        /// Represents an navigation property $ref path, for example: ~/users/{id}/onedrive/$ref
        /// </summary>
        Ref,

        /// <summary>
        /// Represents a media entity path, for example: ~/me/photo/$value or ~/reports/deviceConfigurationUserActivity/Content
        /// </summary>
        MediaEntity,

        /// <summary>
        /// Represents a $metadata path
        /// </summary>
        Metadata,

        /// <summary>
        /// Represents a $count path, for example: ~/customers/$count
        /// </summary>
        DollarCount,

        /// <summary>
        /// Represents an un-supported/unknown path.
        /// </summary>
        Unknown
    }
}
