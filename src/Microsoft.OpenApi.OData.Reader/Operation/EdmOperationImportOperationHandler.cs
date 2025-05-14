﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.References;
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
        /// Initializes a new instance of <see cref="EdmOperationImportOperationHandler"/> class.
        /// </summary>
        /// <param name="document">The document to use to lookup references.</param>
        protected EdmOperationImportOperationHandler(OpenApiDocument document):base(document)
        {
            
        }
        private OperationRestrictionsType? _operationRestriction;

        /// <summary>
        /// Gets the <see cref="IEdmOperationImport"/>.
        /// </summary>
        protected IEdmOperationImport? EdmOperationImport { get; private set; }

        /// <summary>
        /// Gets the <see cref="IEdmOperationImport"/>.
        /// </summary>
        protected ODataOperationImportSegment? OperationImportSegment { get; private set; }

        /// <inheritdoc/>
        protected override void Initialize(ODataContext context, ODataPath path)
        {
            base.Initialize(context, path);

            OperationImportSegment = path.LastSegment as ODataOperationImportSegment;
            EdmOperationImport = OperationImportSegment?.OperationImport;

            _operationRestriction = string.IsNullOrEmpty(TargetPath) ? null : Context?.Model.GetRecord<OperationRestrictionsType>(TargetPath, CapabilitiesConstants.OperationRestrictions);
            
            
            if (EdmOperationImport is not null && Context?.Model.GetRecord<OperationRestrictionsType>(EdmOperationImport, CapabilitiesConstants.OperationRestrictions) is {} operationRestrictions)
            {
                if (_operationRestriction == null)
                {
                    _operationRestriction = operationRestrictions;
                }
                else
                {
                    _operationRestriction.MergePropertiesIfNull(operationRestrictions);
                }
            }
        }

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            operation.Summary = "Invoke " + (EdmOperationImport.IsActionImport() ? "actionImport " : "functionImport ") + EdmOperationImport?.Name;

            if (Context is not null)
                operation.Description = (string.IsNullOrEmpty(TargetPath) ? null : Context.Model.GetDescriptionAnnotation(TargetPath)) ??
                                        Context.Model.GetDescriptionAnnotation(EdmOperationImport);

            if (Context is {Settings.EnableOperationId: true} && EdmOperationImport is not null )
            {
                if (EdmOperationImport.IsActionImport())
                {
                    operation.OperationId = "ActionImport." + EdmOperationImport.Name;
                }
                else
                {
                    if (Context.Model.IsOperationImportOverload(EdmOperationImport))
                    {
                        operation.OperationId = "FunctionImport." + EdmOperationImport.Name + "-" + Path?.LastSegment?.GetPathHash(Context.Settings);
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
            
            if (Context is not null && EdmOperationImport is not null)
                operation.Responses = Context.CreateResponses(EdmOperationImport, _document);

            base.SetResponses(operation);
        }

        /// <inheritdoc/>
        protected override void SetSecurity(OpenApiOperation operation)
        {
            if (_operationRestriction == null || _operationRestriction.Permissions == null)
            {
                return;
            }

            operation.Security = Context?.CreateSecurityRequirements(_operationRestriction.Permissions, _document).ToList();
        }

        /// <inheritdoc/>
        protected override void AppendCustomParameters(OpenApiOperation operation)
        {
            if (_operationRestriction == null)
            {
                return;
            }

            if (_operationRestriction.CustomHeaders != null)
            {
                AppendCustomParameters(operation, _operationRestriction.CustomHeaders, ParameterLocation.Header);
            }

            if (_operationRestriction.CustomQueryOptions != null)
            {
                AppendCustomParameters(operation, _operationRestriction.CustomQueryOptions, ParameterLocation.Query);
            }
        }

        /// <inheritdoc/>
        protected override void SetTags(OpenApiOperation operation)
        {
            if (EdmOperationImport is not null)
            {
                var tag = CreateTag(EdmOperationImport);
                tag.Extensions ??= new Dictionary<string, IOpenApiExtension>();
                tag.Extensions.Add(Constants.xMsTocType, new JsonNodeExtension("container"));
                Context?.AppendTag(tag);

                operation.Tags ??= new HashSet<OpenApiTagReference>();
                operation.Tags.Add(new OpenApiTagReference(tag.Name!, _document));
            }

            base.SetTags(operation);
        }

        private static OpenApiTag CreateTag(IEdmOperationImport operationImport)
        {
            if (operationImport.EntitySet is IEdmPathExpression pathExpression)
            {
                return new OpenApiTag
                {
                    Name = PathAsString(pathExpression.PathSegments)
                };
            }

            return new OpenApiTag
            {
                Name = operationImport.Name
            };
        }

        internal static string PathAsString(IEnumerable<string> path)
        {
            return string.Join("/", path);
        }

        /// <inheritdoc/>
        protected override void SetExternalDocs(OpenApiOperation operation)
        {
            if (Context is {Settings.ShowExternalDocs: true} && CustomLinkRel is not null)
            {
                var externalDocs = (string.IsNullOrEmpty(TargetPath) ? null : Context.Model.GetLinkRecord(TargetPath, CustomLinkRel)) ??
                    (EdmOperationImport is null ? null : Context.Model.GetLinkRecord(EdmOperationImport, CustomLinkRel));

                if (externalDocs != null)
                {
                    operation.ExternalDocs = new OpenApiExternalDocs()
                    {
                        Description = CoreConstants.ExternalDocsDescription,
                        Url = externalDocs.Href
                    };
                }
            }
        }
    }
}
