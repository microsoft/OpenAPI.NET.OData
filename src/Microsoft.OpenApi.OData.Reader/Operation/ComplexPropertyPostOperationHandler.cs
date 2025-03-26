// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.References;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Generator;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;

namespace Microsoft.OpenApi.OData.Operation;

internal class ComplexPropertyPostOperationHandler : ComplexPropertyBaseOperationHandler
{
    /// <summary>
    /// Initializes a new instance of <see cref="ComplexPropertyPostOperationHandler"/> class.
    /// </summary>
    /// <param name="document">The document to use to lookup references.</param>
    public ComplexPropertyPostOperationHandler(OpenApiDocument document):base(document)
    {
        
    }
    /// <inheritdoc/>
    protected override void Initialize(ODataContext context, ODataPath path)
    {
        base.Initialize(context, path);
        if (!ComplexPropertySegment.Property.Type.IsCollection())
        {
            throw new InvalidOperationException("OData conventions do not support POSTing to a complex property that is not a collection.");
        }

        if (!string.IsNullOrEmpty(TargetPath))
        {
            _insertRestrictions = Context?.Model.GetRecord<InsertRestrictionsType>(TargetPath, CapabilitiesConstants.InsertRestrictions);
        }
        
        var complexPropertyInsertRestrictions = Context?.Model.GetRecord<InsertRestrictionsType>(ComplexPropertySegment.Property, CapabilitiesConstants.InsertRestrictions);
        _insertRestrictions?.MergePropertiesIfNull(complexPropertyInsertRestrictions);
        _insertRestrictions ??= complexPropertyInsertRestrictions;
    }
    /// <inheritdoc />
    public override HttpMethod OperationType => HttpMethod.Post;

    private InsertRestrictionsType? _insertRestrictions;

    /// <inheritdoc/>
    protected override void SetBasicInfo(OpenApiOperation operation)
    {
        // OperationId
        if (Context is {Settings.EnableOperationId: true} && Path is not null)
        {
            operation.OperationId = EdmModelHelper.GenerateComplexPropertyPathOperationId(Path, Context, "Set");
        }

        // Summary and Description
        string placeHolder = $"Sets a new value for the collection of {ComplexPropertySegment.ComplexType.Name}.";
        operation.Summary = _insertRestrictions?.Description ?? placeHolder;
        operation.Description = _insertRestrictions?.LongDescription;

        base.SetBasicInfo(operation);
    }

    /// <inheritdoc/>
    protected override void SetParameters(OpenApiOperation operation)
    {
        base.SetParameters(operation);

        operation.Parameters ??= [];
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "If-Match",
            In = ParameterLocation.Header,
            Description = "ETag",
            Schema = new OpenApiSchema
            {
                Type = JsonSchemaType.String
            }
        });
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
        if (Context is not null)
            operation.AddErrorResponses(Context.Settings, _document, true, GetOpenApiSchema());
        base.SetResponses(operation);
    }

    protected override void SetSecurity(OpenApiOperation operation)
    {
        if (_insertRestrictions?.Permissions == null)
        {
            return;
        }

        operation.Security = Context?.CreateSecurityRequirements(_insertRestrictions.Permissions, _document).ToList();
    }

    protected override void AppendCustomParameters(OpenApiOperation operation)
    {
        if (_insertRestrictions == null)
        {
            return;
        }

        if (_insertRestrictions.CustomQueryOptions != null)
        {
            AppendCustomParameters(operation, _insertRestrictions.CustomQueryOptions, ParameterLocation.Query);
        }

        if (_insertRestrictions.CustomHeaders != null)
        {
            AppendCustomParameters(operation, _insertRestrictions.CustomHeaders, ParameterLocation.Header);
        }
    }

    private OpenApiSchema GetOpenApiSchema()
    {
        return new()
        {
            Type = JsonSchemaType.Array,
            Items = new OpenApiSchemaReference(ComplexPropertySegment.ComplexType.FullName(), _document)
        };
    }
}