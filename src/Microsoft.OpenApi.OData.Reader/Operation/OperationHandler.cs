// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Annotations;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.Operation
{
    /// <summary>
    /// Base class for <see cref="OpenApiOperation"/> handler.
    /// </summary>
    internal abstract class OperationHandler : IOperationHandler
    {
        /// <inheritdoc/>
        public abstract OperationType OperationType { get; }

        /// <inheritdoc/>
        public virtual OpenApiOperation CreateOperation(ODataContext context, ODataPath path)
        {
            Context = context ?? throw Error.ArgumentNull(nameof(context));

            Path = path ?? throw Error.ArgumentNull(nameof(path));

            // Initialize the object ahead.
            Initialize(context, path);

            OpenApiOperation operation = new OpenApiOperation();

            // Description / Summary / OperationId
            SetBasicInfo(operation);

            // Security
            SetSecurity(operation);

            // Responses
            SetResponses(operation);

            // RequestBody
            SetRequestBody(operation);

            // Parameters
            SetParameters(operation);

            // Tags
            SetTags(operation);

            // Extensions
            SetExtensions(operation);

            return operation;
        }

        /// <summary>
        /// Gets the OData context.
        /// </summary>
        protected ODataContext Context { get; private set; }

        /// <summary>
        /// Gets the OData path.
        /// </summary>
        protected ODataPath Path { get; private set; }

        /// <summary>
        /// Initialize the handler.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="path">The path.</param>
        protected virtual void Initialize(ODataContext context, ODataPath path)
        { }

        /// <summary>
        /// Set the basic information for <see cref="OpenApiOperation"/>.
        /// </summary>
        /// <param name="operation">The <see cref="OpenApiOperation"/>.</param>
        protected virtual void SetBasicInfo(OpenApiOperation operation)
        { }

        /// <summary>
        /// Set the security information for <see cref="OpenApiOperation"/>.
        /// </summary>
        /// <param name="operation">The <see cref="OpenApiOperation"/>.</param>
        protected virtual void SetSecurity(OpenApiOperation operation)
        { }

        /// <summary>
        /// Set the responses information for <see cref="OpenApiOperation"/>.
        /// </summary>
        /// <param name="operation">The <see cref="OpenApiOperation"/>.</param>
        protected virtual void SetResponses(OpenApiOperation operation)
        { }

        /// <summary>
        /// Set the request body information for <see cref="OpenApiOperation"/>.
        /// </summary>
        /// <param name="operation">The <see cref="OpenApiOperation"/>.</param>
        protected virtual void SetRequestBody(OpenApiOperation operation)
        { }

        /// <summary>
        /// Set the parameters information for <see cref="OpenApiOperation"/>.
        /// </summary>
        /// <param name="operation">The <see cref="OpenApiOperation"/>.</param>
        protected virtual void SetParameters(OpenApiOperation operation)
        { }

        /// <summary>
        /// Set the Tags information for <see cref="OpenApiOperation"/>.
        /// </summary>
        /// <param name="operation">The <see cref="OpenApiOperation"/>.</param>
        protected virtual void SetTags(OpenApiOperation operation)
        { }

        /// <summary>
        /// Set the Extensions information for <see cref="OpenApiOperation"/>.
        /// </summary>
        /// <param name="operation">The <see cref="OpenApiOperation"/>.</param>
        protected virtual void SetExtensions(OpenApiOperation operation)
        {
            operation.Extensions.Add(Constants.xMsDosOperationType, new OpenApiString("operation"));
        }

        protected static void AppendCustomParameters(OpenApiOperation operation, HttpRequest request)
        {
            if (request == null)
            {
                return;
            }

            if (operation.Parameters == null)
            {
                operation.Parameters = new List<OpenApiParameter>();
            }

            if (request.CustomQueryOptions != null)
            {
                AppendCustomParameters(operation.Parameters, request.CustomQueryOptions, ParameterLocation.Query);
            }

            if (request.CustomQueryOptions != null)
            {
                AppendCustomParameters(operation.Parameters, request.CustomHeaders, ParameterLocation.Header);
            }
        }

        private static void AppendCustomParameters(IList<OpenApiParameter> parameters, IList<CustomParameter> headers, ParameterLocation location)
        {
            foreach (var param in headers)
            {
                OpenApiParameter parameter = new OpenApiParameter
                {
                    In = location,
                    Name = param.Name,
                    Description = param.Description,
                    Schema = new OpenApiSchema
                    {
                        // Type = param.Type
                    },
                    Required = param.Required ?? false
                };

                if (param.DocumentationURL != null)
                {
                    parameter.Example = new OpenApiString(param.DocumentationURL?? "N/A");
                }

                if (param.ExampleValues != null)
                {
                    parameter.Examples = new Dictionary<string, OpenApiExample>();
                    int index = 1;
                    foreach (var example in param.ExampleValues)
                    {
                        OpenApiExample ex = new OpenApiExample
                        {
                            Description = example.Description
                        };

                        if (example is ExternalExample)
                        {
                            var externalExample = (ExternalExample)example;
                            ex.Value = new OpenApiString(externalExample.ExternalValue ?? "N/A");
                        }
                        else
                        {
                            var inlineExample = (InlineExample)example;
                            ex.Value = new OpenApiString(inlineExample.InlineValue ?? "N/A");
                        }

                        parameter.Examples.Add("example-" + index++, ex);
                    }
                }

                parameters.Add(parameter);
            }
        }
    }
}
