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

internal abstract class ComplexPropertyUpdateOperationHandler : ComplexPropertyBaseOperationHandler
{
    /// <summary>
    /// Gets/Sets the <see cref="UpdateRestrictionsType"/>
    /// </summary>
    private UpdateRestrictionsType UpdateRestrictions { get; set; }

    protected override void Initialize(ODataContext context, ODataPath path)
    {
        base.Initialize(context, path);

        UpdateRestrictions = Context.Model.GetRecord<UpdateRestrictionsType>(ComplexPropertySegment.Property, CapabilitiesConstants.UpdateRestrictions);
    }

    /// <inheritdoc/>
    protected override void SetBasicInfo(OpenApiOperation operation)
    {
        // Summary and Description
        string placeHolder = $"Update property {ComplexPropertySegment.Property.Name} value.";
        operation.Summary = UpdateRestrictions?.Description ?? placeHolder;
        operation.Description = UpdateRestrictions?.LongDescription ?? Context.Model.GetLongDescriptionAnnotation(ComplexPropertySegment.Property);

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
                    UnresolvedReference = true,
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
                UnresolvedReference = true,
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
        if (UpdateRestrictions?.Permissions == null)
        {
            return;
        }

        operation.Security = Context.CreateSecurityRequirements(UpdateRestrictions.Permissions).ToList();
    }

    protected override void AppendCustomParameters(OpenApiOperation operation)
    {
        if (UpdateRestrictions == null)
        {
            return;
        }

        if (UpdateRestrictions.CustomHeaders != null)
        {
            AppendCustomParameters(operation, UpdateRestrictions.CustomHeaders, ParameterLocation.Header);
        }

        if (UpdateRestrictions.CustomQueryOptions != null)
        {
            AppendCustomParameters(operation, UpdateRestrictions.CustomQueryOptions, ParameterLocation.Query);
        }
    }
}