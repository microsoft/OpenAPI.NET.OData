
using System;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Edm;
using Xunit;

namespace Microsoft.OpenApi.OData.Operation.Tests;

public class ComplexPropertyDeleteOperationHandlerTests
{
	private readonly ComplexPropertyDeleteOperationHandler _operationHandler = new();
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void CreateComplexPropertyDeleteOperationReturnsCorrectOperationForSingle(bool enableOperationId)
	{
		// Arrange
		var model = EntitySetGetOperationHandlerTests.GetEdmModel("");
		var entitySet = model.EntityContainer.FindEntitySet("Customers");
		var entity = entitySet.EntityType();
		var property = entity.FindProperty("BillingAddress");
		var settings = new OpenApiConvertSettings
		{
			EnableOperationId = enableOperationId
		};
		var context = new ODataContext(model, settings);
		var path = new ODataPath(new ODataNavigationSourceSegment(entitySet), new ODataKeySegment(entitySet.EntityType()), new ODataComplexPropertySegment(property as IEdmStructuralProperty));

		// Act
		var delete = _operationHandler.CreateOperation(context, path);

		// Assert
		Assert.NotNull(delete);
		Assert.Equal("Delete BillingAddress property value", delete.Summary);

		Assert.NotNull(delete.Parameters);
		Assert.Equal(2, delete.Parameters.Count); //id, etag

		Assert.NotNull(delete.Responses);
		Assert.Equal(2, delete.Responses.Count);
		Assert.Equal(new[] { "204", "default" }, delete.Responses.Select(r => r.Key));

		if (enableOperationId)
		{
			Assert.Equal("BillingAddress.Address.DeleteAddress", delete.OperationId);
		}
		else
		{
			Assert.Null(delete.OperationId);
		}
	}
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void CreateComplexPropertyPostOperationReturnsCorrectOperationForCollection(bool enableOperationId)
	{
		// Arrange
		var model = EntitySetGetOperationHandlerTests.GetEdmModel("");
		var entitySet = model.EntityContainer.FindEntitySet("Customers");
		var entity = entitySet.EntityType();
		var property = entity.FindProperty("AlternativeAddresses");
		var settings = new OpenApiConvertSettings
		{
			EnableOperationId = enableOperationId
		};
		var context = new ODataContext(model, settings);
		var path = new ODataPath(new ODataNavigationSourceSegment(entitySet), new ODataKeySegment(entitySet.EntityType()), new ODataComplexPropertySegment(property as IEdmStructuralProperty));

		// Act
		var delete = _operationHandler.CreateOperation(context, path);

		// Assert
		Assert.NotNull(delete);
		Assert.Equal("Delete AlternativeAddresses property value", delete.Summary);

		Assert.NotNull(delete.Parameters);
		Assert.Equal(2, delete.Parameters.Count); //id, etag

		Assert.NotNull(delete.Responses);
		Assert.Equal(2, delete.Responses.Count);
		Assert.Equal(new[] { "204", "default" }, delete.Responses.Select(r => r.Key));

		if (enableOperationId)
		{
			Assert.Equal("AlternativeAddresses.Address.DeleteAddress", delete.OperationId);
		}
		else
		{
			Assert.Null(delete.OperationId);
		}
	}
}