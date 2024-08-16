// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
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

	[Theory]
	[InlineData(true, true, 2)]
	[InlineData(true, false, 0)]
	[InlineData(false, false, 2)]
	[InlineData(false, true, 2)]
	public void SetsDefaultOperations(bool useAnnotationToGeneratePath, bool annotationAvailable, int operationCount)
	{
		var annotation = annotationAvailable
			? @"
<Annotation Term=""Org.OData.Capabilities.V1.UpdateRestrictions"">
  <Record>
	<PropertyValue Property=""Updatable"" Bool=""true"" />
  </Record>
</Annotation>
<Annotation Term=""Org.OData.Capabilities.V1.ReadRestrictions"">
  <Record>
	<PropertyValue Property=""Readable"" Bool=""true"" />
  </Record>
</Annotation>"
			: "";
		var target = @"""NS.Customer/BillingAddress""";
		var model = EntitySetPathItemHandlerTests.GetEdmModel(annotation: annotation, target: target);
		var convertSettings = new OpenApiConvertSettings
		{
			RequireRestrictionAnnotationsToGenerateComplexPropertyPaths = useAnnotationToGeneratePath
		};
		var context = new ODataContext(model, convertSettings);
		var entitySet = model.EntityContainer.FindEntitySet("Customers");
		Assert.NotNull(entitySet); // guard
		var entityType = entitySet.EntityType;
		var property = entityType.FindProperty("BillingAddress");
		Assert.NotNull(property); // guard
		var path = new ODataPath(new ODataNavigationSourceSegment(entitySet), new ODataKeySegment(entityType), new ODataComplexPropertySegment(property as IEdmStructuralProperty));
		Assert.Equal(ODataPathKind.ComplexProperty, path.Kind); // guard
		var pathItem = _pathItemHandler.CreatePathItem(context, path);
		Assert.NotNull(pathItem);
		Assert.Equal(operationCount, pathItem.Operations.Count);

		if (operationCount > 0)
		{
			Assert.True(pathItem.Operations.ContainsKey(OperationType.Get));
			Assert.True(pathItem.Operations.ContainsKey(OperationType.Patch));
		}
		else
		{
			Assert.False(pathItem.Operations.ContainsKey(OperationType.Get));
			Assert.False(pathItem.Operations.ContainsKey(OperationType.Patch));
		}
	}

	[Theory]
	[InlineData(true, true, 1)]
	[InlineData(true, false, 0)]
	[InlineData(false, false, 2)]
	[InlineData(false, true, 2)]
	public void SetsUpdateOperationWithUpdateMethodUpdateRestrictions(bool useAnnotationToGeneratePath, bool annotationAvailable, int operationCount)
    {
        var annotation = annotationAvailable 
			? @"
<Annotation Term=""Org.OData.Capabilities.V1.UpdateRestrictions"">
  <Record>
    <PropertyValue Property=""UpdateMethod"">
      <EnumMember>Org.OData.Capabilities.V1.HttpMethod/PUT</EnumMember>
    </PropertyValue>
	<PropertyValue Property=""Updatable"" Bool=""true"" />
  </Record>
</Annotation>
<Annotation Term=""Org.OData.Capabilities.V1.ReadRestrictions"">
  <Record>
	<PropertyValue Property=""Readable"" Bool=""false"" />
  </Record>
</Annotation>"
			: "";
        var target = @"""NS.Customer/BillingAddress""";
        var model = EntitySetPathItemHandlerTests.GetEdmModel(annotation: annotation, target: target);
		var convertSettings = new OpenApiConvertSettings
		{
			RequireRestrictionAnnotationsToGenerateComplexPropertyPaths = useAnnotationToGeneratePath
		};
		var context = new ODataContext(model, convertSettings);
        var entitySet = model.EntityContainer.FindEntitySet("Customers");
        Assert.NotNull(entitySet); // guard
        var entityType = entitySet.EntityType;
        var property = entityType.FindProperty("BillingAddress");
        Assert.NotNull(property); // guard
        var path = new ODataPath(new ODataNavigationSourceSegment(entitySet), new ODataKeySegment(entityType), new ODataComplexPropertySegment(property as IEdmStructuralProperty));
        Assert.Equal(ODataPathKind.ComplexProperty, path.Kind); // guard
        var pathItem = _pathItemHandler.CreatePathItem(context, path);
        Assert.NotNull(pathItem);
        Assert.Equal(operationCount, pathItem.Operations.Count);

		if (operationCount > 0)
		{
			if (annotationAvailable)
			{
				Assert.True(pathItem.Operations.ContainsKey(OperationType.Put));
			}
            else
            {
				Assert.True(pathItem.Operations.ContainsKey(OperationType.Get));
				Assert.True(pathItem.Operations.ContainsKey(OperationType.Patch));
			}
		}
		else
		{
			Assert.False(pathItem.Operations.ContainsKey(OperationType.Patch));
			Assert.False(pathItem.Operations.ContainsKey(OperationType.Put));
		}
    }

	[Theory]
	[InlineData(true, true, 1)]
	[InlineData(true, false, 0)]
	[InlineData(false, false, 3)]
	[InlineData(false, true, 3)]
	public void SetsPostOnCollectionProperties(bool useAnnotationToGeneratePath, bool annotationAvailable, int operationCount)
	{
		var annotation = annotationAvailable
			? @"
<Annotation Term=""Org.OData.Capabilities.V1.InsertRestrictions"">
    <Record>
    <PropertyValue Property=""Insertable"" Bool=""true"" />
    </Record>
</Annotation>
<Annotation Term=""Org.OData.Capabilities.V1.ReadRestrictions"">
  <Record>
	<PropertyValue Property=""Description"" String=""Create groupLifecyclePolicy"" />
	<!-- No Readable property defined! -->
  </Record>
</Annotation>"
			: "";
		var target = @"""NS.Customer/AlternativeAddresses""";
		var model = EntitySetPathItemHandlerTests.GetEdmModel(annotation: annotation, target: target);
		var convertSettings = new OpenApiConvertSettings
		{
			RequireRestrictionAnnotationsToGenerateComplexPropertyPaths = useAnnotationToGeneratePath
		};
		var context = new ODataContext(model, convertSettings);
		var entitySet = model.EntityContainer.FindEntitySet("Customers");
		Assert.NotNull(entitySet); // guard
		var entityType = entitySet.EntityType;
		var property = entityType.FindProperty("AlternativeAddresses");
		Assert.NotNull(property); // guard
		var path = new ODataPath(new ODataNavigationSourceSegment(entitySet), new ODataKeySegment(entityType), new ODataComplexPropertySegment(property as IEdmStructuralProperty));
		Assert.Equal(ODataPathKind.ComplexProperty, path.Kind); // guard
		var pathItem = _pathItemHandler.CreatePathItem(context, path);
		Assert.NotNull(pathItem);
		Assert.Equal(operationCount, pathItem.Operations.Count);

		if (operationCount > 0)
        {
			Assert.True(pathItem.Operations.ContainsKey(OperationType.Post));
		}
        else
        {
			Assert.False(pathItem.Operations.ContainsKey(OperationType.Post));
		}		
	}
	[Fact]
	public void CreateComplexPropertyPathItemAddsCustomAttributeValuesToPathExtensions()
    {
		// Arrange
		var model = EntitySetPathItemHandlerTests.GetEdmModel("");
		ODataContext context = new(model);
		context.Settings.CustomXMLAttributesMapping.Add("ags:IsHidden", "x-ms-isHidden");
		var entitySet = model.EntityContainer.FindEntitySet("Customers");
		Assert.NotNull(entitySet); // guard
		var entityType = entitySet.EntityType;
		var property = entityType.FindProperty("AlternativeAddresses");
		Assert.NotNull(property); // guard
		var path = new ODataPath(new ODataNavigationSourceSegment(entitySet), new ODataKeySegment(entityType), new ODataComplexPropertySegment(property as IEdmStructuralProperty));
        Assert.Equal(ODataPathKind.ComplexProperty, path.Kind); // guard

        // Act
        var pathItem = _pathItemHandler.CreatePathItem(context, path);

		// Assert
		Assert.NotNull(pathItem.Extensions);

		pathItem.Extensions.TryGetValue("x-ms-isHidden", out IOpenApiExtension isHiddenExtension);
		string isHiddenValue = (isHiddenExtension as OpenApiString)?.Value;
		Assert.Equal("true", isHiddenValue);
	}

	[Theory]
	[InlineData("true", "true", "true", 3)]
	[InlineData("false", "false", "false", 0)]
	[InlineData ("false", "false", "true", 1)]
    public void CreatesComplexPropertyPathsBasedOnTargetPathAnnotations(string readable, string insertable, string updatable, int operationCount)
    {
		// Arrange
        var annotation = $@"
<Annotation Term=""Org.OData.Capabilities.V1.InsertRestrictions"">
    <Record>
    <PropertyValue Property=""Insertable"" Bool=""{insertable}"" />
    </Record>
</Annotation>
<Annotation Term=""Org.OData.Capabilities.V1.ReadRestrictions"">
  <Record>
	<PropertyValue Property=""Readable"" Bool=""{readable}"" />
  </Record>
</Annotation>
<Annotation Term=""Org.OData.Capabilities.V1.UpdateRestrictions"">
  <Record>
    <PropertyValue Property=""Updatable"" Bool=""{updatable}"" />
  </Record>
</Annotation>";
        var target = @"""NS.Default/Customers/AlternativeAddresses""";
        var model = EntitySetPathItemHandlerTests.GetEdmModel(annotation, target);
        var context = new ODataContext(model, new OpenApiConvertSettings());

        var entitySet = model.EntityContainer.FindEntitySet("Customers");
        Assert.NotNull(entitySet); // guard

        var entityType = entitySet.EntityType;
        var property = entityType.FindProperty("AlternativeAddresses");
        Assert.NotNull(property); // guard

        var path = new ODataPath(
			new ODataNavigationSourceSegment(entitySet), 
			new ODataKeySegment(entityType), 
			new ODataComplexPropertySegment(property as IEdmStructuralProperty));
        Assert.Equal(ODataPathKind.ComplexProperty, path.Kind); // guard

		// Act
        var pathItem = _pathItemHandler.CreatePathItem(context, path);

		// Assert
        Assert.NotNull(pathItem);
        Assert.Equal(operationCount, pathItem.Operations.Count);
        if (operationCount == 1)
        {
            Assert.True(pathItem.Operations.ContainsKey(OperationType.Patch));
            Assert.False(pathItem.Operations.ContainsKey(OperationType.Post));
            Assert.False(pathItem.Operations.ContainsKey(OperationType.Get));
        }
        else if (operationCount == 3) 
        {
            Assert.True(pathItem.Operations.ContainsKey(OperationType.Patch));
            Assert.True(pathItem.Operations.ContainsKey(OperationType.Post));
            Assert.True(pathItem.Operations.ContainsKey(OperationType.Get));
        }
    }
}