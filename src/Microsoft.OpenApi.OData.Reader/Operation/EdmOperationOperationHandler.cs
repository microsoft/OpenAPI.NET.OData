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

            ODataOperationSegment operationSegment = path.LastSegment as ODataOperationSegment;
            EdmOperation = operationSegment.Operation;

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
                string operationId = String.Join(".", Path.Segments.Where(s => !(s is ODataKeySegment)).Select(s => s.Name));
                if (EdmOperation.IsAction())
                {
                    operation.OperationId = operationId;
                }
                else
                {
                    ODataOperationSegment operationSegment = Path.LastSegment as ODataOperationSegment;
                    string pathItemName = operationSegment.GetPathItemName(Context.Settings);
                    string md5 = pathItemName.GetHashMd5();
                    operation.OperationId = operationId + "." + md5.Substring(0, 4);
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
            IEnumerable<OperationRestrictionType> restrictions = Context.Model.GetCollection<OperationRestrictionType>(EdmOperation, CapabilitiesConstants.OperationRestrictions);
            if (restrictions == null || !restrictions.Any())
            {
                return;
            }

            // TODO: how to use the collection?
            OperationRestrictionType operationRestriction = restrictions.First();

            // the Permission should be collection, however current ODL supports the single permission.
            // Will update after ODL change.
            operation.Security = Context.CreateSecurityRequirements(new[] { operationRestriction.Permission.Scheme }).ToList();
        }

        /// <inheritdoc/>
        protected override void AppendCustomParameters(OpenApiOperation operation)
        {
            IEnumerable<OperationRestrictionType> restrictions = Context.Model.GetCollection<OperationRestrictionType>(EdmOperation, CapabilitiesConstants.OperationRestrictions);
            if (restrictions == null || !restrictions.Any())
            {
                return;
            }

            // TODO: how to use the collection?
            OperationRestrictionType operationRestriction = restrictions.First();

            if (operationRestriction.CustomHeaders != null)
            {
                AppendCustomParameters(operation.Parameters, operationRestriction.CustomHeaders, ParameterLocation.Header);
            }

            if (operationRestriction.CustomQueryOptions != null)
            {
                AppendCustomParameters(operation.Parameters, operationRestriction.CustomQueryOptions, ParameterLocation.Query);
            }
        }
    }
}
