// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Generator;

namespace Microsoft.OpenApi.OData.Operation
{
    /// <summary>
    /// Base class for operation of <see cref="IEdmOperation"/>.
    /// </summary>
    internal abstract class EdmOperationOperationHandler : OperationHandler
    {
        /// <summary>
        /// Gets the navigation source.
        /// </summary>
        protected IEdmNavigationSource NavigationSource { get; private set; }

        /// <summary>
        /// Gets the Edm operation.
        /// </summary>
        protected IEdmOperation EdmOperation { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the path has type cast segment or not.
        /// </summary>
        protected bool HasTypeCast { get; private set; }

        /// <inheritdoc/>
        protected override void Initialize(ODataContext context, ODataPath path)
        {
            ODataNavigationSourceSegment navigationSourceSegment = path.FirstSegment as ODataNavigationSourceSegment;
            NavigationSource = navigationSourceSegment.NavigationSource;

            ODataOperationSegment operationSegment = path.LastSegment as ODataOperationSegment;
            EdmOperation = operationSegment.Operation;

            HasTypeCast = path.Segments.Any(s => s is ODataTypeCastSegment);

            base.Initialize(context, path);
        }

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            // Summary
            operation.Summary = "Invoke " + (EdmOperation.IsAction() ? "action " : "function ") + EdmOperation.Name;

            // OperationId
            if (Context.Settings.OperationId)
            {
                string key = NavigationSource.Name + "-" + Utils.UpperFirstChar(EdmOperation.Name);
                int index = Context.GetIndex(key);
                operation.OperationId = NavigationSource.Name + "." + index + "-" + Utils.UpperFirstChar(EdmOperation.Name);
            }
        }

        /// <inheritdoc/>
        protected override void SetTags(OpenApiOperation operation)
        {
            string value = EdmOperation.IsAction() ? "Actions" : "Functions";
            OpenApiTag tag = new OpenApiTag
            {
                Name = NavigationSource.Name + "." + value,
            };
            tag.Extensions.Add(Constants.xMsTocType, new OpenApiString("container"));
            operation.Tags.Add(tag);

            Context.AppendTag(tag);
        }

        /// <inheritdoc/>
        protected override void SetParameters(OpenApiOperation operation)
        {
            base.SetParameters(operation);
            /*
            IEdmSingleton singleton = NavigationSource as IEdmSingleton;
            if (singleton == null && EdmOperation.IsBound)
            {
                IEdmOperationParameter bindingParameter = EdmOperation.Parameters.FirstOrDefault();
                if (bindingParameter != null &&
                    !bindingParameter.Type.IsCollection() && // bound to a single entity
                    bindingParameter.Type.IsEntity())
                {
                    operation.Parameters = Context.CreateKeyParameters(bindingParameter
                        .Type.AsEntity().EntityDefinition());
                }
            }*/

            if (EdmOperation.IsFunction())
            {
                IEdmFunction function = (IEdmFunction)EdmOperation;
                IList<OpenApiParameter> parameters = Context.CreateParameters(function);
                if (operation.Parameters == null)
                {
                    operation.Parameters = parameters;
                }
                else
                {
                    foreach (var parameter in parameters)
                    {
                        operation.Parameters.Add(parameter);
                    }
                }
            }
        }

        protected override void SetResponses(OpenApiOperation operation)
        {
            operation.Responses = Context.CreateResponses(EdmOperation);
        }
    }
}
