// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Generator;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;

namespace Microsoft.OpenApi.OData.Operation;

internal class ComplexPropertyDeleteOperationHandler : ComplexPropertyBaseOperationHandler
{
    /// <inheritdoc />
    public override OperationType OperationType => OperationType.Delete;

    /// <inheritdoc/>
    protected override void SetBasicInfo(OpenApiOperation operation)
    {
        // Summary
        operation.Summary = $"Delete {ComplexPropertySegment.Property.Name} property value";

        // Description
        operation.Description = Context.Model.GetDescriptionAnnotation(ComplexPropertySegment.Property);

        // OperationId
        if (Context.Settings.EnableOperationId)
        {
            string typeName = ComplexPropertySegment.ComplexType.Name;
            operation.OperationId = ComplexPropertySegment.Property.Name + "." + typeName + ".Delete" + Utils.UpperFirstChar(typeName);
        }
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
    protected override void SetResponses(OpenApiOperation operation)
    {
        operation.AddErrorResponses(Context.Settings, true);
        base.SetResponses(operation);
    }
    protected override void SetSecurity(OpenApiOperation operation)
    {
        DeleteRestrictionsType delete = Context.Model.GetRecord<DeleteRestrictionsType>(ComplexPropertySegment.Property, CapabilitiesConstants.DeleteRestrictions);
        if (delete == null || delete.Permissions == null)
        {
            return;
        }

        operation.Security = Context.CreateSecurityRequirements(delete.Permissions).ToList();
    }

    protected override void AppendCustomParameters(OpenApiOperation operation)
    {
        DeleteRestrictionsType delete = Context.Model.GetRecord<DeleteRestrictionsType>(ComplexPropertySegment.Property, CapabilitiesConstants.DeleteRestrictions);
        if (delete == null)
        {
            return;
        }

        if (delete.CustomHeaders != null)
        {
            AppendCustomParameters(operation, delete.CustomHeaders, ParameterLocation.Header);
        }

        if (delete.CustomQueryOptions != null)
        {
            AppendCustomParameters(operation, delete.CustomQueryOptions, ParameterLocation.Query);
        }
    }
}