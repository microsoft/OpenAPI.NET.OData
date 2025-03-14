// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Net.Http;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.PathItem
{
    /// <summary>
    /// Path item handler for $count.
    /// </summary>
    internal class DollarCountPathItemHandler : PathItemHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DollarCountPathItemHandler"/> class.
        /// </summary>
        /// <param name="document">The document to use for references lookup.</param>
        public DollarCountPathItemHandler(OpenApiDocument document) : base(document)
        {
            
        }
        /// <inheritdoc/>
        protected override ODataPathKind HandleKind => ODataPathKind.DollarCount;

        /// <inheritdoc/>
        protected override void SetOperations(OpenApiPathItem item)
        {
            AddOperation(item, HttpMethod.Get);
        }
        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiPathItem pathItem)
        {
            base.SetBasicInfo(pathItem);
            pathItem.Description = "Provides operations to count the resources in the collection.";
        }
    }
}
