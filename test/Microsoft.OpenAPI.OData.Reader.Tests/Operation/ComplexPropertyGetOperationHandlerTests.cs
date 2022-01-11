using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Edm;
using Xunit;

namespace Microsoft.OpenApi.OData.Operation.Tests;

public class ComplexPropertyGetOperationHandlerTests
{
	private readonly ComplexPropertyGetOperationHandler _operationHandler = new();

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void CreateComplexPropertyGetOperationReturnsCorrectOperationForSingle(bool enableOperationId)
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
		var get = _operationHandler.CreateOperation(context, path);

		// Assert
		Assert.NotNull(get);
		Assert.Equal("Get BillingAddress property value", get.Summary);

		Assert.NotNull(get.Parameters);
		Assert.Equal(3, get.Parameters.Count); //id, select, expand

		Assert.NotNull(get.Responses);
		Assert.Equal(2, get.Responses.Count);
		Assert.Equal(new[] { "200", "default" }, get.Responses.Select(r => r.Key));

		if (enableOperationId)
		{
			Assert.Equal("BillingAddress.Address.GetAddress", get.OperationId);
		}
		else
		{
			Assert.Null(get.OperationId);
		}
	}
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void CreateComplexPropertyGetOperationReturnsCorrectOperationForCollection(bool enableOperationId)
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
		var get = _operationHandler.CreateOperation(context, path);

		// Assert
		Assert.NotNull(get);
		Assert.Equal("Get AlternativeAddresses property value", get.Summary);

		Assert.NotNull(get.Parameters);
		Assert.Equal(9, get.Parameters.Count); //id, select, expand, order, top, skip, count, search, filter

		Assert.NotNull(get.Responses);
		Assert.Equal(2, get.Responses.Count);
		Assert.Equal(new[] { "200", "default" }, get.Responses.Select(r => r.Key));

		if (enableOperationId)
		{
			Assert.Equal("AlternativeAddresses.Address.ListAddress", get.OperationId);
		}
		else
		{
			Assert.Null(get.OperationId);
		}
	}
}