using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Generator;

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
}