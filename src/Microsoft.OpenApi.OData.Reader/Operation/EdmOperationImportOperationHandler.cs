// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Generator;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;
using Microsoft.OpenApi.OData.Vocabulary.Core;

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

        /// <summary>
        /// Gets the <see cref="IEdmOperationImport"/>.
        /// </summary>
        protected ODataOperationImportSegment OperationImportSegment { get; private set; }

        /// <inheritdoc/>
        protected override void Initialize(ODataContext context, ODataPath path)
        {
            base.Initialize(context, path);

            OperationImportSegment = path.LastSegment as ODataOperationImportSegment;
            EdmOperationImport = OperationImportSegment.OperationImport;
        }

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            operation.Summary = "Invoke " + (EdmOperationImport.IsActionImport() ? "actionImport " : "functionImport ") + EdmOperationImport.Name;

            operation.Description = Context.Model.GetDescriptionAnnotation(EdmOperationImport);

            if (Context.Settings.EnableOperationId)
            {
                if (EdmOperationImport.IsActionImport())
                {
                    operation.OperationId = "ActionImport." + EdmOperationImport.Name;
                }
                else
                {
                    if (Context.Model.IsOperationImportOverload(EdmOperationImport))
                    {
                        operation.OperationId = "FunctionImport." + EdmOperationImport.Name + "-" + Path.LastSegment.GetPathHash(Context.Settings);
                    }
                    else
                    {
                        operation.OperationId = "FunctionImport." + EdmOperationImport.Name;
                    }
                }
            }

            base.SetBasicInfo(operation);
        }

        /// <inheritdoc/>
        protected override void SetResponses(OpenApiOperation operation)
        {
            // The responses object contains a name/value pair for the success case (HTTP response code 200)
            // describing the structure of the success response by referencing an appropriate schema
            // in the global schemas. In addition, it contains a default name/value pair for
            // the OData error response referencing the global responses.
            operation.Responses = Context.CreateResponses(EdmOperationImport, Path);

            base.SetResponses(operation);
        }

        /// <inheritdoc/>
        protected override void SetSecurity(OpenApiOperation operation)
        {
            OperationRestrictionsType restriction = Context.Model.GetRecord<OperationRestrictionsType>(EdmOperationImport, CapabilitiesConstants.OperationRestrictions);
            if (restriction == null || restriction.Permissions == null)
            {
                return;
            }

            operation.Security = Context.CreateSecurityRequirements(restriction.Permissions).ToList();
        }

        /// <inheritdoc/>
        protected override void AppendCustomParameters(OpenApiOperation operation)
        {
            OperationRestrictionsType restriction = Context.Model.GetRecord<OperationRestrictionsType>(EdmOperationImport, CapabilitiesConstants.OperationRestrictions);
            if (restriction == null)
            {
                return;
            }

            if (restriction.CustomHeaders != null)
            {
                AppendCustomParameters(operation, restriction.CustomHeaders, ParameterLocation.Header);
            }

            if (restriction.CustomQueryOptions != null)
            {
                AppendCustomParameters(operation, restriction.CustomQueryOptions, ParameterLocation.Query);
            }
        }

        /// <inheritdoc/>
        protected override void SetTags(OpenApiOperation operation)
        {
            operation.Tags = CreateTags(EdmOperationImport);
            operation.Tags[0].Extensions.Add(Constants.xMsTocType, new OpenApiString("container"));
            Context.AppendTag(operation.Tags[0]);

            base.SetTags(operation);
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

        /// <inheritdoc/>
        protected override void SetExternalDocs(OpenApiOperation operation)
        {
            if (Context.Settings.ShowExternalDocs && Context.Model.GetLinkRecord(EdmOperationImport, OperationType, Path) is Link externalDocs)
            {
                operation.ExternalDocs = operation.ExternalDocs = new OpenApiExternalDocs()
                {
                    Description = CoreConstants.ExternalDocsDescription,
                    Url = externalDocs.Href
                };
            }
        }
    }
}
