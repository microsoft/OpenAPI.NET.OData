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
		var entity = entitySet.EntityType();
		var property = entity.FindProperty("BillingAddress");
		var settings = new OpenApiConvertSettings
		{
			EnableOperationId = enableOperationId
		};
		var context = new ODataContext(model, settings);
		var path = new ODataPath(new ODataNavigationSourceSegment(entitySet), new ODataKeySegment(entitySet.EntityType()), new ODataComplexPropertySegment(property as IEdmStructuralProperty));

		// Act
		var patch = _operationHandler.CreateOperation(context, path);

		// Assert
		Assert.NotNull(patch);
		Assert.Equal("Update property BillingAddress value.", patch.Summary);
		Assert.Equal("Update the BillingAddress.", patch.Description);

		Assert.NotNull(patch.Parameters);
		Assert.Equal(1, patch.Parameters.Count); //id

		Assert.NotNull(patch.Responses);
		Assert.Equal(2, patch.Responses.Count);
		Assert.Equal(new[] { "204", "default" }, patch.Responses.Select(r => r.Key));

		if (enableOperationId)
		{
			Assert.Equal("BillingAddress.Address.UpdateAddress", patch.OperationId);
		}
		else
		{
			Assert.Null(patch.OperationId);
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
		var patch = _operationHandler.CreateOperation(context, path);

		// Assert
		Assert.NotNull(patch);
		Assert.Equal("Update property AlternativeAddresses value.", patch.Summary);
		Assert.Null(patch.Description);

		Assert.NotNull(patch.Parameters);
		Assert.Equal(1, patch.Parameters.Count); //id

		Assert.NotNull(patch.Responses);
		Assert.Equal(2, patch.Responses.Count);
		Assert.Equal(new[] { "204", "default" }, patch.Responses.Select(r => r.Key));

		if (enableOperationId)
		{
			Assert.Equal("AlternativeAddresses.Address.UpdateAddress", patch.OperationId);
		}
		else
		{
			Assert.Null(patch.OperationId);
		}
	}
}