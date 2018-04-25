// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Operation;

namespace Microsoft.OpenApi.OData.PathItem
{
    /// <summary>
    /// Base class for <see cref="IPathItemHandler"/>.
    /// </summary>
    internal abstract class PathItemHandler : IPathItemHandler
    {
        /// <summary>
        /// Gets the OData Context.
        /// </summary>
        protected ODataContext Context { get; private set; }

        /// <summary>
        /// Gets the OData Path.
        /// </summary>
        protected ODataPath Path { get; private set; }

        /// <inheritdoc/>
        public virtual OpenApiPathItem CreatePathItem(ODataContext context, ODataPath path)
        {
            Context = context ?? throw Error.ArgumentNull(nameof(context));

            Path = path ?? throw Error.ArgumentNull(nameof(path));

            Initialize(context, path);

            OpenApiPathItem item = new OpenApiPathItem();

            SetOperations(item);

            return item;
        }

        /// <summary>
        /// Set the operation for the path item.
        /// </summary>
        /// <param name="item">The path item.</param>
        protected abstract void SetOperations(OpenApiPathItem item);

        /// <summary>
        /// Initialize the Handler.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="path">The path.</param>
        protected virtual void Initialize(ODataContext context, ODataPath path)
        { }

        /// <summary>
        /// Add one operation into path item.
        /// </summary>
        /// <param name="item">The path item.</param>
        /// <param name="operationType">The operatin type.</param>
        protected void AddOperation(OpenApiPathItem item, OperationType operationType)
        {
            IOperationHandlerProvider provider = Context.OperationHanderProvider;
            IOperationHandler operationHander = provider.GetHandler(Path.PathType, operationType);
            item.AddOperation(operationType, operationHander.CreateOperation(Context, Path));
        }
    }
}
