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
        /// Represents an operation (function or action) path, for example: ~/users/NS.findRooms(roomId={roomId})
        /// </summary>
        Operation,

        /// <summary>
        /// Represents an operation import (function import or action import path), for example: ~/ResetData
        /// </summary>
        OperationImport,

        /// <summary>
        /// Represents an navigation propert path, for example: ~/users/{id}/onedrive
        /// </summary>
        NavigationProperty,

        /// <summary>
        /// Represents an un-supported/unknown path.
        /// </summary>
        Unknown
    }
}
