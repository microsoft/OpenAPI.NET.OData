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

public class ComplexPropertyPatchOperationHandlerTests
{
	private readonly ComplexPropertyPatchOperationHandler _operationHandler = new();

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void CreateComplexPropertyDeleteOperationReturnsCorrectOperationForSingle(bool enableOperationId)
	{
		// Arrange
		var model = EntitySetGetOperationHandlerTests.GetEdmModel("");
		var entitySet = model.EntityContainer.FindEntitySet("Customers");
		var entity = entitySet.EntityType;
		var property = entity.FindProperty("BillingAddress");
		var settings = new OpenApiConvertSettings
		{
			EnableOperationId = enableOperationId
		};
		var context = new ODataContext(model, settings);
		var path = new ODataPath(new ODataNavigationSourceSegment(entitySet), new ODataKeySegment(entitySet.EntityType), new ODataComplexPropertySegment(property as IEdmStructuralProperty));

		// Act
		var patch = _operationHandler.CreateOperation(context, path);

		// Assert
		Assert.NotNull(patch);
		Assert.Equal("Update the BillingAddress.", patch.Summary);
		Assert.Equal("Update the BillingAddress value.", patch.Description);

		Assert.NotNull(patch.Parameters);
		Assert.Single(patch.Parameters); //id

		Assert.NotNull(patch.Responses);
		Assert.Equal(2, patch.Responses.Count);
		Assert.Equal(new[] { "204", "default" }, patch.Responses.Select(r => r.Key));

		if (enableOperationId)
		{
			Assert.Equal("Customers.UpdateBillingAddress", patch.OperationId);
		}
		else
		{
			Assert.Null(patch.OperationId);
		}
	}
	[Theory]
	[InlineData(true, true)]
	[InlineData(false, true)]
	[InlineData(true, false)]
	[InlineData(false, false)]
	public void CreateComplexPropertyPatchOperationReturnsCorrectOperationForCollection(bool enableOperationId, bool useHTTPStatusCodeClass2XX)
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
		var patch = _operationHandler.CreateOperation(context, path);

		// Assert
		Assert.NotNull(patch);
		Assert.Equal("Update the BillingAddress.", patch.Summary);
        Assert.Equal("Update the BillingAddress value.", patch.Description);

		Assert.NotNull(patch.Parameters);
		Assert.Single(patch.Parameters); //id

		Assert.NotNull(patch.Responses);
		Assert.Equal(2, patch.Responses.Count);
		var statusCode = useHTTPStatusCodeClass2XX ? "2XX" : "204";
		Assert.Equal(new[] { statusCode, "default" }, patch.Responses.Select(r => r.Key));

		if (useHTTPStatusCodeClass2XX)
		{
			Assert.Single(patch.Responses[statusCode].Content);
		}
		else
		{
			Assert.Empty(patch.Responses[statusCode].Content);
		}

		if (enableOperationId)
		{
			Assert.Equal("Customers.UpdateBillingAddress", patch.OperationId);
		}
		else
		{
			Assert.Null(patch.OperationId);
		}
	}
}