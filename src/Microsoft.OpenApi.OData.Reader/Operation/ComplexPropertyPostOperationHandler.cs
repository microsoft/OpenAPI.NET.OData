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
    /// <inheritdoc />
    public override OperationType OperationType => OperationType.Post;

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
        operation.Responses = new OpenApiResponses
        {
            { Constants.StatusCode204, Constants.StatusCode204.GetResponse() },
            { Constants.StatusCodeDefault, Constants.StatusCodeDefault.GetResponse() }
        };

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