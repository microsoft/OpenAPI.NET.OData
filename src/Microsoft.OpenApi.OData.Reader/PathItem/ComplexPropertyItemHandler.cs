
using Microsoft.OData.Edm;
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
		AddOperation(item, OperationType.Get);
		AddOperation(item, OperationType.Patch);
		if(Path.LastSegment is ODataComplexPropertySegment segment)
		{
			if(segment.Property.Type.IsNullable)
				AddOperation(item, OperationType.Delete);
			if(segment.Property.Type.IsCollection())
				AddOperation(item, OperationType.Post);
		}
	}
}