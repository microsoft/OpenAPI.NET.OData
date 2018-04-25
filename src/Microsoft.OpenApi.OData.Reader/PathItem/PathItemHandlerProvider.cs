// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OpenApi.OData.Edm;
using System.Collections.Generic;

namespace Microsoft.OpenApi.OData.PathItem
{
    /// <summary>
    /// Default path item handler provider.
    /// </summary>
    internal class PathItemHandlerProvider : IPathItemHandlerProvider
    {
        private IDictionary<PathType, IPathItemHandler> _handlers = new Dictionary<PathType, IPathItemHandler>
        {
            { PathType.EntitySet, new EntitySetPathItemHandler() },
            { PathType.Entity, new EntityPathItemHandler() },
            { PathType.Singleton, new SingletonPathItemHandler() },
            { PathType.NavigationProperty, new NavigationPropertyPathItemHandler() },
            { PathType.Operation, new OperationPathItemHandler() },
            { PathType.OperationImport, new OperationImportPathItemHandler() },
        };

        /// <inheritdoc/>
        public IPathItemHandler GetHandler(PathType pathType)
        {
            return _handlers[pathType];
        }
    }
}
