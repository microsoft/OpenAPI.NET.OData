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

namespace Microsoft.OpenApi.OData.Operation;

internal class ComplexPropertyPatchOperationHandler : ComplexPropertyBaseOperationHandler
{
    /// <inheritdoc />
    public override OperationType OperationType => OperationType.Patch;

    /// <inheritdoc/>
    protected override void SetBasicInfo(OpenApiOperation operation)
    {
        // Summary
        operation.Summary = $"Update property {ComplexPropertySegment.Property.Name} value.";

        // Description
        operation.Description = Context.Model.GetDescriptionAnnotation(ComplexPropertySegment.Property);

        // OperationId
        if (Context.Settings.EnableOperationId)
        {
            string typeName = ComplexPropertySegment.ComplexType.Name;
            operation.OperationId = ComplexPropertySegment.Property.Name + "." + typeName + ".Update" + Utils.UpperFirstChar(typeName);
        }
    }

    /// <inheritdoc/>
    protected override void SetRequestBody(OpenApiOperation operation)
    {
        OpenApiSchema schema =  ComplexPropertySegment.Property.Type.IsCollection() ?
            new OpenApiSchema
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
            }
        :
            new OpenApiSchema
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.Schema,
                    Id = ComplexPropertySegment.ComplexType.FullName()
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
        UpdateRestrictionsType update = Context.Model.GetRecord<UpdateRestrictionsType>(ComplexPropertySegment.Property, CapabilitiesConstants.UpdateRestrictions);
        if (update == null || update.Permissions == null)
        {
            return;
        }

        operation.Security = Context.CreateSecurityRequirements(update.Permissions).ToList();
    }

    protected override void AppendCustomParameters(OpenApiOperation operation)
    {
        UpdateRestrictionsType update = Context.Model.GetRecord<UpdateRestrictionsType>(ComplexPropertySegment.Property, CapabilitiesConstants.UpdateRestrictions);
        if (update == null)
        {
            return;
        }

        if (update.CustomHeaders != null)
        {
            AppendCustomParameters(operation, update.CustomHeaders, ParameterLocation.Header);
        }

        if (update.CustomQueryOptions != null)
        {
            AppendCustomParameters(operation, update.CustomQueryOptions, ParameterLocation.Query);
        }
    }
}