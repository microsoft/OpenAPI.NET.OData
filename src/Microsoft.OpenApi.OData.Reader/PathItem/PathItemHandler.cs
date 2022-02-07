// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Generator;
using Microsoft.OpenApi.OData.Operation;
using Microsoft.OpenApi.OData.Properties;

namespace Microsoft.OpenApi.OData.PathItem
{
    /// <summary>
    /// Base class for <see cref="IPathItemHandler"/>.
    /// </summary>
    internal abstract class PathItemHandler : IPathItemHandler
    {
        /// <summary>
        /// Gets the handler path kind.
        /// </summary>
        protected abstract ODataPathKind HandleKind { get; }

        /// <summary>
        /// Gets the OData Context.
        /// </summary>
        protected ODataContext Context { get; private set; }

        /// <summary>
        /// Gets the OData Path.
        /// </summary>
        protected ODataPath Path { get; private set; }

        protected IDictionary<ODataSegment, IDictionary<string, string>> ParameterMappings;

        /// <inheritdoc/>
        public virtual OpenApiPathItem CreatePathItem(ODataContext context, ODataPath path)
        {
            Context = context ?? throw Error.ArgumentNull(nameof(context));

            Path = path ?? throw Error.ArgumentNull(nameof(path));

            ParameterMappings = path.CalculateParameterMapping(context.Settings);

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
        protected virtual void AddOperation(OpenApiPathItem item, OperationType operationType)
        {
            string httpMethod = operationType.ToString();
            if (!Path.SupportHttpMethod(httpMethod))
            {
                return;
            }

            IOperationHandlerProvider provider = Context.OperationHanderProvider;
            IOperationHandler operationHander = provider.GetHandler(Path.Kind, operationType);
            item.AddOperation(operationType, operationHander.CreateOperation(Context, Path));
        }

        /// <summary>
        /// Set the parameters information for <see cref="OpenApiPathItem"/>
        /// </summary>
        /// <param name="item">The <see cref="OpenApiPathItem"/>.</param>
        protected virtual void SetParameters(OpenApiPathItem item)
        {
            foreach (ODataKeySegment keySegment in Path.OfType<ODataKeySegment>())
            {
                IDictionary<string, string> mapping = ParameterMappings[keySegment];
                foreach (var parameter in Context.CreateKeyParameters(keySegment, mapping))
                {
                    AppendParameter(item, parameter);
                }
            }

            // Add the route prefix parameter v1{data}
            if (Context.Settings.RoutePathPrefixProvider != null && Context.Settings.RoutePathPrefixProvider.Parameters != null)
            {
                foreach (var parameter in Context.Settings.RoutePathPrefixProvider.Parameters)
                {
                    item.Parameters.Add(parameter);
                }
            }
        }

        protected static void AppendParameter(OpenApiPathItem item, OpenApiParameter parameter)
        {
            HashSet<string> set = new HashSet<string>(item.Parameters.Select(p => p.Name));

            if (!set.Contains(parameter.Name))
            {
                item.Parameters.Add(parameter);
                return;
            }

            int index = 1;
            string originalName = parameter.Name;
            string newName;
            do
            {
                newName = originalName + index.ToString();
                index++;
            }
            while (set.Contains(newName));

            parameter.Name = newName;
            item.Parameters.Add(parameter);
        }
    }
}
