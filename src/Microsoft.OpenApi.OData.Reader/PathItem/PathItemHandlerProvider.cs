// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.PathItem
{
    /// <summary>
    /// Default path item handler provider.
    /// </summary>
    internal class PathItemHandlerProvider : IPathItemHandlerProvider
    {
        private IDictionary<ODataPathType, IPathItemHandler> _handlers = new Dictionary<ODataPathType, IPathItemHandler>
        {
            // Entity
            { ODataPathType.EntitySet, new EntitySetPathItemHandler() },

            // Entity
            { ODataPathType.Entity, new EntityPathItemHandler() },

            // Singleton
            { ODataPathType.Singleton, new SingletonPathItemHandler() },

            // Navigation property
            { ODataPathType.NavigationProperty, new NavigationPropertyPathItemHandler() },

            // Edm Operation
            { ODataPathType.Operation, new OperationPathItemHandler() },

            // Edm OperationImport
            { ODataPathType.OperationImport, new OperationImportPathItemHandler() },
        };

        /// <inheritdoc/>
        public IPathItemHandler GetHandler(ODataPathType pathType)
        {
            return _handlers[pathType];
        }
    }
}
