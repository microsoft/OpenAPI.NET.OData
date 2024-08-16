// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace Microsoft.OpenApi.OData.Operation.Tests
{
    public class MediaEntityDeleteOperationHandlerTests
    {
        private readonly MediaEntityDeleteOperationHandler _operationalHandler = new MediaEntityDeleteOperationHandler();

        [Fact]
        public void CreateMediaEntityPropertyDeleteOperationWithTargetPathAnnotationsReturnsCorrectOperation()
        {
            // Arrange
            IEdmModel model = OData.Tests.EdmModelHelper.TripServiceModel;
            ODataContext context = new(model, new OpenApiConvertSettings());
            IEdmEntitySet people = model.EntityContainer.FindEntitySet("People");
            Assert.NotNull(people);

            IEdmEntityType person = people.EntityType;
            IEdmStructuralProperty property = person.StructuralProperties().First(c => c.Name == "Photo");
            ODataPath path = new(new ODataNavigationSourceSegment(people),
                new ODataKeySegment(person),
                new ODataStreamPropertySegment(property.Name));

            // Act
            var operation = _operationalHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(operation);
            Assert.Equal("Delete photo", operation.Summary);
            Assert.Equal("Delete photo of a specific user", operation.Description);

            Assert.NotNull(operation.ExternalDocs);
            Assert.Equal("Find more info here", operation.ExternalDocs.Description);
            Assert.Equal("https://learn.microsoft.com/graph/api/person-delete-photo?view=graph-rest-1.0", operation.ExternalDocs.Url.ToString());

            Assert.NotNull(operation.Tags);
            var tag = Assert.Single(operation.Tags);
            Assert.Equal("People.Person", tag.Name);

            Assert.NotNull(operation.Parameters);
            Assert.NotEmpty(operation.Parameters);

            Assert.Null(operation.RequestBody);

            Assert.Equal(2, operation.Responses.Count);
            Assert.Equal(new string[] { "204", "default" }, operation.Responses.Select(e => e.Key));

            Assert.Equal("People.Person.DeletePhoto", operation.OperationId);
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(true, true)]
        [InlineData(false, false)]
        [InlineData(false, true)]
        public void CreateMediaEntityDeleteOperationReturnsCorrectOperation(bool enableOperationId, bool useSuccessStatusCodeRange)
        {
            // Arrange
            string annotation = $@"
            <Annotation Term=""Org.OData.Capabilities.V1.NavigationRestrictions"">
              <Record>
                <PropertyValue Property=""RestrictedProperties"">
                  <Collection>
                    <Record>
                        <PropertyValue Property=""DeleteRestrictions"">
                            <Record>
                                <PropertyValue Property=""Description"" String=""Delete photo"" />
						        <PropertyValue Property=""LongDescription"" String=""Delete photo in Todo"" />
                            </Record>
                        </PropertyValue>
                    </Record>
                  </Collection>
                </PropertyValue>
              </Record>  
            </Annotation>";

            // Assert
            VerifyMediaEntityDeleteOperation("", enableOperationId, useSuccessStatusCodeRange);
            VerifyMediaEntityDeleteOperation(annotation, enableOperationId, useSuccessStatusCodeRange);
        }

        private void VerifyMediaEntityDeleteOperation(string annotation, bool enableOperationId, bool useSuccessStatusCodeRange)
        {
            // Arrange
            IEdmModel model = GetEdmModel(annotation);
            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableOperationId = enableOperationId,
                UseSuccessStatusCodeRange = useSuccessStatusCodeRange
            };

            ODataContext context = new(model, settings);
            IEdmEntitySet todos = model.EntityContainer.FindEntitySet("Todos");
            IEdmSingleton me = model.EntityContainer.FindSingleton("me");
            Assert.NotNull(todos);

            IEdmEntityType todo = todos.EntityType;
            IEdmStructuralProperty structuralProperty = todo.StructuralProperties().First(c => c.Name == "Logo");
            ODataPath path = new(new ODataNavigationSourceSegment(todos),
                new ODataKeySegment(todo),
                new ODataStreamPropertySegment(structuralProperty.Name));

            IEdmEntityType user = me.EntityType;
            IEdmNavigationProperty navProperty = user.NavigationProperties().First(c => c.Name == "photo");
            ODataPath path2 = new(new ODataNavigationSourceSegment(me),
                new ODataNavigationPropertySegment(navProperty),
                new ODataStreamContentSegment());

            IEdmStructuralProperty structuralProperty2 = todo.StructuralProperties().First(c => c.Name == "Content");
            ODataPath path3 = new(new ODataNavigationSourceSegment(todos),
                new ODataKeySegment(todo),
                new ODataStreamPropertySegment(structuralProperty2.Name));

            // Act
            var deleteOperation = _operationalHandler.CreateOperation(context, path);
            var deleteOperation2 = _operationalHandler.CreateOperation(context, path2);
            var deleteOperation3 = _operationalHandler.CreateOperation(context, path3);

            // Assert
            Assert.NotNull(deleteOperation);
            Assert.NotNull(deleteOperation2);
            Assert.NotNull(deleteOperation3);
            Assert.Equal("Delete Logo for Todo in Todos", deleteOperation.Summary);
            if (!string.IsNullOrEmpty(annotation))
            {
                Assert.Equal("Delete photo", deleteOperation2.Summary);
                Assert.Equal("Delete photo in Todo", deleteOperation2.Description);
            }
            else
            {
                Assert.Equal("Delete media content for the navigation property photo in me", deleteOperation2.Summary);
                Assert.Null(deleteOperation2.Description);
            }
            Assert.NotNull(deleteOperation.Tags);
            Assert.NotNull(deleteOperation2.Tags);

            var tag = Assert.Single(deleteOperation.Tags);
            var tag2 = Assert.Single(deleteOperation2.Tags);
            var tag3 = Assert.Single(deleteOperation3.Tags);
            Assert.Equal("Todos.Todo", tag.Name);
            Assert.Equal("me.profilePhoto", tag2.Name);
            Assert.Equal("Todos.Todo", tag3.Name);

            Assert.Null(deleteOperation.RequestBody);
            Assert.Null(deleteOperation2.RequestBody);
            Assert.Null(deleteOperation3.RequestBody);

            Assert.NotNull(deleteOperation.Responses);
            Assert.NotNull(deleteOperation2.Responses);
            Assert.NotNull(deleteOperation3.Responses);

            Assert.Equal(2, deleteOperation.Responses.Count);
            Assert.Equal(2, deleteOperation2.Responses.Count);
            Assert.Equal(2, deleteOperation3.Responses.Count);

            Assert.Equal(new[] { Constants.StatusCode204, "default" }, deleteOperation.Responses.Select(r => r.Key));
            Assert.Equal(new[] { Constants.StatusCode204, "default" }, deleteOperation2.Responses.Select(r => r.Key));
            Assert.Equal(new[] { Constants.StatusCode204, "default" }, deleteOperation3.Responses.Select(r => r.Key));

            if (enableOperationId)
            {
                Assert.Equal("Todos.Todo.DeleteLogo", deleteOperation.OperationId);
                Assert.Equal("me.DeletePhotoContent", deleteOperation2.OperationId);
            }
            else
            {
                Assert.Null(deleteOperation.OperationId);
                Assert.Null(deleteOperation2.OperationId);
            }
        }

        public static IEdmModel GetEdmModel(string annotation)
        {
            const string template = @"<edmx:Edmx Version=""4.0"" xmlns:edmx=""http://docs.oasis-open.org/odata/ns/edmx"">
  <edmx:DataServices>
    <Schema Namespace=""microsoft.graph"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
      <EntityType Name=""Todo"" HasStream=""true"">
        <Key>
          <PropertyRef Name=""Id"" />
        </Key>
        <Property Name=""Id"" Type=""Edm.Int32"" Nullable=""false"" />
        <Property Name=""Logo"" Type=""Edm.Stream""/>
        <Property Name=""Content"" Type=""Edm.Stream""/>
        <Property Name = ""Description"" Type = ""Edm.String"" />
      </EntityType>
      <EntityType Name=""user"" OpenType=""true"">
        <NavigationProperty Name = ""photo"" Type = ""microsoft.graph.profilePhoto"" >
            {0}
        </NavigationProperty>   
      </EntityType>
      <EntityType Name=""profilePhoto"" HasStream=""true"">
        <Property Name = ""height"" Type = ""Edm.Int32"" />
        <Property Name = ""width"" Type = ""Edm.Int32"" />
      </EntityType >
      <EntityContainer Name =""GraphService"">
        <EntitySet Name=""Todos"" EntityType=""microsoft.graph.Todo"" />
        <Singleton Name=""me"" Type=""microsoft.graph.user"" />
      </EntityContainer>
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>";
            string modelText = string.Format(template, annotation);

            bool result = CsdlReader.TryParse(XElement.Parse(modelText).CreateReader(), out IEdmModel model, out _);
            Assert.True(result);
            return model;
        }
    }
}
