using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Generator;

namespace Microsoft.OpenApi.OData.Operation;

internal class ComplexPropertyGetOperationHandler : ComplexPropertyBaseOperationHandler
{
    /// <inheritdoc />
    public override OperationType OperationType => OperationType.Get;
    /// <inheritdoc/>
    protected override void SetResponses(OpenApiOperation operation)
    {
        SetSingleResponse(operation);
        operation.Responses.Add(Constants.StatusCodeDefault, Constants.StatusCodeDefault.GetResponse());

        base.SetResponses(operation);
    }
    private void SetSingleResponse(OpenApiOperation operation)
    {
        OpenApiSchema schema = null;

        if (schema == null)
        {
            schema = new OpenApiSchema
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.Schema,
                    Id = ComplexPropertySegment.ComplexType.FullName()
                }
            };
        }
        operation.Responses = new OpenApiResponses
        {
            {
                Constants.StatusCode200,
                new OpenApiResponse
                {
                    Description = "Result entities",
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
                }
            }
        };
    }
}