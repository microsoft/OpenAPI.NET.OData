// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Generator;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;

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
        /// Gets the OData operation segment.
        /// </summary>
        protected ODataOperationSegment OperationSegment { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the path has type cast segment or not.
        /// </summary>
        protected bool HasTypeCast { get; private set; }

        /// <inheritdoc/>
        protected override void Initialize(ODataContext context, ODataPath path)
        {
            base.Initialize(context, path);

            // It's bound operation, the first segment must be the navigaiton source.
            ODataNavigationSourceSegment navigationSourceSegment = path.FirstSegment as ODataNavigationSourceSegment;
            NavigationSource = navigationSourceSegment.NavigationSource;

            OperationSegment = path.LastSegment as ODataOperationSegment;
            EdmOperation = OperationSegment.Operation;

            HasTypeCast = path.Segments.Any(s => s is ODataTypeCastSegment);
        }

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            // Summary
            operation.Summary = "Invoke " + (EdmOperation.IsAction() ? "action " : "function ") + EdmOperation.Name;

            // OperationId
            if (Context.Settings.EnableOperationId)
            {
                string operationId = String.Join(".", Path.Segments.Where(s => !(s is ODataKeySegment)).Select(s => s.Identifier));
                if (EdmOperation.IsAction())
                {
                    operation.OperationId = operationId;
                }
                else
                {
                    ODataOperationSegment operationSegment = Path.LastSegment as ODataOperationSegment;
                    string pathItemName = operationSegment.GetPathItemName(Context.Settings, new HashSet<string>());

                    if (Context.Model.IsOperationOverload(operationSegment.Operation))
                    {
                        string hash = pathItemName.GetHashSHA256();
                        operation.OperationId = operationId + "-" + hash.Substring(0, 4);
                    }
                    else
                    {
                        operation.OperationId = operationId;
                    }
                }
            }

            base.SetBasicInfo(operation);
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

            base.SetTags(operation);
        }

        /// <inheritdoc/>
        protected override void SetParameters(OpenApiOperation operation)
        {
            base.SetParameters(operation);

            if (EdmOperation.IsFunction())
            {
                IEdmFunction function = (IEdmFunction)EdmOperation;

                if (OperationSegment.ParameterMappings != null)
                {
                    IList<OpenApiParameter> parameters = Context.CreateParameters(function, OperationSegment.ParameterMappings);
                    foreach (var parameter in parameters)
                    {
                        AppendParameter(operation, parameter);
                    }
                }
                else
                {
                    IDictionary<string, string> mappings = ParameterMappings[OperationSegment];
                    IList<OpenApiParameter> parameters = Context.CreateParameters(function, mappings);
                    if (operation.Parameters == null)
                    {
                        operation.Parameters = parameters;
                    }
                    else
                    {
                        foreach (var parameter in parameters)
                        {
                            AppendParameter(operation, parameter);
                        }
                    }
                }
            }
        }

        /// <inheritdoc/>
        protected override void SetResponses(OpenApiOperation operation)
        {
            if (EdmOperation.IsAction() && EdmOperation.ReturnType == null)
            {
                operation.Responses.Add(Constants.StatusCode204, Constants.StatusCode204.GetResponse());
            }
            else
            {
                // function should have a return type.
                OpenApiResponse response = new OpenApiResponse
                {
                    Description = "Success",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        {
                            Constants.ApplicationJsonMediaType,
                            new OpenApiMediaType
                            {
                                Schema = Context.CreateEdmTypeSchema(EdmOperation.ReturnType)
                            }
                        }
                    }
                };

                operation.Responses.Add(Constants.StatusCode200, response);
            }

            // both action & function has the default response.
            operation.Responses.Add(Constants.StatusCodeDefault, Constants.StatusCodeDefault.GetResponse());

            base.SetResponses(operation);
        }

        /// <inheritdoc/>
        protected override void SetSecurity(OpenApiOperation operation)
        {
            OperationRestrictionsType restriction = Context.Model.GetRecord<OperationRestrictionsType>(EdmOperation, CapabilitiesConstants.OperationRestrictions);
            if (restriction == null || restriction.Permissions == null)
            {
                return;
            }

            operation.Security = Context.CreateSecurityRequirements(restriction.Permissions).ToList();
        }

        /// <inheritdoc/>
        protected override void AppendCustomParameters(OpenApiOperation operation)
        {
            OperationRestrictionsType restriction = Context.Model.GetRecord<OperationRestrictionsType>(EdmOperation, CapabilitiesConstants.OperationRestrictions);
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
    }
}
