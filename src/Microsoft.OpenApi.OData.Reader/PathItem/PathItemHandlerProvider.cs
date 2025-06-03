// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.PathItem
{
    /// <summary>
    /// Default path item handler provider.
    /// </summary>
    internal class PathItemHandlerProvider : IPathItemHandlerProvider
    {
        /// <inheritdoc/>
        public IPathItemHandler? GetHandler(ODataPathKind pathKind, OpenApiDocument document)
        {
            return pathKind switch {
            // EntitySet
            ODataPathKind.EntitySet => new EntitySetPathItemHandler(document),

            // Entity
            ODataPathKind.Entity => new EntityPathItemHandler(document),

            // Singleton
            ODataPathKind.Singleton => new SingletonPathItemHandler(document),

            // Navigation property
            ODataPathKind.NavigationProperty => new NavigationPropertyPathItemHandler(document),

            // Edm Operation
            ODataPathKind.Operation => new OperationPathItemHandler(document),

            // Edm OperationImport
            ODataPathKind.OperationImport => new OperationImportPathItemHandler(document),

            // Edm Ref
            ODataPathKind.Ref => new RefPathItemHandler(document),

            // Media Entity
            ODataPathKind.MediaEntity => new MediaEntityPathItemHandler(document),

            // $Metadata
            ODataPathKind.Metadata => new MetadataPathItemHandler(document),

            // $count
            ODataPathKind.DollarCount => new DollarCountPathItemHandler(document),

            // ~/groups/{id}/members/microsoft.graph.user
            ODataPathKind.TypeCast => new ODataTypeCastPathItemHandler(document),

            // ~/users/{id}/mailboxSettings
            ODataPathKind.ComplexProperty => new ComplexPropertyItemHandler(document),

            // Unknown
                _ => null,
            };
        }
    }
}
