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
        private readonly IDictionary<ODataPathKind, IPathItemHandler> _handlers = new Dictionary<ODataPathKind, IPathItemHandler>
        {
            // EntitySet
            { ODataPathKind.EntitySet, new EntitySetPathItemHandler() },

            // Entity
            { ODataPathKind.Entity, new EntityPathItemHandler() },

            // Singleton
            { ODataPathKind.Singleton, new SingletonPathItemHandler() },

            // Navigation property
            { ODataPathKind.NavigationProperty, new NavigationPropertyPathItemHandler() },

            // Edm Operation
            { ODataPathKind.Operation, new OperationPathItemHandler() },

            // Edm OperationImport
            { ODataPathKind.OperationImport, new OperationImportPathItemHandler() },

            // Edm Ref
            { ODataPathKind.Ref, new RefPathItemHandler() },

            // Media Entity
            { ODataPathKind.MediaEntity, new MediaEntityPathItemHandler() },

            // $Metadata
            { ODataPathKind.Metadata, new MetadataPathItemHandler() },

            // $count
            { ODataPathKind.DollarCount, new DollarCountPathItemHandler() },

            // ~/groups/{id}/members/microsoft.graph.user
            { ODataPathKind.TypeCast, new ODataTypeCastPathItemHandler() },

            // ~/users/{id}/mailboxSettings
            { ODataPathKind.ComplexProperty, new ComplexPropertyItemHandler() },

            // Unknown
            { ODataPathKind.Unknown, null },
        };

        /// <inheritdoc/>
        public IPathItemHandler GetHandler(ODataPathKind pathKind)
        {
            return _handlers[pathKind];
        }
    }
}
