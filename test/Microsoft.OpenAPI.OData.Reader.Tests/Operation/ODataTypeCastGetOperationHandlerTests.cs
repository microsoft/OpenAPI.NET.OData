// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Tests;
using Xunit;

namespace Microsoft.OpenApi.OData.Operation.Tests;
public class ODataTypeCastGetOperationHandlerTests
{
    private readonly ODataTypeCastGetOperationHandler _operationHandler = new ();

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void CreateODataTypeCastGetOperationReturnsCorrectOperationForCollectionNavigationProperty(bool enableOperationId, bool enablePagination)
    {// ../People/{id}/Friends/Microsoft.OData.Service.Sample.TrippinInMemory.Models.Employee
        // Arrange
        IEdmModel model = EdmModelHelper.TripServiceModel;
        OpenApiConvertSettings settings = new()
        {
                EnableOperationId = enableOperationId,
                EnablePagination = enablePagination,
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
        Assert.Single(tag.Extensions);

        Assert.NotNull(operation.Parameters);
        Assert.Equal(9, operation.Parameters.Count);

        Assert.Null(operation.RequestBody);
        if(enablePagination)
            Assert.Equal(2, operation.Extensions.Count); //deprecated, pagination

        Assert.Equal(2, operation.Responses.Count);
        Assert.Equal(new string[] { "200", "default" }, operation.Responses.Select(e => e.Key));

        if (enableOperationId)
        {
            Assert.Equal("Get.Microsoft.OData.Service.Sample.TrippinInMemory.Models.Person.Items.As.Microsoft.OData.Service.Sample.TrippinInMemory.Models.Employee-11bf", operation.OperationId);
        }
        else
        {
            Assert.Null(operation.OperationId);
        }
        Assert.True(operation.Responses["200"].Content["application/json"].Schema.Properties.ContainsKey("value"));
    }
    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void CreateODataTypeCastGetOperationReturnsCorrectOperationForCollectionNavigationPropertyId(bool enableOperationId, bool enablePagination)
    {// ../People/{id}/Friends/{id}/Microsoft.OData.Service.Sample.TrippinInMemory.Models.Employee/
        // Arrange
        IEdmModel model = EdmModelHelper.TripServiceModel;
        OpenApiConvertSettings settings = new()
        {
                EnableOperationId = enableOperationId,
                EnablePagination = enablePagination,
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
                                                                    new ODataKeySegment(people.EntityType()),
                                                                    new ODataTypeCastSegment(employee));

        // Act
        var operation = _operationHandler.CreateOperation(context, path);

        // Assert
        Assert.NotNull(operation);
        Assert.Equal("Get the item of type Microsoft.OData.Service.Sample.TrippinInMemory.Models.Person as Microsoft.OData.Service.Sample.TrippinInMemory.Models.Employee", operation.Summary);
        Assert.NotNull(operation.Tags);
        var tag = Assert.Single(operation.Tags);
        Assert.Equal("Person.Employee", tag.Name);
        Assert.Empty(tag.Extensions);

        Assert.NotNull(operation.Parameters);
        Assert.Equal(4, operation.Parameters.Count); //select, expand, id, id

        Assert.Null(operation.RequestBody);
        if(enablePagination)
            Assert.Single(operation.Extensions); //deprecated

        Assert.Equal(2, operation.Responses.Count);
        Assert.Equal(new string[] { "200", "default" }, operation.Responses.Select(e => e.Key));

        if (enableOperationId)
        {
            Assert.Equal("Get.Microsoft.OData.Service.Sample.TrippinInMemory.Models.Person.Item.As.Microsoft.OData.Service.Sample.TrippinInMemory.Models.Employee-11bf", operation.OperationId);
        }
        else
        {
            Assert.Null(operation.OperationId);
        }
        Assert.False(operation.Responses["200"].Content["application/json"].Schema.Properties.ContainsKey("value"));
    }
    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void CreateODataTypeCastGetOperationReturnsCorrectOperationForEntitySet(bool enableOperationId, bool enablePagination)
    {// .../People/Microsoft.OData.Service.Sample.TrippinInMemory.Models.Employee
        // Arrange
        IEdmModel model = EdmModelHelper.TripServiceModel;
        OpenApiConvertSettings settings = new()
        {
                EnableOperationId = enableOperationId,
                EnablePagination = enablePagination,
        };
        ODataContext context = new(model, settings);
        IEdmEntitySet people = model.EntityContainer.FindEntitySet("People");
        Assert.NotNull(people);

        IEdmEntityType employee = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Employee");
        ODataPath path = new(new ODataNavigationSourceSegment(people),
                                                                    new ODataTypeCastSegment(employee));

        // Act
        var operation = _operationHandler.CreateOperation(context, path);

        // Assert
        Assert.NotNull(operation);
        Assert.Equal("Get the items of type Microsoft.OData.Service.Sample.TrippinInMemory.Models.Employee in the Microsoft.OData.Service.Sample.TrippinInMemory.Models.Person collection", operation.Summary);
        Assert.NotNull(operation.Tags);
        var tag = Assert.Single(operation.Tags);
        Assert.Equal("Person.Employee", tag.Name);
        Assert.Single(tag.Extensions);

        Assert.NotNull(operation.Parameters);
        Assert.Equal(8, operation.Parameters.Count);

        Assert.Null(operation.RequestBody);
        if(enablePagination)
            Assert.Equal(2, operation.Extensions.Count);

        Assert.Equal(2, operation.Responses.Count);
        Assert.Equal(new string[] { "200", "default" }, operation.Responses.Select(e => e.Key));

        if (enableOperationId)
        {
            Assert.Equal("Get.Microsoft.OData.Service.Sample.TrippinInMemory.Models.Person.Items.As.Microsoft.OData.Service.Sample.TrippinInMemory.Models.Employee-013a", operation.OperationId);
        }
        else
        {
            Assert.Null(operation.OperationId);
        }
        Assert.True(operation.Responses["200"].Content["application/json"].Schema.Properties.ContainsKey("value"));
    }
    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void CreateODataTypeCastGetOperationReturnsCorrectOperationForEntitySetId(bool enableOperationId, bool enablePagination)
    {// .../People/{id}/Microsoft.OData.Service.Sample.TrippinInMemory.Models.Employee
        // Arrange
        IEdmModel model = EdmModelHelper.TripServiceModel;
        OpenApiConvertSettings settings = new()
        {
                EnableOperationId = enableOperationId,
                EnablePagination = enablePagination,
        };
        ODataContext context = new(model, settings);
        IEdmEntitySet people = model.EntityContainer.FindEntitySet("People");
        Assert.NotNull(people);

        IEdmEntityType person = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Person");
        IEdmEntityType employee = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Employee");
        ODataPath path = new(new ODataNavigationSourceSegment(people),
                                                                    new ODataKeySegment(people.EntityType()),
                                                                    new ODataTypeCastSegment(employee));

        // Act
        var operation = _operationHandler.CreateOperation(context, path);

        // Assert
        Assert.NotNull(operation);
        Assert.Equal("Get the item of type Microsoft.OData.Service.Sample.TrippinInMemory.Models.Person as Microsoft.OData.Service.Sample.TrippinInMemory.Models.Employee", operation.Summary);
        Assert.NotNull(operation.Tags);
        var tag = Assert.Single(operation.Tags);
        Assert.Equal("Person.Employee", tag.Name);
        Assert.Empty(tag.Extensions);

        Assert.NotNull(operation.Parameters);
        Assert.Equal(3, operation.Parameters.Count); //select, expand, id

        Assert.Null(operation.RequestBody);
        if(enablePagination)
            Assert.Single(operation.Extensions); // deprecated

        Assert.Equal(2, operation.Responses.Count);
        Assert.Equal(new string[] { "200", "default" }, operation.Responses.Select(e => e.Key));

        if (enableOperationId)
        {
            Assert.Equal("Get.Microsoft.OData.Service.Sample.TrippinInMemory.Models.Person.Item.As.Microsoft.OData.Service.Sample.TrippinInMemory.Models.Employee-317b", operation.OperationId);
        }
        else
        {
            Assert.Null(operation.OperationId);
        }
        Assert.False(operation.Responses["200"].Content["application/json"].Schema.Properties.ContainsKey("value"));
    }
    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void CreateODataTypeCastGetOperationReturnsCorrectOperationForSingleNavigationproperty(bool enableOperationId, bool enablePagination)
    {// .../People/{id}/BestFriend/Microsoft.OData.Service.Sample.TrippinInMemory.Models.Employee
        // Arrange
        IEdmModel model = EdmModelHelper.TripServiceModel;
        OpenApiConvertSettings settings = new()
        {
                EnableOperationId = enableOperationId,
                EnablePagination = enablePagination,
        };
        ODataContext context = new(model, settings);
        IEdmEntitySet people = model.EntityContainer.FindEntitySet("People");
        Assert.NotNull(people);

        IEdmEntityType person = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Person");
        IEdmEntityType employee = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Employee");
        IEdmNavigationProperty navProperty = person.DeclaredNavigationProperties().First(c => c.Name == "BestFriend");
        ODataPath path = new(new ODataNavigationSourceSegment(people),
                                                                    new ODataKeySegment(people.EntityType()),
                                                                    new ODataNavigationPropertySegment(navProperty),
                                                                    new ODataTypeCastSegment(employee));

        // Act
        var operation = _operationHandler.CreateOperation(context, path);

        // Assert
        Assert.NotNull(operation);
        Assert.Equal("Get the item of type Microsoft.OData.Service.Sample.TrippinInMemory.Models.Person as Microsoft.OData.Service.Sample.TrippinInMemory.Models.Employee", operation.Summary);
        Assert.NotNull(operation.Tags);
        var tag = Assert.Single(operation.Tags);
        Assert.Equal("Person.Employee", tag.Name);
        Assert.Empty(tag.Extensions);

        Assert.NotNull(operation.Parameters);
        Assert.Equal(3, operation.Parameters.Count); //select, expand, id

        Assert.Null(operation.RequestBody);
        if(enablePagination)
            Assert.Single(operation.Extensions); //deprecated

        Assert.Equal(2, operation.Responses.Count);
        Assert.Equal(new string[] { "200", "default" }, operation.Responses.Select(e => e.Key));

        if (enableOperationId)
        {
            Assert.Equal("Get.Microsoft.OData.Service.Sample.TrippinInMemory.Models.Person.Item.As.Microsoft.OData.Service.Sample.TrippinInMemory.Models.Employee-7188", operation.OperationId);
        }
        else
        {
            Assert.Null(operation.OperationId);
        }
        Assert.False(operation.Responses["200"].Content["application/json"].Schema.Properties.ContainsKey("value"));
    }
    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void CreateODataTypeCastGetOperationReturnsCorrectOperationForSingleton(bool enableOperationId, bool enablePagination)
    {// .../Me/Microsoft.OData.Service.Sample.TrippinInMemory.Models.Employee
        // Arrange
        IEdmModel model = EdmModelHelper.TripServiceModel;
        OpenApiConvertSettings settings = new()
        {
                EnableOperationId = enableOperationId,
                EnablePagination = enablePagination,
        };
        ODataContext context = new(model, settings);
        IEdmSingleton me = model.EntityContainer.FindSingleton("Me");
        Assert.NotNull(me);

        IEdmEntityType employee = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Employee");
        ODataPath path = new(new ODataNavigationSourceSegment(me),
                                                                    new ODataTypeCastSegment(employee));

        // Act
        var operation = _operationHandler.CreateOperation(context, path);

        // Assert
        Assert.NotNull(operation);
        Assert.Equal("Get the item of type Microsoft.OData.Service.Sample.TrippinInMemory.Models.Person as Microsoft.OData.Service.Sample.TrippinInMemory.Models.Employee", operation.Summary);
        Assert.NotNull(operation.Tags);
        var tag = Assert.Single(operation.Tags);
        Assert.Equal("Person.Employee", tag.Name);
        Assert.Empty(tag.Extensions);

        Assert.NotNull(operation.Parameters);
        Assert.Equal(2, operation.Parameters.Count); //select, expand

        Assert.Null(operation.RequestBody);
        if(enablePagination)
            Assert.Single(operation.Extensions); //deprecated

        Assert.Equal(2, operation.Responses.Count);
        Assert.Equal(new string[] { "200", "default" }, operation.Responses.Select(e => e.Key));

        if (enableOperationId)
        {
            Assert.Equal("Get.Microsoft.OData.Service.Sample.TrippinInMemory.Models.Person.Item.As.Microsoft.OData.Service.Sample.TrippinInMemory.Models.Employee-bd18", operation.OperationId);
        }
        else
        {
            Assert.Null(operation.OperationId);
        }
        Assert.False(operation.Responses["200"].Content["application/json"].Schema.Properties.ContainsKey("value"));
    }
}
