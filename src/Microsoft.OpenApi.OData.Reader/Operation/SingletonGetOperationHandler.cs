﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.OData.Edm;
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
        /// <summary>
        /// Initializes a new instance of <see cref="SingletonGetOperationHandler"/> class.
        /// </summary>
        /// <param name="document">The document to use to lookup references.</param>
        public SingletonGetOperationHandler(OpenApiDocument document) : base(document)
        {
            
        }
        /// <inheritdoc/>
        public override HttpMethod OperationType => HttpMethod.Get;

        private ReadRestrictionsType? _readRestrictions;

        protected override void Initialize(ODataContext context, ODataPath path)
        {
            base.Initialize(context, path);

            if (!string.IsNullOrEmpty(TargetPath))
                _readRestrictions = Context?.Model.GetRecord<ReadRestrictionsType>(TargetPath, CapabilitiesConstants.ReadRestrictions);
            
            if (Context is not null && Singleton is not null)
            {
                var singletonReadRestrictions = Context.Model.GetRecord<ReadRestrictionsType>(Singleton, CapabilitiesConstants.ReadRestrictions);
                _readRestrictions?.MergePropertiesIfNull(singletonReadRestrictions);
                _readRestrictions ??= singletonReadRestrictions;
            }
        }

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            // Summary and Descriptions
            string placeHolder = "Get " + Singleton?.Name;
            operation.Summary = _readRestrictions?.Description ?? placeHolder;
            operation.Description = _readRestrictions?.LongDescription ?? Context?.Model.GetDescriptionAnnotation(Singleton);

            // OperationId, it should be unique among all operations described in the API.
            if (Context is {Settings.EnableOperationId: true} && Singleton is not null)
            {
                string typeName = Singleton.EntityType.Name;
                operation.OperationId = Singleton.Name + "." + typeName + ".Get" + Utils.UpperFirstChar(typeName);
            }
        }

        /// <inheritdoc/>
        protected override void SetParameters(OpenApiOperation operation)
        {
            base.SetParameters(operation);
            
            if (Singleton is null) return;

            // $select
            var parameter = Context?.CreateSelect(Singleton);
            operation.Parameters ??= [];
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }

            // $expand
            parameter = Context?.CreateExpand(Singleton);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }
        }

        /// <inheritdoc/>
        protected override void SetResponses(OpenApiOperation operation)
        {
            if (Singleton is not null)
            {
                IOpenApiSchema? schema = null;
                Dictionary<string, IOpenApiLink>? links = null;

                if (Context is {Settings.EnableDerivedTypesReferencesForResponses: true})
                {
                    schema = EdmModelHelper.GetDerivedTypesReferenceSchema(Singleton.EntityType, Context.Model, _document);
                }

                if (Context is {Settings.ShowLinks: true} && Path is not null)
                {
                    links = Context.CreateLinks(entityType: Singleton.EntityType, entityName: Singleton.Name,
                            entityKind: Singleton.ContainerElementKind.ToString(), path: Path, parameters: PathParameters);
                }

                schema ??= new OpenApiSchemaReference(Singleton.EntityType.FullName(), _document);

                operation.Responses = new OpenApiResponses
                {
                    {
                        Context?.Settings.UseSuccessStatusCodeRange ?? false ? Constants.StatusCodeClass2XX : Constants.StatusCode200,
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
            }

    		if (Context is not null)
                operation.AddErrorResponses(Context.Settings, _document, false);

            base.SetResponses(operation);
        }

        /// <inheritdoc/>
        protected override void SetSecurity(OpenApiOperation operation)
        {
            if (_readRestrictions?.Permissions == null)
            {
                return;
            }

            operation.Security = Context?.CreateSecurityRequirements(_readRestrictions.Permissions, _document).ToList();
        }

        /// <inheritdoc/>
        protected override void AppendCustomParameters(OpenApiOperation operation)
        {
            if (_readRestrictions == null)
            {
                return;
            }

            if (_readRestrictions.CustomHeaders != null)
            {
                AppendCustomParameters(operation, _readRestrictions.CustomHeaders, ParameterLocation.Header);
            }

            if (_readRestrictions.CustomQueryOptions != null)
            {
                AppendCustomParameters(operation, _readRestrictions.CustomQueryOptions, ParameterLocation.Query);
            }
        }
    }
}
