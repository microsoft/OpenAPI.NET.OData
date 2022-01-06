
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.PathItem;

internal class ComplexPropertyItemHandler : PathItemHandler
{
    /// <inheritdoc/>
	protected override ODataPathKind HandleKind => ODataPathKind.ComplexProperty;

    /// <inheritdoc/>
	protected override void SetOperations(OpenApiPathItem item)
	{
		AddOperation(item, OperationType.Get); //TODO post patch delete
	}
}