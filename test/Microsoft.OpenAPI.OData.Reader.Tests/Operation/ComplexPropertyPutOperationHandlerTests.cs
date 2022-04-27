// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
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
	public void CreateComplexPropertyDeleteOperationReturnsCorrectOperationForSingle(bool enableOperationId, bool useHTTPStatusCodeClass2XX)
	{
		// Arrange
		var model = EntitySetGetOperationHandlerTests.GetEdmModel("");
		var entitySet = model.EntityContainer.FindEntitySet("Customers");
		var entity = entitySet.EntityType();
		var property = entity.FindProperty("BillingAddress");
		var settings = new OpenApiConvertSettings
		{
			EnableOperationId = enableOperationId,
			UseHTTPStatusCodeClass2XX = useHTTPStatusCodeClass2XX
		};
		var context = new ODataContext(model, settings);
		var path = new ODataPath(new ODataNavigationSourceSegment(entitySet), new ODataKeySegment(entitySet.EntityType()), new ODataComplexPropertySegment(property as IEdmStructuralProperty));

		// Act
		var put = _operationHandler.CreateOperation(context, path);

		// Assert
		Assert.NotNull(put);
		Assert.Equal("Update the BillingAddress.", put.Summary);
		Assert.Equal("Update the BillingAddress value.", put.Description);

		Assert.NotNull(put.Parameters);
		Assert.Equal(1, put.Parameters.Count); //id

		Assert.NotNull(put.Responses);
		Assert.Equal(2, put.Responses.Count);
		var statusCode = useHTTPStatusCodeClass2XX ? "2XX" : "204";
		Assert.Equal(new[] { statusCode, "default" }, put.Responses.Select(r => r.Key));

		if (enableOperationId)
		{
			Assert.Equal("BillingAddress.Address.UpdateAddress", put.OperationId);
		}
		else
		{
			Assert.Null(put.OperationId);
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
		var property = entity.FindProperty("BillingAddress");
		var settings = new OpenApiConvertSettings
		{
			EnableOperationId = enableOperationId
		};
		var context = new ODataContext(model, settings);
		var path = new ODataPath(new ODataNavigationSourceSegment(entitySet), new ODataKeySegment(entitySet.EntityType()), new ODataComplexPropertySegment(property as IEdmStructuralProperty));

		// Act
		var put = _operationHandler.CreateOperation(context, path);

		// Assert
		Assert.NotNull(put);
		Assert.Equal("Update the BillingAddress.", put.Summary);
		Assert.Equal("Update the BillingAddress value.", put.Description);

		Assert.NotNull(put.Parameters);
		Assert.Equal(1, put.Parameters.Count); //id

		Assert.NotNull(put.Responses);
		Assert.Equal(2, put.Responses.Count);
		Assert.Equal(new[] { "204", "default" }, put.Responses.Select(r => r.Key));

		if (enableOperationId)
		{
			Assert.Equal("BillingAddress.Address.UpdateAddress", put.OperationId);
		}
		else
		{
			Assert.Null(put.OperationId);
		}
	}
}
