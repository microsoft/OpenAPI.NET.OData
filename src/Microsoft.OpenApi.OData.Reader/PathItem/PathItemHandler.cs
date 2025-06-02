// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Generator;
using Microsoft.OpenApi.OData.Properties;
using System;
using System.Net.Http;

namespace Microsoft.OpenApi.OData.PathItem
{
    /// <summary>
    /// Base class for <see cref="IPathItemHandler"/>.
    /// </summary>
    internal abstract class PathItemHandler : IPathItemHandler
    {
        private readonly OpenApiDocument _document;
        /// <summary>
        /// Initializes a new instance of the <see cref="PathItemHandler"/> class.
        /// </summary>
        /// <param name="document">The document to use to lookup references.</param>
        protected PathItemHandler(OpenApiDocument document)
        {
            Utils.CheckArgumentNull(document, nameof(document));
            _document = document;
        }
        /// <summary>
        /// Gets the handler path kind.
        /// </summary>
        protected abstract ODataPathKind HandleKind { get; }

        /// <summary>
        /// Gets the OData Context.
        /// </summary>
        protected ODataContext? Context { get; private set; }

        /// <summary>
        /// Gets the OData Path.
        /// </summary>
        protected ODataPath? Path { get; private set; }

        /// <summary>
        /// Gets the string representation of the Edm target path for annotations.
        /// </summary>
        protected string? TargetPath;

        /// <inheritdoc/>
        public virtual OpenApiPathItem CreatePathItem(ODataContext context, ODataPath path)
        {
            Context = context ?? throw Error.ArgumentNull(nameof(context));

            Path = path ?? throw Error.ArgumentNull(nameof(path));

            Initialize(context, path);

            OpenApiPathItem item = new();

            SetBasicInfo(item);

            if (Context.Settings.DeclarePathParametersOnPathItem)
            {
                SetParameters(item);
            }

            SetOperations(item);

            SetExtensions(item);

            return item;
        }
        /// <summary>
        /// Set the basic information for <see cref="OpenApiPathItem"/>.
        /// </summary>
        /// <param name="pathItem">The <see cref="OpenApiPathItem"/>.</param>
        protected virtual void SetBasicInfo(OpenApiPathItem pathItem)
        { }

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
        {
            if (HandleKind != path.Kind)
            {
                throw Error.InvalidOperation(String.Format(SRResource.InvalidPathKindForPathItemHandler, GetType().Name, path.Kind));
            }
            TargetPath = path.GetTargetPath(context.Model);
        }

        /// <summary>
        /// Set the extensions for the path item.
        /// </summary>
        /// <param name="item">The path item.</param>
        protected virtual void SetExtensions(OpenApiPathItem item)
        { }

        /// <summary>
        /// Add one operation into path item.
        /// </summary>
        /// <param name="item">The path item.</param>
        /// <param name="operationType">The operation type.</param>
        protected virtual void AddOperation(OpenApiPathItem item, HttpMethod operationType)
        {
            string httpMethod = operationType.ToString();
            if (Path is null || !Path.SupportHttpMethod(httpMethod))
            {
                return;
            }

            if (Context?.OperationHandlerProvider is {} provider &&
                provider.GetHandler(Path.Kind, operationType, _document) is {} operationHandler)
                item.AddOperation(operationType, operationHandler.CreateOperation(Context, Path));
        }

        /// <summary>
        /// Set the parameters information for <see cref="OpenApiPathItem"/>
        /// </summary>
        /// <param name="item">The <see cref="OpenApiPathItem"/>.</param>
        protected virtual void SetParameters(OpenApiPathItem item)
        {
            if (Context is not null && Path?.CreatePathParameters(Context, _document) is { Count:> 0} parameters)
            {
                item.Parameters ??= [];
                foreach (var parameter in parameters)
                {
                    item.Parameters.AppendParameter(parameter);
                }
            }
        }
    }
}
