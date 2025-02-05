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
		var entity = entitySet.EntityType;
		var property = entity.FindProperty("BillingAddress");
		var settings = new OpenApiConvertSettings();
		var context = new ODataContext(model, settings);
		var path = new ODataPath(new ODataNavigationSourceSegment(entitySet), new ODataKeySegment(entitySet.EntityType), new ODataComplexPropertySegment(property as IEdmStructuralProperty));

		// Act
		Assert.Throws<InvalidOperationException>(() => _operationHandler.CreateOperation(context, path));
	}
	[Theory]
	[InlineData(true, true)]
	[InlineData(false, true)]
	[InlineData(true, false)]
	[InlineData(false, false)]
	public void CreateComplexPropertyPostOperationReturnsCorrectOperationForCollection(bool enableOperationId, bool useHTTPStatusCodeClass2XX)
	{
		// Arrange
		var model = EntitySetPostOperationHandlerTests.GetEdmModel("");
		var entitySet = model.EntityContainer.FindEntitySet("Customers");
		var entity = entitySet.EntityType;
		var property = entity.FindProperty("AlternativeAddresses");
		var settings = new OpenApiConvertSettings
		{
			EnableOperationId = enableOperationId,
			UseSuccessStatusCodeRange = useHTTPStatusCodeClass2XX
		};
		var context = new ODataContext(model, settings);
		var path = new ODataPath(new ODataNavigationSourceSegment(entitySet), new ODataKeySegment(entitySet.EntityType), new ODataComplexPropertySegment(property as IEdmStructuralProperty));

		// Act
		var post = _operationHandler.CreateOperation(context, path);

		// Assert
		Assert.NotNull(post);
		Assert.Equal("Create AlternativeAddress.", post.Summary);
		Assert.Equal("Create a new AlternativeAddress.", post.Description);

		Assert.NotNull(post.Parameters);
		Assert.Equal(2, post.Parameters.Count); //id, etag

		Assert.NotNull(post.Responses);
		Assert.Equal(2, post.Responses.Count);
		var statusCode = useHTTPStatusCodeClass2XX ? "2XX" : "204";
		Assert.Equal(new[] { statusCode, "default" }, post.Responses.Select(r => r.Key));

		if (useHTTPStatusCodeClass2XX)
		{
			Assert.Single(post.Responses[statusCode].Content);
		}
		else
		{
			Assert.Empty(post.Responses[statusCode].Content);
		}

		if (enableOperationId)
		{
			Assert.Equal("Customers.SetAlternativeAddresses", post.OperationId);
		}
		else
		{
			Assert.Null(post.OperationId);
		}
	}
}