// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;

namespace Microsoft.OpenApi.OData.Operation
{
    /// <summary>
    /// Retrieve a metadata document "get"
    /// </summary>
    internal class MetadataGetOperationHandler : OperationHandler
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MetadataGetOperationHandler"/> class.
        /// </summary>
        /// <param name="document">The document to use to lookup references.</param>
        public MetadataGetOperationHandler(OpenApiDocument document):base(document)
        {
            
        }
        /// <inheritdoc/>
        public override OperationType OperationType => OperationType.Get;

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            // Summary
            operation.Summary = $"Get OData metadata (CSDL) document";

            // OperationId
            if (Context.Settings.EnableOperationId)
            {
                string routePrefix = Context.Settings.PathPrefix ?? "";
                if (Context.Settings.PathPrefix != null)
                {
                    operation.OperationId = $"{routePrefix}.Get.Metadata";
                }
                else
                {
                    operation.OperationId = "Get.Metadata";
                }
            }

            base.SetBasicInfo(operation);
        }

        /// <inheritdoc/>
        protected override void SetResponses(OpenApiOperation operation)
        {
            OpenApiSchema schema = new OpenApiSchema
            {
                Type = JsonSchemaType.String
            };

            operation.Responses = new OpenApiResponses
            {
                {
                    Context.Settings.UseSuccessStatusCodeRange ? Constants.StatusCodeClass2XX : Constants.StatusCode200,
                    new OpenApiResponse
                    {
                        Description = "Retrieved metadata document",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            {
                                Constants.ApplicationXmlMediaType,
                                new OpenApiMediaType
                                {
                                    Schema = schema
                                }
                            }
                        }
                    }
                }
            };
            operation.AddErrorResponses(Context.Settings, _document, false);

            base.SetResponses(operation);
        }
    }
}
