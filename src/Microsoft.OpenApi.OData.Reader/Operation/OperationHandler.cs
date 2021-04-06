// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Generator;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;

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

        protected IDictionary<ODataSegment, IDictionary<string, string>> ParameterMappings;

        /// <inheritdoc/>
        public virtual OpenApiOperation CreateOperation(ODataContext context, ODataPath path)
        {
            Context = context ?? throw Error.ArgumentNull(nameof(context));
            Path = path ?? throw Error.ArgumentNull(nameof(path));

            ParameterMappings = path.CalculateParameterMapping(context.Settings);

            // Initialize the object ahead.
            Initialize(context, path);

            OpenApiOperation operation = new OpenApiOperation();

            // Description / Summary / OperationId
            SetBasicInfo(operation);

            // Security
            SetSecurity(operation);

            /* Parameters
               These need to be set before Responses, as the Parameters
               will be used in the Responses when creating Links.
            */
            SetParameters(operation);

            // Responses
            SetResponses(operation);

            // RequestBody
            SetRequestBody(operation);

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
        {
            foreach (ODataKeySegment keySegment in Path.OfType<ODataKeySegment>())
            {
                IDictionary<string, string> mapping = ParameterMappings[keySegment];
                foreach (var p in Context.CreateKeyParameters(keySegment, mapping))
                {
                    AppendParameter(operation, p);
                }
            }

            AppendCustomParameters(operation);

            // Add the route prefix parameter v1{data}
            if (Context.Settings.RoutePathPrefixProvider != null && Context.Settings.RoutePathPrefixProvider.Parameters != null)
            {
                foreach (var parameter in Context.Settings.RoutePathPrefixProvider.Parameters)
                {
                    operation.Parameters.Add(parameter);
                }
            }
        }

        /// <summary>
        /// Set the Tags information for <see cref="OpenApiOperation"/>.
        /// </summary>
        /// <param name="operation">The <see cref="OpenApiOperation"/>.</param>
        protected virtual void SetTags(OpenApiOperation operation)
        {
            // The OASIS mapping doc says:
            // The tags array of the Operation Object includes the entity set or singleton name
            // in the first segment of the path template. Additional tag values,
            // e.g. for the entity type of a containment navigation property or the target entity set
            // of a non-containment navigation property, can be included to make this operation more easily discoverable.
            // However, in this SDK, we use the different pattern for the Tags. See each hander.
        }

        /// <summary>
        /// Set the Extensions information for <see cref="OpenApiOperation"/>.
        /// </summary>
        /// <param name="operation">The <see cref="OpenApiOperation"/>.</param>
        protected virtual void SetExtensions(OpenApiOperation operation)
        { }

        /// <summary>
        /// Set the customized parameters for the operation.
        /// </summary>
        /// <param name="operation">The operation.</param>
        protected virtual void AppendCustomParameters(OpenApiOperation operation)
        {
        }

        /// <summary>
        /// Set the addition annotation for the response.
        /// </summary>
        /// <param name="operation">The operation.</param>
        protected virtual void AppendHttpResponses(OpenApiOperation operation)
        {
        }

        /// <summary>
        /// Sets the custom parameters.
        /// </summary>
        /// <param name="operation">The OpenApi operation.</param>
        /// <param name="customParameters">The custom parameters.</param>
        /// <param name="location">The parameter location.</param>
        protected static void AppendCustomParameters(OpenApiOperation operation, IList<CustomParameter> customParameters, ParameterLocation location)
        {
            foreach (var param in customParameters)
            {
                string documentationUrl = null;
                string paramDescription;
                if (param.DocumentationURL != null)
                {
                    documentationUrl = $" Documentation URL: {param.DocumentationURL}";
                }

                if (param.Description == null)
                {
                    paramDescription = documentationUrl?.Remove(0, 1);
                }
                else
                {
                    paramDescription = param.Description + documentationUrl;
                }


                OpenApiParameter parameter = new OpenApiParameter
                {
                    In = location,
                    Name = param.Name,
                    Description = paramDescription,
                    Schema = new OpenApiSchema
                    {
                        Type = "string"
                    },
                    Required = param.Required ?? false
                };

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

                        // maybe call convert to Uri literal
                        ex.Value = new OpenApiString(example.Value.ToString());

                        parameter.Examples.Add("example-" + index++, ex);
                    }
                }

                AppendParameter(operation, parameter);
            }
        }

        protected static void AppendParameter(OpenApiOperation operation, OpenApiParameter parameter)
        {
            HashSet<string> set = new HashSet<string>(operation.Parameters.Select(p => p.Name));

            if (!set.Contains(parameter.Name))
            {
                operation.Parameters.Add(parameter);
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
            operation.Parameters.Add(parameter);
        }
    }
}
