// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.OpenApi.MicrosoftExtensions;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Generator;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;
using Microsoft.OpenApi.OData.Vocabulary.Core;

namespace Microsoft.OpenApi.OData.Operation
{
    /// <summary>
    /// Base class for <see cref="OpenApiOperation"/> handler.
    /// All derived class should call base method for Set** at the end of override.
    /// </summary>
    internal abstract class OperationHandler : IOperationHandler
    {
        protected readonly OpenApiDocument _document;
        /// <summary>
        /// Creates a new instance of <see cref="OperationHandler"/> class.
        /// </summary>
        /// <param name="document">Document to use to lookup references.</param>
        protected OperationHandler(OpenApiDocument document)
        {
            Utils.CheckArgumentNull(document, nameof(document));
            _document = document;
        }
        /// <inheritdoc/>
        public abstract HttpMethod OperationType { get; }

        protected IDictionary<ODataSegment, IDictionary<string, string>>? ParameterMappings;

        /// <summary>
        /// The path parameters in the path
        /// </summary>
        protected IList<IOpenApiParameter>? PathParameters;

        /// <summary>
        /// The string representation of the Edm target path for annotations.
        /// </summary>
        protected string? TargetPath;

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
            SetDeprecation(operation);

            // ExternalDocs
            SetExternalDocs(operation);

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

        private void SetDeprecation(OpenApiOperation operation)
        {
            if (operation is null ||
                Context is null ||
                !Context.Settings.EnableDeprecationInformation ||
                Path is null)
            {
                return;
            }
            var deprecationInfo = Path.SelectMany(x => x.GetAnnotables())
                                .SelectMany(x => Context.GetDeprecationInformations(x))
                                .Where(x => x != null)
                                .OrderByDescending(x => x.Date)
                                .ThenByDescending(x => x.RemovalDate)
                                .FirstOrDefault();

            if (deprecationInfo != null)
            {
                operation.Deprecated = true;
                var deprecationDetails = deprecationInfo.GetOpenApiExtension();
                operation.Extensions ??= new Dictionary<string, IOpenApiExtension>();
                operation.Extensions.Add(OpenApiDeprecationExtension.Name, deprecationDetails);
            }
        }

        /// <summary>
        /// Gets the OData context.
        /// </summary>
        protected ODataContext? Context { get; private set; }

        /// <summary>
        /// Gets the OData path.
        /// </summary>
        protected ODataPath? Path { get; private set; }

        /// <summary>
        /// Gets the custom link relation type for path based on operation type
        /// </summary>
        protected string? CustomLinkRel { get; set; }

        /// <summary>
        /// Initialize the handler.
        /// It should be call ahead of in derived class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="path">The path.</param>
        protected virtual void Initialize(ODataContext context, ODataPath path)
        {
            SetCustomLinkRelType();
            TargetPath = path.GetTargetPath(context.Model);
        }

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
            if (Context is not null && Path is not null)
            {
                PathParameters = Path.CreatePathParameters(Context, _document);
                if (!Context.Settings.DeclarePathParametersOnPathItem)
                {
                    operation.Parameters ??= [];
                    foreach (var parameter in PathParameters)
                    {
                        operation.Parameters.AppendParameter(parameter);
                    }
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
        /// Set the ExternalDocs information for <see cref="OpenApiOperation"/>.
        /// </summary>
        /// <param name="operation">The <see cref="OpenApiOperation"/>.</param>
        protected virtual void SetExternalDocs(OpenApiOperation operation)
        { }

        /// <summary>
        /// Set the customized parameters for the operation.
        /// </summary>
        /// <param name="operation">The operation.</param>
        protected virtual void AppendCustomParameters(OpenApiOperation operation)
        { }

        /// <summary>
        /// Set the addition annotation for the response.
        /// </summary>
        /// <param name="operation">The operation.</param>
        protected virtual void AppendHttpResponses(OpenApiOperation operation)
        { }

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
                string? documentationUrl = null;
                if (param.DocumentationURL != null)
                {
                    documentationUrl = $" Documentation URL: {param.DocumentationURL}";
                }

                // DocumentationURL value is to be appended to
                // the parameter Description property
                var paramDescription = (param.Description == null) ? documentationUrl?.Remove(0, 1) : param.Description + documentationUrl;

                OpenApiParameter parameter = new()
                {
                    In = location,
                    Name = param.Name,
                    Description = paramDescription,
                    Schema = new OpenApiSchema
                    {
                        Type = JsonSchemaType.String
                    },
                    Required = param.Required ?? false
                };

                if (param.ExampleValues != null)
                {
                    parameter.Examples = new Dictionary<string, IOpenApiExample>();
                    int index = 1;
                    foreach (var example in param.ExampleValues)
                    {
                        OpenApiExample ex = new OpenApiExample
                        {
                            Description = example.Description
                        };

                        // maybe call convert to Uri literal
                        ex.Value = example.Value?.ToString();

                        parameter.Examples.Add("example-" + index++, ex);
                    }
                }

                operation.Parameters ??= [];
                operation.Parameters.AppendParameter(parameter);
            }
        }

        /// <summary>
        /// Set link relation type to be used to get external docs link for path operation
        /// </summary>
        protected virtual void SetCustomLinkRelType()
        {
            if (Context?.Settings.CustomHttpMethodLinkRelMapping is not null &&
                Path is not null)
            {
                LinkRelKey? key = OperationType.ToString().ToLowerInvariant() switch
                {
                    "get" => Path.LastSegment?.Kind ==  ODataSegmentKind.Key ? LinkRelKey.ReadByKey : LinkRelKey.List,
                    "post" => LinkRelKey.Create,
                    "patch" => LinkRelKey.Update,
                    "put" => LinkRelKey.Update,
                    "delete" => LinkRelKey.Delete,
                    _ => null,
                };

                if (key != null && key.HasValue && Context.Settings.CustomHttpMethodLinkRelMapping.TryGetValue(key.Value, out var linkRelValue))
                {
                    CustomLinkRel = linkRelValue;
                }
            }
        }

        internal void SetCollectionResponse(OpenApiOperation operation, string targetElementFullName)
        {
            Utils.CheckArgumentNull(operation, nameof(operation));
            Utils.CheckArgumentNullOrEmpty(targetElementFullName, nameof(targetElementFullName));            

            operation.Responses = new OpenApiResponses
            {
                {
                    (Context?.Settings.UseSuccessStatusCodeRange ?? false) ? Constants.StatusCodeClass2XX : Constants.StatusCode200,
                    new OpenApiResponseReference($"{targetElementFullName}{Constants.CollectionSchemaSuffix}", _document)
                }
            };
        }

        internal void SetSingleResponse(OpenApiOperation operation, IOpenApiSchema schema)
        {
            Utils.CheckArgumentNull(operation, nameof(operation));
            Utils.CheckArgumentNull(schema, nameof(schema));
            
            operation.Responses = new OpenApiResponses
            {
                {
                    (Context?.Settings.UseSuccessStatusCodeRange ?? false) ? Constants.StatusCodeClass2XX : Constants.StatusCode200,
                    new OpenApiResponse
                    {
                        Description = "Entity result.",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            {
                                Constants.ApplicationJsonMediaType,
                                new OpenApiMediaType
                                {
                                    Schema = schema
                                }
                            }
                        },
                    }
                }
            };
        }
    }
}
