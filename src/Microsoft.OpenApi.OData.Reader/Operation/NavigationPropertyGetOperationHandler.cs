// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Generator;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;

namespace Microsoft.OpenApi.OData.Operation
{
    /// <summary>
    /// Retrieve a navigation property from a navigation source.
    /// The Path Item Object for the entity contains the keyword get with an Operation Object as value
    /// that describes the capabilities for retrieving a navigation property form a navigation source.
    /// </summary>
    internal class NavigationPropertyGetOperationHandler : NavigationPropertyOperationHandler
    {
        /// <inheritdoc/>
        public override OperationType OperationType => OperationType.Get;

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            // Summary
            operation.Summary = "Get " + NavigationProperty.Name + " from " + NavigationSource.Name;

            // OperationId
            if (Context.Settings.EnableOperationId)
            {
                string prefix = "Get";
                if (!LastSegmentIsKeySegment && NavigationProperty.TargetMultiplicity() == EdmMultiplicity.Many)
                {
                    prefix = "List";
                }

                operation.OperationId = GetOperationId(prefix);
            }

            base.SetBasicInfo(operation);
        }

        protected override void SetExtensions(OpenApiOperation operation)
        {
            if (Context.Settings.EnablePagination)
            {
                if (!LastSegmentIsKeySegment && NavigationProperty.TargetMultiplicity() == EdmMultiplicity.Many)
                {
                    OpenApiObject extension = new OpenApiObject
                    {
                        { "nextLinkName", new OpenApiString("@odata.nextLink")},
                        { "operationName", new OpenApiString(Context.Settings.PageableOperationName)}
                    };

                    operation.Extensions.Add(Constants.xMsPageable, extension);
                }
            }

            base.SetExtensions(operation);
        }

        /// <inheritdoc/>
        protected override void SetResponses(OpenApiOperation operation)
        {
            if (!LastSegmentIsKeySegment && NavigationProperty.TargetMultiplicity() == EdmMultiplicity.Many)
            {
                operation.Responses = new OpenApiResponses
                {
                    {
                        Constants.StatusCode200,
                        new OpenApiResponse
                        {
                            Reference = new OpenApiReference()
                            {
                                Type = ReferenceType.Response,
                                Id = $"{NavigationProperty.ToEntityType().FullName()}{Constants.CollectionSchemaSuffix}"
                            }
                        }
                    }
                };
            }
            else
            {
                OpenApiSchema schema = null;
                var entityType = NavigationProperty.ToEntityType();

                if (Context.Settings.EnableDerivedTypesReferencesForResponses)
                {
                    schema = EdmModelHelper.GetDerivedTypesReferenceSchema(entityType, Context.Model);
                }

                if (schema == null)
                {
                    schema = new OpenApiSchema
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.Schema,
                            Id = entityType.FullName()
                        }
                    };
                }
                IDictionary<string, OpenApiLink> links = null;
                if (Context.Settings.ShowLinks)
                {
                    string operationId = GetOperationId();

                    links = Context.CreateLinks(entityType: NavigationProperty.ToEntityType(), entityName: NavigationProperty.Name,
                            entityKind: NavigationProperty.PropertyKind.ToString(), parameters: operation.Parameters,
                            navPropOperationId: operationId);
                }

                operation.Responses = new OpenApiResponses
                {
                    {
                        Constants.StatusCode200,
                        new OpenApiResponse
                        {
                            Description = "Retrieved navigation property",
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
                            Links = links
                        }
                    }
                };
            }

            operation.AddErrorResponses(Context.Settings, false);

            base.SetResponses(operation);
        }

        /// <inheritdoc/>
        protected override void SetParameters(OpenApiOperation operation)
        {
            base.SetParameters(operation);

            if (!LastSegmentIsKeySegment && NavigationProperty.TargetMultiplicity() == EdmMultiplicity.Many)
            {
                // Need to verify that TopSupported or others should be applied to navigation source.
                // So, how about for the navigation property.
                OpenApiParameter parameter = Context.CreateTop(NavigationProperty);
                if (parameter != null)
                {
                    operation.Parameters.Add(parameter);
                }

                parameter = Context.CreateSkip(NavigationProperty);
                if (parameter != null)
                {
                    operation.Parameters.Add(parameter);
                }

                parameter = Context.CreateSearch(NavigationProperty);
                if (parameter != null)
                {
                    operation.Parameters.Add(parameter);
                }

                parameter = Context.CreateFilter(NavigationProperty);
                if (parameter != null)
                {
                    operation.Parameters.Add(parameter);
                }

                parameter = Context.CreateCount(NavigationProperty);
                if (parameter != null)
                {
                    operation.Parameters.Add(parameter);
                }

                parameter = Context.CreateOrderBy(NavigationProperty);
                if (parameter != null)
                {
                    operation.Parameters.Add(parameter);
                }

                parameter = Context.CreateSelect(NavigationProperty);
                if (parameter != null)
                {
                    operation.Parameters.Add(parameter);
                }

                parameter = Context.CreateExpand(NavigationProperty);
                if (parameter != null)
                {
                    operation.Parameters.Add(parameter);
                }
            }
            else
            {
                OpenApiParameter parameter = Context.CreateSelect(NavigationProperty);
                if (parameter != null)
                {
                    operation.Parameters.Add(parameter);
                }

                parameter = Context.CreateExpand(NavigationProperty);
                if (parameter != null)
                {
                    operation.Parameters.Add(parameter);
                }
            }
        }

        protected override void SetSecurity(OpenApiOperation operation)
        {
            if (Restriction == null || Restriction.ReadRestrictions == null)
            {
                return;
            }

            ReadRestrictionsBase readBase = Restriction.ReadRestrictions;
            if (LastSegmentIsKeySegment)
            {
                if (Restriction.ReadRestrictions.ReadByKeyRestrictions != null)
                {
                    readBase = Restriction.ReadRestrictions.ReadByKeyRestrictions;
                }
            }

            operation.Security = Context.CreateSecurityRequirements(readBase.Permissions).ToList();
        }

        protected override void AppendCustomParameters(OpenApiOperation operation)
        {
            if (Restriction == null || Restriction.ReadRestrictions == null)
            {
                return;
            }

            ReadRestrictionsBase readBase = Restriction.ReadRestrictions;
            if (LastSegmentIsKeySegment)
            {
                if (Restriction.ReadRestrictions.ReadByKeyRestrictions != null)
                {
                    readBase = Restriction.ReadRestrictions.ReadByKeyRestrictions;
                }
            }

            if (readBase.CustomHeaders != null)
            {
                AppendCustomParameters(operation, readBase.CustomHeaders, ParameterLocation.Header);
            }

            if (readBase.CustomQueryOptions != null)
            {
                AppendCustomParameters(operation, readBase.CustomQueryOptions, ParameterLocation.Query);
            }
        }
    }
}
