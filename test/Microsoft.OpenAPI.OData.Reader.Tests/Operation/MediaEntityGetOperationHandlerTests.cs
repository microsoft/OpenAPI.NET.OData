// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace Microsoft.OpenApi.OData.Operation.Tests
{
    public class MediaEntityGetOperationHandlerTests
    {
        private readonly MediaEntityGetOperationHandler _operationalHandler = new MediaEntityGetOperationHandler();

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateMediaEntityGetOperationReturnsCorrectOperation(bool enableOperationId)
        {
            // Arrange
            string qualifiedName = CapabilitiesConstants.AcceptableMediaTypes;
            string annotation = $@"
            <Annotation Term=""{qualifiedName}"" >
              <Collection>
                <String>image/png</String>
                <String>image/jpeg</String>
              </Collection>
            </Annotation>";

            // Assert
            VerifyMediaEntityGetOperation("", enableOperationId);
            VerifyMediaEntityGetOperation(annotation, enableOperationId);
        }

        private void VerifyMediaEntityGetOperation(string annotation, bool enableOperationId)
        {
            // Arrange
            IEdmModel model = GetEdmModel(annotation);
            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableOperationId = enableOperationId
            };

            ODataContext context = new ODataContext(model, settings);
            IEdmEntitySet todos = model.EntityContainer.FindEntitySet("Todos");
            IEdmSingleton me = model.EntityContainer.FindSingleton("me");
            Assert.NotNull(todos);
            Assert.NotNull(me);

            IEdmEntityType todo = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Todo");
            IEdmStructuralProperty sp = todo.DeclaredStructuralProperties().First(c => c.Name == "Logo");
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(todos),
                new ODataKeySegment(todos.EntityType()),
                new ODataStreamPropertySegment(sp.Name));

            IEdmEntityType user = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "user");
            IEdmNavigationProperty navProperty = user.DeclaredNavigationProperties().First(c => c.Name == "photo");
            ODataPath path2 = new ODataPath(new ODataNavigationSourceSegment(me),
                new ODataNavigationPropertySegment(navProperty),
                new ODataStreamContentSegment());

            // Act
            var getOperation = _operationalHandler.CreateOperation(context, path);
            var getOperation2 = _operationalHandler.CreateOperation(context, path2);

            // Assert
            Assert.NotNull(getOperation);
            Assert.NotNull(getOperation2);
            Assert.Equal("Get media content for Todo from Todos", getOperation.Summary);
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
            Assert.Equal(new[] { "200", "default" }, getOperation.Responses.Select(r => r.Key));
            Assert.Equal(new[] { "200", "default" }, getOperation2.Responses.Select(r => r.Key));

            if (!string.IsNullOrEmpty(annotation))
            {
                Assert.Equal(2, getOperation.Responses[Constants.StatusCode200].Content.Keys.Count);
                Assert.True(getOperation.Responses[Constants.StatusCode200].Content.ContainsKey("image/png"));
                Assert.True(getOperation.Responses[Constants.StatusCode200].Content.ContainsKey("image/jpeg"));

                Assert.Equal(1, getOperation2.Responses[Constants.StatusCode200].Content.Keys.Count);
                Assert.True(getOperation2.Responses[Constants.StatusCode200].Content.ContainsKey(Constants.ApplicationOctetStreamMediaType));
            }
            else
            {
                Assert.Equal(1, getOperation.Responses[Constants.StatusCode200].Content.Keys.Count);
                Assert.Equal(1, getOperation2.Responses[Constants.StatusCode200].Content.Keys.Count);
                Assert.True(getOperation.Responses[Constants.StatusCode200].Content.ContainsKey(Constants.ApplicationOctetStreamMediaType));
                Assert.True(getOperation2.Responses[Constants.StatusCode200].Content.ContainsKey(Constants.ApplicationOctetStreamMediaType));
            }

            if (enableOperationId)
            {
                Assert.Equal("Todos.Todo.GetLogo", getOperation.OperationId);
                Assert.Equal("me.photo.GetContent", getOperation2.OperationId);
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
        <Property Name = ""Description"" Type = ""Edm.String"" />
      </EntityType>
      <EntityType Name=""user"" OpenType=""true"">
        <NavigationProperty Name = ""photo"" Type = ""microsoft.graph.profilePhoto"" ContainsTarget = ""true"" />
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
    }
}
