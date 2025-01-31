// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.Interfaces;
using Microsoft.OpenApi.Models.References;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Generator;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;

namespace Microsoft.OpenApi.OData.Operation;

internal abstract class ComplexPropertyUpdateOperationHandler : ComplexPropertyBaseOperationHandler
{
    /// <summary>
    /// Initializes a new instance of <see cref="ComplexPropertyUpdateOperationHandler"/> class.
    /// </summary>
    /// <param name="document">The document to use to lookup references.</param>
    protected ComplexPropertyUpdateOperationHandler(OpenApiDocument document) : base(document)
    {
        
    }
    
    private UpdateRestrictionsType _updateRestrictions;

    protected override void Initialize(ODataContext context, ODataPath path)
    {
        base.Initialize(context, path);

        _updateRestrictions = Context.Model.GetRecord<UpdateRestrictionsType>(TargetPath, CapabilitiesConstants.UpdateRestrictions);
        var complexPropertyUpdateRestrictions = Context.Model.GetRecord<UpdateRestrictionsType>(ComplexPropertySegment.Property, CapabilitiesConstants.UpdateRestrictions);
        _updateRestrictions?.MergePropertiesIfNull(complexPropertyUpdateRestrictions);
        _updateRestrictions ??= complexPropertyUpdateRestrictions;
    }

    /// <inheritdoc/>
    protected override void SetBasicInfo(OpenApiOperation operation)
    {
        // Summary and Description
        string placeHolder = $"Update property {ComplexPropertySegment.Property.Name} value.";
        operation.Summary = _updateRestrictions?.Description ?? placeHolder;
        operation.Description = _updateRestrictions?.LongDescription;

        // OperationId
        if (Context.Settings.EnableOperationId)
        {
            string prefix = OperationType == OperationType.Patch ? "Update" : "Set";
            operation.OperationId = EdmModelHelper.GenerateComplexPropertyPathOperationId(Path, Context, prefix);
        }
    }

    /// <inheritdoc/>
    protected override void SetRequestBody(OpenApiOperation operation)
    {
        operation.RequestBody = new OpenApiRequestBody
        {
            Required = true,
            Description = "New property values",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                {
                    Constants.ApplicationJsonMediaType, new OpenApiMediaType
                    {
                        Schema = GetOpenApiSchema()
                    }
                }
            }
        };

        base.SetRequestBody(operation);
    }

    /// <inheritdoc/>
    protected override void SetResponses(OpenApiOperation operation)
    {
        operation.AddErrorResponses(Context.Settings, _document, true, GetOpenApiSchema());
        base.SetResponses(operation);
    }
    protected override void SetSecurity(OpenApiOperation operation)
    {
        if (_updateRestrictions?.Permissions == null)
        {
            return;
        }

        operation.Security = Context.CreateSecurityRequirements(_updateRestrictions.Permissions, _document).ToList();
    }

    protected override void AppendCustomParameters(OpenApiOperation operation)
    {
        if (_updateRestrictions == null)
        {
            return;
        }

        if (_updateRestrictions.CustomHeaders != null)
        {
            AppendCustomParameters(operation, _updateRestrictions.CustomHeaders, ParameterLocation.Header);
        }

        if (_updateRestrictions.CustomQueryOptions != null)
        {
            AppendCustomParameters(operation, _updateRestrictions.CustomQueryOptions, ParameterLocation.Query);
        }
    }

    private IOpenApiSchema GetOpenApiSchema()
    {
        var schema = new OpenApiSchemaReference(ComplexPropertySegment.ComplexType.FullName(), _document);

        if (ComplexPropertySegment.Property.Type.IsCollection())
        {
            return new OpenApiSchema
            {
                Type = JsonSchemaType.Object,
                Properties = new Dictionary<string, IOpenApiSchema>
                {
                    {
                        "value",
                        new OpenApiSchema
                        {
                            Type = JsonSchemaType.Array,
                            Items = schema
                        }
                    }
                }
            };
        }
        else
        {
            return schema;
        }
    }
}