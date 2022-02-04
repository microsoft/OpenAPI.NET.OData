// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Generator;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;

namespace Microsoft.OpenApi.OData.Operation;

internal class ComplexPropertyPostOperationHandler : ComplexPropertyBaseOperationHandler
{
    /// <inheritdoc/>
    protected override void Initialize(ODataContext context, ODataPath path)
    {
        base.Initialize(context, path);
        if(!ComplexPropertySegment.Property.Type.IsCollection())
        {
            throw new InvalidOperationException("OData conventions do not support POSTing to a complex property that is not a collection.");
        }
    }
    /// <inheritdoc />
    public override OperationType OperationType => OperationType.Post;

    /// <inheritdoc/>
    protected override void SetBasicInfo(OpenApiOperation operation)
    {
        // Summary
        operation.Summary = $"Sets a new value for the collection of {ComplexPropertySegment.ComplexType.Name}.";

        // OperationId
        if (Context.Settings.EnableOperationId)
        {
            string typeName = ComplexPropertySegment.ComplexType.Name;
            operation.OperationId = ComplexPropertySegment.Property.Name + "." + typeName + ".Set" + Utils.UpperFirstChar(typeName);
        }

        base.SetBasicInfo(operation);
    }

    /// <inheritdoc/>
    protected override void SetParameters(OpenApiOperation operation)
    {
        base.SetParameters(operation);

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "If-Match",
            In = ParameterLocation.Header,
            Description = "ETag",
            Schema = new OpenApiSchema
            {
                Type = "string"
            }
        });
    }
    /// <inheritdoc/>
    protected override void SetRequestBody(OpenApiOperation operation)
    {
        OpenApiSchema schema = new()
        {
            Type = "array",
            Items = new OpenApiSchema
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.Schema,
                    Id = ComplexPropertySegment.ComplexType.FullName()
                }
            }
        };
        operation.RequestBody = new OpenApiRequestBody
        {
            Required = true,
            Description = "New property values",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                {
                    Constants.ApplicationJsonMediaType, new OpenApiMediaType
                    {
                        Schema = schema
                    }
                }
            }
        };

        base.SetRequestBody(operation);
    }
    /// <inheritdoc/>
    protected override void SetResponses(OpenApiOperation operation)
    {
        operation.AddErrorResponses(Context.Settings, true);
        base.SetResponses(operation);
    }

    protected override void SetSecurity(OpenApiOperation operation)
    {
        InsertRestrictionsType insert = Context.Model.GetRecord<InsertRestrictionsType>(ComplexPropertySegment.Property, CapabilitiesConstants.InsertRestrictions);
        if (insert == null || insert.Permissions == null)
        {
            return;
        }

        operation.Security = Context.CreateSecurityRequirements(insert.Permissions).ToList();
    }

    protected override void AppendCustomParameters(OpenApiOperation operation)
    {
        InsertRestrictionsType insert = Context.Model.GetRecord<InsertRestrictionsType>(ComplexPropertySegment.Property, CapabilitiesConstants.InsertRestrictions);
        if (insert == null)
        {
            return;
        }

        if (insert.CustomQueryOptions != null)
        {
            AppendCustomParameters(operation, insert.CustomQueryOptions, ParameterLocation.Query);
        }

        if (insert.CustomHeaders != null)
        {
            AppendCustomParameters(operation, insert.CustomHeaders, ParameterLocation.Header);
        }
    }
}