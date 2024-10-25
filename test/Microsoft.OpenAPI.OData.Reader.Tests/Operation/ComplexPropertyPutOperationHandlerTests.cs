// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Edm;
using Xunit;

namespace Microsoft.OpenApi.OData.Operation.Tests;

public class ComplexPropertyPutOperationHandlerTests
{
	private readonly ComplexPropertyPutOperationHandler _operationHandler = new();

	[Theory]
	[InlineData(true, true)]
	[InlineData(false, true)]
	[InlineData(true, false)]
	[InlineData(false, false)]
	public void CreateComplexPropertyPutOperationReturnsCorrectOperationForSingle(bool enableOperationId, bool useHTTPStatusCodeClass2XX)
	{
		// Arrange
		var model = EntitySetGetOperationHandlerTests.GetEdmModel("");
		var entitySet = model.EntityContainer.FindEntitySet("Customers");
		var entity = entitySet.EntityType;
		var property = entity.FindProperty("BillingAddress");
		var settings = new OpenApiConvertSettings
		{
			EnableOperationId = enableOperationId,
			UseSuccessStatusCodeRange = useHTTPStatusCodeClass2XX
		};
		var context = new ODataContext(model, settings);
		var path = new ODataPath(new ODataNavigationSourceSegment(entitySet), new ODataKeySegment(entitySet.EntityType), new ODataComplexPropertySegment(property as IEdmStructuralProperty));

		// Act
		var put = _operationHandler.CreateOperation(context, path);

		// Assert
		Assert.NotNull(put);
		Assert.Equal("Update the BillingAddress.", put.Summary);
		Assert.Equal("Update the BillingAddress value.", put.Description);

		Assert.NotNull(put.Parameters);
		Assert.Single(put.Parameters); //id

		Assert.NotNull(put.Responses);
		Assert.Equal(2, put.Responses.Count);
		var statusCode = useHTTPStatusCodeClass2XX ? "2XX" : "204";
		Assert.Equal(new[] { statusCode, "default" }, put.Responses.Select(r => r.Key));

		if (useHTTPStatusCodeClass2XX)
        {
			Assert.Single(put.Responses[statusCode].Content);
		}
		else
		{
			Assert.Empty(put.Responses[statusCode].Content);
		}

		if (enableOperationId)
		{
			Assert.Equal("Customers.SetBillingAddress", put.OperationId);
		}
		else
		{
			Assert.Null(put.OperationId);
		}
	}
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void CreateComplexPropertyPutOperationReturnsCorrectOperationForCollection(bool enableOperationId)
	{
		// Arrange
		var model = EntitySetGetOperationHandlerTests.GetEdmModel("");
		var entitySet = model.EntityContainer.FindEntitySet("Customers");
		var entity = entitySet.EntityType;
		var property = entity.FindProperty("AlternativeAddresses");
		var settings = new OpenApiConvertSettings
		{
			EnableOperationId = enableOperationId
		};
		var context = new ODataContext(model, settings);
		var path = new ODataPath(new ODataNavigationSourceSegment(entitySet), new ODataKeySegment(entitySet.EntityType), new ODataComplexPropertySegment(property as IEdmStructuralProperty));

		// Act
		var put = _operationHandler.CreateOperation(context, path);

		// Assert
		Assert.NotNull(put);
		Assert.Equal("Update the AlternativeAddresses.", put.Summary);
		Assert.Equal("Update the AlternativeAddresses value.", put.Description);

		Assert.NotNull(put.Parameters);
		Assert.Single(put.Parameters); //id

		Assert.NotNull(put.Responses);
		Assert.Equal(2, put.Responses.Count);
		Assert.Equal(new[] { "204", "default" }, put.Responses.Select(r => r.Key));
		var schema = put.RequestBody?.Content.FirstOrDefault().Value?.Schema;

        Assert.NotNull(schema);
		Assert.Equal("object", schema.Type);
		Assert.Equal("value", schema.Properties.FirstOrDefault().Key);
        Assert.Equal("array", schema.Properties.FirstOrDefault().Value.Type);

        if (enableOperationId)
		{
			Assert.Equal("Customers.SetAlternativeAddresses", put.OperationId);
		}
		else
		{
			Assert.Null(put.OperationId);
		}
	}
}
