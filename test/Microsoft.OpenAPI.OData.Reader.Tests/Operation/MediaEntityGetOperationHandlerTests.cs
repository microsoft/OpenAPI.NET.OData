// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
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
        [InlineData(true)]
        [InlineData(false)]
        public void CreateMediaEntityGetOperationReturnsCorrectOperation(bool enableOperationId)
        {
            // Arrange
            IEdmModel model = GetEdmModel();
            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableOperationId = enableOperationId
            };

            ODataContext context = new ODataContext(model, settings);
            IEdmEntitySet todos = model.EntityContainer.FindEntitySet("Todos");
            Assert.NotNull(todos);

            IEdmEntityType todo = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Todo");
            IEdmStructuralProperty sp = todo.DeclaredStructuralProperties().First(c => c.Name == "Logo");
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(todos),
                new ODataKeySegment(todos.EntityType()),
                new ODataStreamPropertySegment(sp.Name));

            // Act
            var getOperation = _operationalHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(getOperation);
            Assert.Equal("Get media content for Todo from Todos", getOperation.Summary);
            Assert.NotNull(getOperation.Tags);
            var tag = Assert.Single(getOperation.Tags);
            Assert.Equal("Todos.Todo", tag.Name);

            Assert.NotNull(getOperation.Responses);
            Assert.Equal(2, getOperation.Responses.Count);
            Assert.Equal(new[] { "200", "default" }, getOperation.Responses.Select(r => r.Key));

            if (enableOperationId)
            {
                Assert.Equal("Todos.Todo.GetLogo", getOperation.OperationId);
            }
            else
            {
                Assert.Null(getOperation.OperationId);
            }
        }

        public static IEdmModel GetEdmModel()
        {
            const string modelText = @"<edmx:Edmx Version=""4.0"" xmlns:edmx=""http://docs.oasis-open.org/odata/ns/edmx"">
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
      <EntityContainer Name =""TodoService"">
         <EntitySet Name=""Todos"" EntityType=""microsoft.graph.Todo"" />
      </EntityContainer>
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>";
            bool result = CsdlReader.TryParse(XElement.Parse(modelText).CreateReader(), out IEdmModel model, out _);
            Assert.True(result);
            return model;
        }
    }
}
