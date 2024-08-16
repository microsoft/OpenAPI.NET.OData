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
    public class MediaEntityGetOperationHandlerTests
    {
        private readonly MediaEntityGetOperationHandler _operationalHandler = new MediaEntityGetOperationHandler();

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(false, false)]
        public void CreateMediaEntityGetOperationReturnsCorrectOperation(bool enableOperationId, bool useHTTPStatusCodeClass2XX)
        {
            // Arrange
            string annotation = $@"
            <Annotation Term=""Org.OData.Core.V1.AcceptableMediaTypes"" >
              <Collection>
                <String>image/png</String>
                <String>image/jpeg</String>
              </Collection>
            </Annotation>
            <Annotation Term=""Org.OData.Core.V1.Description"" String=""The logo image."" />
            <Annotation Term=""Org.OData.Capabilities.V1.ReadRestrictions"">
              <Record>
                <PropertyValue Property=""CustomQueryOptions"">
                  <Collection>
                    <Record>
                      <PropertyValue Property=""Name"" String=""format"" />
                      <PropertyValue Property=""Description"" String=""Specify the format the item's content should be downloaded as."" />
                      <PropertyValue Property=""Required"" Bool=""false"" />
                    </Record>                
                  </Collection>
                </PropertyValue>
              </Record>
            </Annotation>";

            // Assert
            VerifyMediaEntityGetOperation("", enableOperationId, useHTTPStatusCodeClass2XX);
            VerifyMediaEntityGetOperation(annotation, enableOperationId, useHTTPStatusCodeClass2XX);
        }

        private void VerifyMediaEntityGetOperation(string annotation, bool enableOperationId, bool useHTTPStatusCodeClass2XX)
        {
            // Arrange
            IEdmModel model = GetEdmModel(annotation);
            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableOperationId = enableOperationId,
                UseSuccessStatusCodeRange = useHTTPStatusCodeClass2XX
            };

            ODataContext context = new ODataContext(model, settings);
            IEdmEntitySet todos = model.EntityContainer.FindEntitySet("Todos");
            IEdmSingleton me = model.EntityContainer.FindSingleton("me");
            Assert.NotNull(todos);
            Assert.NotNull(me);

            IEdmEntityType todo = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Todo");
            IEdmStructuralProperty sp = todo.StructuralProperties().First(c => c.Name == "Logo");
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(todos),
                new ODataKeySegment(todos.EntityType),
                new ODataStreamPropertySegment(sp.Name));

            IEdmEntityType user = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "user");
            IEdmNavigationProperty navProperty = user.NavigationProperties().First(c => c.Name == "photo");
            ODataPath path2 = new ODataPath(new ODataNavigationSourceSegment(me),
                new ODataNavigationPropertySegment(navProperty),
                new ODataStreamContentSegment());

            // Act
            var getOperation = _operationalHandler.CreateOperation(context, path);
            var getOperation2 = _operationalHandler.CreateOperation(context, path2);

            // Assert
            Assert.NotNull(getOperation);
            Assert.NotNull(getOperation2);
            Assert.Equal("Get Logo for Todo from Todos", getOperation.Summary);
            Assert.Equal("Get media content for the navigation property photo from me", getOperation2.Summary);
            Assert.NotNull(getOperation.Tags);
            Assert.NotNull(getOperation2.Tags);

            var tag = Assert.Single(getOperation.Tags);
            var tag2 = Assert.Single(getOperation2.Tags);
            Assert.Equal("Todos.Todo", tag.Name);
            Assert.Equal("me.profilePhoto", tag2.Name);

            Assert.NotNull(getOperation.Responses);
            Assert.NotNull(getOperation2.Responses);
            Assert.Equal(2, getOperation.Responses.Count);
            Assert.Equal(2, getOperation2.Responses.Count);
            var statusCode = useHTTPStatusCodeClass2XX ? Constants.StatusCodeClass2XX : Constants.StatusCode200;
            Assert.Equal(new[] { statusCode, "default" }, getOperation.Responses.Select(r => r.Key));
            Assert.Equal(new[] { statusCode, "default" }, getOperation2.Responses.Select(r => r.Key));
            Assert.NotNull(getOperation.Responses[statusCode].Content.Values?.Select(x => x.Schema));

            foreach (var item in getOperation.Responses[statusCode].Content)
            {
                Assert.Equal("binary", item.Value.Schema.Format);
                Assert.Equal("string", item.Value.Schema.Type);
            }

            if (!string.IsNullOrEmpty(annotation))
            {
                Assert.Equal(2, getOperation.Responses[statusCode].Content.Keys.Count);
                Assert.True(getOperation.Responses[statusCode].Content.ContainsKey("image/png"));
                Assert.True(getOperation.Responses[statusCode].Content.ContainsKey("image/jpeg"));
                Assert.Equal("The logo image.", getOperation.Description);
                Assert.Equal(2, getOperation.Parameters.Count);
                Assert.NotNull(getOperation.Parameters.FirstOrDefault(x => x.Name.Equals("format")));

                Assert.Single(getOperation2.Responses[statusCode].Content.Keys);
                Assert.True(getOperation2.Responses[statusCode].Content.ContainsKey(Constants.ApplicationOctetStreamMediaType));
            }
            else
            {
                Assert.Single(getOperation.Parameters);
                Assert.Single(getOperation.Responses[statusCode].Content.Keys);
                Assert.Single(getOperation2.Responses[statusCode].Content.Keys);
                Assert.True(getOperation.Responses[statusCode].Content.ContainsKey(Constants.ApplicationOctetStreamMediaType));
                Assert.True(getOperation2.Responses[statusCode].Content.ContainsKey(Constants.ApplicationOctetStreamMediaType));
            }

            if (enableOperationId)
            {
                Assert.Equal("Todos.Todo.GetLogo", getOperation.OperationId);
                Assert.Equal("me.GetPhotoContent", getOperation2.OperationId);
            }
            else
            {
                Assert.Null(getOperation.OperationId);
                Assert.Null(getOperation2.OperationId);
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
            <Annotation Term=""Org.OData.Core.V1.Description"" String=""The user's profile photo."" />
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
      <Annotations Target=""microsoft.graph.Todo/Logo"">
        {0}
      </Annotations>
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>";
            string modelText = string.Format(template, annotation);

            bool result = CsdlReader.TryParse(XElement.Parse(modelText).CreateReader(), out IEdmModel model, out _);
            Assert.True(result);
            return model;
        }

        [Fact]
        public void CreateMediaEntityPropertyGetOperationWithTargetPathAnnotationsReturnsCorrectOperation()
        {
            // Arrange
            IEdmModel model = OData.Tests.EdmModelHelper.TripServiceModel;
            ODataContext context = new(model, new OpenApiConvertSettings());
            IEdmEntitySet people = model.EntityContainer.FindEntitySet("People");
            Assert.NotNull(people);

            IEdmEntityType person = people.EntityType;
            IEdmStructuralProperty property = person.StructuralProperties().First(c => c.Name == "Photo");
            ODataPath path = new (new ODataNavigationSourceSegment(people),
                new ODataKeySegment(person),
                new ODataStreamPropertySegment(property.Name));

            // Act
            var operation = _operationalHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(operation);
            Assert.Equal("Get photo", operation.Summary);
            Assert.Equal("Get photo of a specific user", operation.Description);
            Assert.Single(operation.Parameters);

            Assert.NotNull(operation.ExternalDocs);
            Assert.Equal("Find more info here", operation.ExternalDocs.Description);
            Assert.Equal("https://learn.microsoft.com/graph/api/person-get-photo?view=graph-rest-1.0", operation.ExternalDocs.Url.ToString());
        }
    }
}
