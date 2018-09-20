// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Annotations;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Generator;

namespace Microsoft.OpenApi.OData.Operation
{
    /// <summary>
    /// Base class for <see cref="OpenApiOperation"/> handler.
    /// All derived class should call base method for Set** at the end of override.
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
        /// Gets/sets the <see cref="HttpRequest"/>.
        /// That will be set in the initialize method.
        /// </summary>
        protected HttpRequest Request { get; set; }

        /// <summary>
        /// Initialize the handler.
        /// It should be call ahead of in derived class.
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
        {
            if (Request != null && Request.Description != null)
            {
                operation.Summary = Request.Description;
            }
        }

        /// <summary>
        /// Set the security information for <see cref="OpenApiOperation"/>.
        /// </summary>
        /// <param name="operation">The <see cref="OpenApiOperation"/>.</param>
        protected virtual void SetSecurity(OpenApiOperation operation)
        {
            if (Request != null)
            {
                operation.Security = Context.CreateSecurityRequirements(Request.SecuritySchemes).ToList();
            }
        }

        /// <summary>
        /// Set the responses information for <see cref="OpenApiOperation"/>.
        /// </summary>
        /// <param name="operation">The <see cref="OpenApiOperation"/>.</param>
        protected virtual void SetResponses(OpenApiOperation operation)
        {
            AppendHttpResponses(operation);
        }

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
        {
            foreach (ODataKeySegment keySegment in Path.OfType<ODataKeySegment>())
            {
                foreach (var p in Context.CreateKeyParameters(keySegment))
                {
                    operation.Parameters.Add(p);
                }
            }

            AppendCustomParameters(operation);
        }

        /// <summary>
        /// Set the Tags information for <see cref="OpenApiOperation"/>.
        /// </summary>
        /// <param name="operation">The <see cref="OpenApiOperation"/>.</param>
        protected virtual void SetTags(OpenApiOperation operation)
        {
            /// The OASIS mapping doc says:
            /// The tags array of the Operation Object includes the entity set or singleton name
            /// in the first segment of the path template. Additional tag values,
            /// e.g. for the entity type of a containment navigation property or the target entity set
            /// of a non-containment navigation property, can be included to make this operation more easily discoverable.
            /// However, in this SDK, we use the different pattern for the Tags. See each hander.
        }

        /// <summary>
        /// Set the Extensions information for <see cref="OpenApiOperation"/>.
        /// </summary>
        /// <param name="operation">The <see cref="OpenApiOperation"/>.</param>
        protected virtual void SetExtensions(OpenApiOperation operation)
        { }

        /// <summary>
        /// Set the <see cref="HttpRequest"/> annotation for the operation.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="request">The <see cref="HttpRequest"/>.</param>
        protected virtual void AppendCustomParameters(OpenApiOperation operation)
        {
            if (Request == null)
            {
                return;
            }

            if (operation.Parameters == null)
            {
                operation.Parameters = new List<OpenApiParameter>();
            }

            if (Request.CustomQueryOptions != null)
            {
                AppendCustomParameters(operation.Parameters, Request.CustomQueryOptions, ParameterLocation.Query);
            }

            if (Request.CustomHeaders != null)
            {
                AppendCustomParameters(operation.Parameters, Request.CustomHeaders, ParameterLocation.Header);
            }
        }

        /// <summary>
        /// Set the <see cref="HttpRequest"/> annotation for the response.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="request">The <see cref="HttpRequest"/>.</param>
        protected virtual void AppendHttpResponses(OpenApiOperation operation)
        {
            if (Request == null || Request.HttpResponses == null || !Request.HttpResponses.Any())
            {
                return;
            }

            foreach(var httpResponse in Request.HttpResponses)
            {
                if (operation.Responses.TryGetValue(httpResponse.ResponseCode, out OpenApiResponse response))
                {
                    if (httpResponse.Description != null)
                    {
                        response.Description = httpResponse.Description;
                    }

                    if (httpResponse.Examples != null)
                    {
                        int index = 1;
                        foreach (var example in httpResponse.Examples)
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

                            if (ex.Description != null)
                            {
                                if (response.Content.TryGetValue(ex.Description, out OpenApiMediaType mediaType))
                                {
                                    mediaType.Examples.Add("example-" + index++, ex);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sets the custom parameters.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="headers">The custom parameters.</param>
        /// <param name="location">The parameter location.</param>
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
                        Type = "string"
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
