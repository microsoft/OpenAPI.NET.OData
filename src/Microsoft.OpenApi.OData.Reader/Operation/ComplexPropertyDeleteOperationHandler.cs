using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Generator;

namespace Microsoft.OpenApi.OData.Operation;

internal class ComplexPropertyDeleteOperationHandler : ComplexPropertyBaseOperationHandler
{
    /// <inheritdoc />
    public override OperationType OperationType => OperationType.Delete;

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
        operation.Responses = new OpenApiResponses
        {
            { Constants.StatusCode204, Constants.StatusCode204.GetResponse() },
            { Constants.StatusCodeDefault, Constants.StatusCodeDefault.GetResponse() }
        };

        base.SetResponses(operation);
    }
}