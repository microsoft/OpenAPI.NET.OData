// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Generator;

namespace Microsoft.OpenApi.OData.Operation
{
    /// <summary>
    /// Base class for <see cref="IEdmOperationImport"/>.
    /// </summary>
    internal abstract class EdmOperationImportOperationHandler : OperationHandler
    {
        /// <summary>
        /// Gets the <see cref="IEdmOperationImport"/>.
        /// </summary>
        protected IEdmOperationImport EdmOperationImport { get; private set; }

        /// <inheritdoc/>
        protected override void Initialize(ODataContext context, ODataPath path)
        {
            ODataOperationImportSegment operationImportSegment = path.LastSegment as ODataOperationImportSegment;
            EdmOperationImport = operationImportSegment.OperationImport;
            base.Initialize(context, path);
        }

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            operation.Summary = "Invoke " + (EdmOperationImport.IsActionImport() ? "actionImport " : "functionImport ") + EdmOperationImport.Name;

            if (Context.Settings.OperationId)
            {
                string key = "OperationImport." + EdmOperationImport.Name;
                operation.OperationId += "OperationImport." + Context.GetIndex(key) + "-" + Utils.UpperFirstChar(EdmOperationImport.Name);

                if (EdmOperationImport.IsActionImport())
                {
                    operation.OperationId = "OperationImport." + EdmOperationImport.Name;
                }
                else
                {
                    ODataOperationImportSegment operationImportSegment = Path.LastSegment as ODataOperationImportSegment;
                    string pathItemName = operationImportSegment.GetPathItemName(Context.Settings);
                    string md5 = pathItemName.GetHashMd5();
                    operation.OperationId = "OperationImport." + EdmOperationImport.Name + "." + md5.Substring(8);
                }
            }
        }

        /// <inheritdoc/>
        protected override void SetResponses(OpenApiOperation operation)
        {
            // The responses object contains a name/value pair for the success case (HTTP response code 200)
            // describing the structure of the success response by referencing an appropriate schema
            // in the global schemas. In addition, it contains a default name/value pair for
            // the OData error response referencing the global responses.
            operation.Responses = Context.CreateResponses(EdmOperationImport);
        }

        /// <inheritdoc/>
        protected override void SetTags(OpenApiOperation operation)
        {
            operation.Tags = CreateTags(EdmOperationImport);
            operation.Tags[0].Extensions.Add(Constants.xMsTocType, new OpenApiString("container"));
            Context.AppendTag(operation.Tags[0]);
        }

        private static IList<OpenApiTag> CreateTags(IEdmOperationImport operationImport)
        {
            if (operationImport.EntitySet != null)
            {
                var pathExpression = operationImport.EntitySet as IEdmPathExpression;
                if (pathExpression != null)
                {
                    return new List<OpenApiTag>
                    {
                        new OpenApiTag
                        {
                            Name = PathAsString(pathExpression.PathSegments)
                        }
                    };
                }
            }

            return new List<OpenApiTag>{
                new OpenApiTag
                {
                    Name = operationImport.Name
                }
            };
        }

        internal static string PathAsString(IEnumerable<string> path)
        {
            return String.Join("/", path);
        }
    }
}
