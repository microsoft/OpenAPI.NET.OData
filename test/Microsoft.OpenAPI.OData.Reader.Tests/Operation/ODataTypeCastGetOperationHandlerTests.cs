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
    [InlineData(true, true, true)]
    [InlineData(true, false, true)]
    [InlineData(false, true, false)]
    [InlineData(false, false, false)]
    public void CreateODataTypeCastGetOperationReturnsCorrectOperationForCollectionNavigationProperty(
        bool enableOperationId, bool enablePagination, bool useHTTPStatusCodeClass2XX)
    {// ../People/{id}/Friends/Microsoft.OData.Service.Sample.TrippinInMemory.Models.Employee
        // Arrange
        IEdmModel model = EdmModelHelper.TripServiceModel;
        OpenApiConvertSettings settings = new()
        {
                EnableOperationId = enableOperationId,
                EnablePagination = enablePagination,
                UseSuccessStatusCodeRange = useHTTPStatusCodeClass2XX
        };
        ODataContext context = new(model, settings);
        IEdmEntitySet people = model.EntityContainer.FindEntitySet("People");
        Assert.NotNull(people);

        IEdmEntityType person = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Person");
        IEdmEntityType employee = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Employee");
        IEdmNavigationProperty navProperty = person.DeclaredNavigationProperties().First(c => c.Name == "Friends");
        ODataPath path = new(new ODataNavigationSourceSegment(people),
                                                                    new ODataKeySegment(people.EntityType),
                                                                    new ODataNavigationPropertySegment(navProperty),
                                                                    new ODataTypeCastSegment(employee, model));

        // Act
        var operation = _operationHandler.CreateOperation(context, path);

        // Assert
        Assert.NotNull(operation);
        Assert.Equal("Get the items of type Microsoft.OData.Service.Sample.TrippinInMemory.Models.Employee in the Microsoft.OData.Service.Sample.TrippinInMemory.Models.Person collection", operation.Summary);
        Assert.NotNull(operation.Tags);
        var tag = Assert.Single(operation.Tags);
        Assert.Equal("People.Person", tag.Name);
        Assert.Single(tag.Extensions);

        Assert.NotNull(operation.Parameters);
        Assert.Equal(10, operation.Parameters.Count);

        Assert.Null(operation.RequestBody);
        if(enablePagination)
            Assert.Equal(2, operation.Extensions.Count); //deprecated, pagination

        Assert.Equal(2, operation.Responses.Count);
        var statusCode = useHTTPStatusCodeClass2XX ? "2XX" : "200";
        Assert.Equal(new string[] { statusCode, "default" }, operation.Responses.Select(e => e.Key));

        if (enableOperationId)
        {
            Assert.Equal("People.ListFriends.AsEmployee", operation.OperationId);
        }
        else
        {
            Assert.Null(operation.OperationId);
        }
        Assert.True(operation.Responses.ContainsKey(statusCode));
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
                                                                    new ODataKeySegment(people.EntityType),
                                                                    new ODataNavigationPropertySegment(navProperty),
                                                                    new ODataKeySegment(people.EntityType),
                                                                    new ODataTypeCastSegment(employee,model));

        // Act
        var operation = _operationHandler.CreateOperation(context, path);

        // Assert
        Assert.NotNull(operation);
        Assert.Equal("Get the item of type Microsoft.OData.Service.Sample.TrippinInMemory.Models.Person as Microsoft.OData.Service.Sample.TrippinInMemory.Models.Employee", operation.Summary);
        Assert.NotNull(operation.Tags);
        var tag = Assert.Single(operation.Tags);
        Assert.Equal("People.Person", tag.Name);
        Assert.Empty(tag.Extensions);

        Assert.NotNull(operation.Parameters);
        Assert.Equal(5, operation.Parameters.Count); //select, expand, id, id, ConsistencyLevel

        Assert.Null(operation.RequestBody);
        if(enablePagination)
            Assert.Single(operation.Extensions); //deprecated

        Assert.Equal(2, operation.Responses.Count);
        Assert.Equal(new string[] { "200", "default" }, operation.Responses.Select(e => e.Key));

        if (enableOperationId)
        {
            Assert.Equal("People.GetFriends.AsEmployee", operation.OperationId);
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
                                                                    new ODataTypeCastSegment(employee,model));

        // Act
        var operation = _operationHandler.CreateOperation(context, path);

        // Assert
        Assert.NotNull(operation);
        Assert.Equal("Get the items of type Microsoft.OData.Service.Sample.TrippinInMemory.Models.Employee in the Microsoft.OData.Service.Sample.TrippinInMemory.Models.Person collection", operation.Summary);
        Assert.NotNull(operation.Tags);
        var tag = Assert.Single(operation.Tags);
        Assert.Equal("People.Person", tag.Name);
        Assert.Single(tag.Extensions);

        Assert.NotNull(operation.Parameters);
        Assert.Equal(9, operation.Parameters.Count);

        Assert.Null(operation.RequestBody);
        if(enablePagination)
            Assert.Equal(2, operation.Extensions.Count);

        Assert.Equal(2, operation.Responses.Count);
        Assert.Equal(new string[] { "200", "default" }, operation.Responses.Select(e => e.Key));

        if (enableOperationId)
        {
            Assert.Equal("People.Person.ListPerson.AsEmployee", operation.OperationId);
        }
        else
        {
            Assert.Null(operation.OperationId);
        }
        Assert.True(operation.Responses.ContainsKey("200"));
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
                                                                    new ODataKeySegment(people.EntityType),
                                                                    new ODataTypeCastSegment(employee,model));

        // Act
        var operation = _operationHandler.CreateOperation(context, path);

        // Assert
        Assert.NotNull(operation);
        Assert.Equal("Get the item of type Microsoft.OData.Service.Sample.TrippinInMemory.Models.Person as Microsoft.OData.Service.Sample.TrippinInMemory.Models.Employee", operation.Summary);
        Assert.NotNull(operation.Tags);
        var tag = Assert.Single(operation.Tags);
        Assert.Equal("People.Person", tag.Name);
        Assert.Empty(tag.Extensions);

        Assert.NotNull(operation.Parameters);
        Assert.Equal(4, operation.Parameters.Count); //select, expand, id

        Assert.Null(operation.RequestBody);
        if(enablePagination)
            Assert.Single(operation.Extensions); // deprecated

        Assert.Equal(2, operation.Responses.Count);
        Assert.Equal(new string[] { "200", "default" }, operation.Responses.Select(e => e.Key));

        if (enableOperationId)
        {
            Assert.Equal("People.Person.GetPerson.AsEmployee", operation.OperationId);
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
                                                                    new ODataKeySegment(people.EntityType),
                                                                    new ODataNavigationPropertySegment(navProperty),
                                                                    new ODataTypeCastSegment(employee, model));

        // Act
        var operation = _operationHandler.CreateOperation(context, path);

        // Assert
        Assert.NotNull(operation);
        Assert.Equal("Get the item of type Microsoft.OData.Service.Sample.TrippinInMemory.Models.Person as Microsoft.OData.Service.Sample.TrippinInMemory.Models.Employee", operation.Summary);
        Assert.NotNull(operation.Tags);
        var tag = Assert.Single(operation.Tags);
        Assert.Equal("People.Person", tag.Name);
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
            Assert.Equal("People.GetBestFriend.AsEmployee", operation.OperationId);
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
                                                                    new ODataTypeCastSegment(employee, model));

        // Act
        var operation = _operationHandler.CreateOperation(context, path);

        // Assert
        Assert.NotNull(operation);
        Assert.Equal("Get the item of type Microsoft.OData.Service.Sample.TrippinInMemory.Models.Person as Microsoft.OData.Service.Sample.TrippinInMemory.Models.Employee", operation.Summary);
        Assert.NotNull(operation.Tags);
        var tag = Assert.Single(operation.Tags);
        Assert.Equal("Me.Person", tag.Name);
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
            Assert.Equal("Me.Person.GetPerson.AsEmployee", operation.OperationId);
        }
        else
        {
            Assert.Null(operation.OperationId);
        }
        Assert.False(operation.Responses["200"].Content["application/json"].Schema.Properties.ContainsKey("value"));
    }
    [Fact]
    public void CreateODataTypeCastGetOperationReturnsCorrectOperationForSingleNavigationPropertyWithTargetPathAnnotations()
    {// .../People/{id}/BestFriend/Microsoft.OData.Service.Sample.TrippinInMemory.Models.Manager
        // Arrange
        IEdmModel model = EdmModelHelper.TripServiceModel;
        ODataContext context = new(model, new OpenApiConvertSettings());
        IEdmEntitySet people = model.EntityContainer.FindEntitySet("People");
        Assert.NotNull(people);

        IEdmEntityType person = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Person");
        IEdmEntityType manager = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Manager");
        IEdmNavigationProperty navProperty = person.DeclaredNavigationProperties().First(c => c.Name == "BestFriend");
        ODataPath path = new(new ODataNavigationSourceSegment(people),
                                                                    new ODataKeySegment(people.EntityType),
                                                                    new ODataNavigationPropertySegment(navProperty),
                                                                    new ODataTypeCastSegment(manager, model));

        // Act
        var operation = _operationHandler.CreateOperation(context, path);

        // Assert
        Assert.NotNull(operation);
        Assert.Equal("Get best friend", operation.Summary);
        Assert.Equal("Get the item of type Person cast as Manager", operation.Description);

        Assert.NotNull(operation.ExternalDocs);
        Assert.Equal("Find more info here", operation.ExternalDocs.Description);
        Assert.Equal("https://learn.microsoft.com/graph/api/person-get-friend-manager?view=graph-rest-1.0", operation.ExternalDocs.Url.ToString());

    }
}
