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

public class ComplexPropertyPostOperationHandlerTests
{
	private readonly ComplexPropertyPostOperationHandler _operationHandler = new();
	[Fact]
	public void CreateComplexPropertyPostOperationThrowsForSingle()
	{
		// Arrange
		var model = EntitySetPostOperationHandlerTests.GetEdmModel("");
		var entitySet = model.EntityContainer.FindEntitySet("Customers");
		var entity = entitySet.EntityType();
		var property = entity.FindProperty("BillingAddress");
		var settings = new OpenApiConvertSettings();
		var context = new ODataContext(model, settings);
		var path = new ODataPath(new ODataNavigationSourceSegment(entitySet), new ODataKeySegment(entitySet.EntityType()), new ODataComplexPropertySegment(property as IEdmStructuralProperty));

		// Act
		Assert.Throws<InvalidOperationException>(() => _operationHandler.CreateOperation(context, path));
	}
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void CreateComplexPropertyPostOperationReturnsCorrectOperationForCollection(bool enableOperationId)
	{
		// Arrange
		var model = EntitySetPostOperationHandlerTests.GetEdmModel("");
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
		var post = _operationHandler.CreateOperation(context, path);

		// Assert
		Assert.NotNull(post);
		Assert.Equal("Sets a new value for the collection of Address.", post.Summary);

		Assert.NotNull(post.Parameters);
		Assert.Equal(2, post.Parameters.Count); //id, etag

		Assert.NotNull(post.Responses);
		Assert.Equal(2, post.Responses.Count);
		Assert.Equal(new[] { "204", "default" }, post.Responses.Select(r => r.Key));

		if (enableOperationId)
		{
			Assert.Equal("AlternativeAddresses.Address.SetAddress", post.OperationId);
		}
		else
		{
			Assert.Null(post.OperationId);
		}
	}
}