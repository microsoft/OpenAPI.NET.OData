// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Edm;
using Xunit;

namespace Microsoft.OpenApi.OData.PathItem.Tests;


public class ComplexPropertyPathItemHandlerTests
{
	private readonly ComplexPropertyItemHandler _pathItemHandler = new();
	[Fact]
	public void CreatePathItemThrowsForNullContext()
	{
		Assert.Throws<ArgumentNullException>("context",
			() => _pathItemHandler.CreatePathItem(context: null, path: new ODataPath()));
	}
	[Fact]
	public void SetsDefaultOperations()
	{
		var model = EntitySetPathItemHandlerTests.GetEdmModel(annotation: "");
		var context = new ODataContext(model);
		var entitySet = model.EntityContainer.FindEntitySet("Customers");
		Assert.NotNull(entitySet); // guard
		var entityType = entitySet.EntityType();
		var property = entityType.FindProperty("BillingAddress");
		Assert.NotNull(property); // guard
		var path = new ODataPath(new ODataNavigationSourceSegment(entitySet), new ODataKeySegment(entityType), new ODataComplexPropertySegment(property as IEdmStructuralProperty));
		Assert.Equal(ODataPathKind.ComplexProperty, path.Kind); // guard
		var pathItem = _pathItemHandler.CreatePathItem(context, path);
		Assert.NotNull(pathItem);
		Assert.Equal(3, pathItem.Operations.Count);
		Assert.True(pathItem.Operations.ContainsKey(OperationType.Get));
		Assert.True(pathItem.Operations.ContainsKey(OperationType.Patch));
		Assert.True(pathItem.Operations.ContainsKey(OperationType.Delete));
	}

	[Fact]
	public void SetsPutUpdateOperationWithUpdateMethodUpdateRestrictions()
    {
		string annotation = $@"
<Annotation Term=""Org.OData.Capabilities.V1.UpdateRestrictions"">
  <Record>
    <PropertyValue Property=""UpdateMethod"">
      <EnumMember>Org.OData.Capabilities.V1.HttpMethod/PUT</EnumMember>
    </PropertyValue>
  </Record>
</Annotation>";
		string target = $@"""NS.Default/Customers/BillingAddress""";

		var model = EntitySetPathItemHandlerTests.GetEdmModel(annotation: annotation);
		var context = new ODataContext(model);
		var entitySet = model.EntityContainer.FindEntitySet("Customers");
		Assert.NotNull(entitySet); // guard
		var entityType = entitySet.EntityType();
		var property = entityType.FindProperty("BillingAddress");
		Assert.NotNull(property); // guard
		var path = new ODataPath(new ODataNavigationSourceSegment(entitySet), new ODataKeySegment(entityType), new ODataComplexPropertySegment(property as IEdmStructuralProperty));
		Assert.Equal(ODataPathKind.ComplexProperty, path.Kind); // guard
		var pathItem = _pathItemHandler.CreatePathItem(context, path);
		Assert.NotNull(pathItem);
		Assert.Equal(3, pathItem.Operations.Count);
		Assert.True(pathItem.Operations.ContainsKey(OperationType.Get));
		Assert.True(pathItem.Operations.ContainsKey(OperationType.Patch));
		Assert.True(pathItem.Operations.ContainsKey(OperationType.Delete));
	}

	[Fact]
	public void DoesntSetDeleteOnNonNullableProperties()
	{
		var model = EntitySetPathItemHandlerTests.GetEdmModel(annotation: "");
		var context = new ODataContext(model);
		var entitySet = model.EntityContainer.FindEntitySet("Customers");
		Assert.NotNull(entitySet); // guard
		var entityType = entitySet.EntityType();
		var property = entityType.FindProperty("MailingAddress");
		Assert.NotNull(property); // guard
		var path = new ODataPath(new ODataNavigationSourceSegment(entitySet), new ODataKeySegment(entityType), new ODataComplexPropertySegment(property as IEdmStructuralProperty));
		Assert.Equal(ODataPathKind.ComplexProperty, path.Kind); // guard
		var pathItem = _pathItemHandler.CreatePathItem(context, path);
		Assert.NotNull(pathItem);
		Assert.Equal(2, pathItem.Operations.Count);
		Assert.False(pathItem.Operations.ContainsKey(OperationType.Delete));
	}
	[Fact]
	public void SetsPostOnCollectionProperties()
	{
		var model = EntitySetPathItemHandlerTests.GetEdmModel(annotation: "");
		var context = new ODataContext(model);
		var entitySet = model.EntityContainer.FindEntitySet("Customers");
		Assert.NotNull(entitySet); // guard
		var entityType = entitySet.EntityType();
		var property = entityType.FindProperty("AlternativeAddresses");
		Assert.NotNull(property); // guard
		var path = new ODataPath(new ODataNavigationSourceSegment(entitySet), new ODataKeySegment(entityType), new ODataComplexPropertySegment(property as IEdmStructuralProperty));
		Assert.Equal(ODataPathKind.ComplexProperty, path.Kind); // guard
		var pathItem = _pathItemHandler.CreatePathItem(context, path);
		Assert.NotNull(pathItem);
		Assert.Equal(3, pathItem.Operations.Count);
		Assert.True(pathItem.Operations.ContainsKey(OperationType.Post));
	}
}