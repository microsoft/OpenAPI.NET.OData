// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.PathItem.Tests;
using Microsoft.OpenApi.OData.Tests;
using Xunit;

namespace Microsoft.OpenApi.OData.Operation.Tests;
public class ODataTypeCastGetOperationHandlerTests
{
  private readonly ODataTypeCastGetOperationHandler _operationHandler = new ();

  [Theory]
  [InlineData(true)]
  [InlineData(false)]
  public void CreateODataTypeCastGetOperationReturnsCorrectOperation(bool enableOperationId)
  {
    // Arrange
    IEdmModel model = EdmModelHelper.TripServiceModel;
    OpenApiConvertSettings settings = new()
    {
        EnableOperationId = enableOperationId
    };
    ODataContext context = new(model, settings);
    IEdmEntitySet people = model.EntityContainer.FindEntitySet("People");
    Assert.NotNull(people);

    IEdmEntityType person = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Person");
    IEdmEntityType employee = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Employee");
    IEdmNavigationProperty navProperty = person.DeclaredNavigationProperties().First(c => c.Name == "Friends");
    ODataPath path = new(new ODataNavigationSourceSegment(people),
                                  new ODataKeySegment(people.EntityType()),
                                  new ODataNavigationPropertySegment(navProperty),
                                  new ODataTypeCastSegment(employee));

    // Act
    var operation = _operationHandler.CreateOperation(context, path);

    // Assert
    Assert.NotNull(operation);
    Assert.Equal("Get the items of type Microsoft.OData.Service.Sample.TrippinInMemory.Models.Employee in the Microsoft.OData.Service.Sample.TrippinInMemory.Models.Person collection", operation.Summary);
    Assert.NotNull(operation.Tags);
    var tag = Assert.Single(operation.Tags);
    Assert.Equal("Person.Employee", tag.Name);

    Assert.NotNull(operation.Parameters);
    Assert.Equal(9, operation.Parameters.Count);

    Assert.Null(operation.RequestBody);

    Assert.Equal(2, operation.Responses.Count);
    Assert.Equal(new string[] { "200", "default" }, operation.Responses.Select(e => e.Key));

    if (enableOperationId)
    {
      Assert.Equal("Get.Microsoft.OData.Service.Sample.TrippinInMemory.Models.Person.As.Microsoft.OData.Service.Sample.TrippinInMemory.Models.Employee", operation.OperationId);
    }
    else
    {
      Assert.Null(operation.OperationId);
    }
  }
  //TODO test on entity set
  //TODO test on cast cast key
  //TODO test on cast on single nav property
}
