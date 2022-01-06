using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Generator;

namespace Microsoft.OpenApi.OData.Operation;

internal class ComplexPropertyGetOperationHandler : OperationHandler
{
    /// <inheritdoc />
    public override OperationType OperationType => OperationType.Get;

    /// <inheritdoc/>
    protected override void Initialize(ODataContext context, ODataPath path)
	{
		complexPropertySegment = path.LastSegment as ODataComplexPropertySegment ?? throw Error.ArgumentNull(nameof(path));
	}
	private ODataComplexPropertySegment complexPropertySegment;

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
					Id = complexPropertySegment.ComplexType.FullName()
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