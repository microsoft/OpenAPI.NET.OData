// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Generator;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;

namespace Microsoft.OpenApi.OData.Operation
{
    /// <summary>
    /// Retrieve a Singleton
    /// The Path Item Object for the singleton contains the keyword get with an Operation Object as value
    /// that describes the capabilities for retrieving the singleton.
    /// </summary>
    internal class SingletonGetOperationHandler : SingletonOperationHandler
    {
        /// <inheritdoc/>
        public override OperationType OperationType => OperationType.Get;

        /// <summary>
        /// Gets/Sets the <see cref="ReadRestrictionsType"/>
        /// </summary>
        private ReadRestrictionsType ReadRestrictions { get; set; }

        protected override void Initialize(ODataContext context, ODataPath path)
        {
            base.Initialize(context, path);

            ReadRestrictions = Context.Model.GetRecord<ReadRestrictionsType>(Singleton, CapabilitiesConstants.ReadRestrictions);
        }

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            // Summary and Descriptions
            var placeHolder = "Get " + Singleton.Name;
            operation.Summary = ReadRestrictions?.Description ?? Context.Model.GetDescriptionAnnotation(Singleton) ?? placeHolder;
            operation.Description = ReadRestrictions?.LongDescription ?? Context.Model.GetLongDescriptionAnnotation(Singleton) ?? placeHolder;

            // OperationId, it should be unique among all operations described in the API.
            if (Context.Settings.EnableOperationId)
            {
                string typeName = Singleton.EntityType().Name;
                operation.OperationId = Singleton.Name + "." + typeName + ".Get" + Utils.UpperFirstChar(typeName);
            }
        }

        /// <inheritdoc/>
        protected override void SetParameters(OpenApiOperation operation)
        {
            base.SetParameters(operation);

            // $select
            OpenApiParameter parameter = Context.CreateSelect(Singleton);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }

            // $expand
            parameter = Context.CreateExpand(Singleton);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }
        }

        /// <inheritdoc/>
        protected override void SetResponses(OpenApiOperation operation)
        {
            OpenApiSchema schema = null;
            IDictionary<string, OpenApiLink> links = null;

            if (Context.Settings.EnableDerivedTypesReferencesForResponses)
            {
                schema = EdmModelHelper.GetDerivedTypesReferenceSchema(Singleton.EntityType(), Context.Model);
            }

            if (Context.Settings.ShowLinks)
            {
                links = Context.CreateLinks(entityType: Singleton.EntityType(), entityName: Singleton.Name,
                        entityKind: Singleton.ContainerElementKind.ToString(), path: Path, parameters: PathParameters);
            }

            if (schema == null)
            {
                schema = new OpenApiSchema
                {
                    UnresolvedReference = true,
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.Schema,
                        Id = Singleton.EntityType().FullName()
                    }
                };
            }

            operation.Responses = new OpenApiResponses
            {
                {
                    Constants.StatusCode200,
                    new OpenApiResponse
                    {
                        Description = "Retrieved entity",
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

    		operation.AddErrorResponses(Context.Settings, false);

            base.SetResponses(operation);
        }

        /// <inheritdoc/>
        protected override void SetSecurity(OpenApiOperation operation)
        {
            if (ReadRestrictions?.Permissions == null)
            {
                return;
            }

            operation.Security = Context.CreateSecurityRequirements(ReadRestrictions.Permissions).ToList();
        }

        /// <inheritdoc/>
        protected override void AppendCustomParameters(OpenApiOperation operation)
        {
            if (ReadRestrictions == null)
            {
                return;
            }

            if (ReadRestrictions.CustomHeaders != null)
            {
                AppendCustomParameters(operation, ReadRestrictions.CustomHeaders, ParameterLocation.Header);
            }

            if (ReadRestrictions.CustomQueryOptions != null)
            {
                AppendCustomParameters(operation, ReadRestrictions.CustomQueryOptions, ParameterLocation.Query);
            }
        }
    }
}
