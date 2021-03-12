// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OpenApi.OData.Tests;
using Xunit;

namespace Microsoft.OpenApi.OData.Edm.Tests
{
    public class ODataPathProviderTests
    {
        [Fact]
        public void GetPathsForEmptyEdmModelReturnsEmptyPaths()
        {
            // Arrange
            IEdmModel model = new EdmModel();
            ODataPathProvider provider = new ODataPathProvider();

            // Act
            var paths = provider.GetPaths(model);

            // Assert
            Assert.NotNull(paths);
            Assert.Empty(paths);
        }

        [Fact]
        public void GetPathsForGraphBetaModelReturnsAllPaths()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.GraphBetaModel;
            ODataPathProvider provider = new ODataPathProvider();

            // Act
            var paths = provider.GetPaths(model);

            // Assert
            Assert.NotNull(paths);
            Assert.Equal(20172, paths.Count());
        }

        [Fact]
        public void GetPathsForSingleEntitySetWorks()
        {
            // Arrange
            IEdmModel model = GetEdmModel("", "");
            ODataPathProvider provider = new ODataPathProvider();

            // Act
            var paths = provider.GetPaths(model);

            // Assert
            Assert.NotNull(paths);
            Assert.Equal(2, paths.Count());
            Assert.Equal(new[] { "/Customers", "/Customers({ID})" }, paths.Select(p => p.GetPathItemName()));
        }

        [Fact]
        public void GetPathsWithSingletonWorks()
        {
            // Arrange
            IEdmModel model = GetEdmModel("", @"<Singleton Name=""Me"" Type=""NS.Customer"" />");
            ODataPathProvider provider = new ODataPathProvider();

            // Act
            var paths = provider.GetPaths(model);

            // Assert
            Assert.NotNull(paths);
            Assert.Equal(3, paths.Count());
            Assert.Contains("/Me", paths.Select(p => p.GetPathItemName()));
        }

        [Fact]
        public void GetPathsWithBoundFunctionOperationWorks()
        {
            // Arrange
            string boundFunction =
@"<Function Name=""delta"" IsBound=""true"">
   <Parameter Name=""bindingParameter"" Type=""Collection(NS.Customer)"" />
     <ReturnType Type=""Collection(NS.Customer)"" />
</Function>";
            IEdmModel model = GetEdmModel(boundFunction, "");
            ODataPathProvider provider = new ODataPathProvider();

            // Act
            var paths = provider.GetPaths(model);

            // Assert
            Assert.NotNull(paths);
            Assert.Equal(3, paths.Count());
            Assert.Contains("/Customers/NS.delta()", paths.Select(p => p.GetPathItemName()));
        }

        [Fact]
        public void GetPathsWithBoundActionOperationWorks()
        {
            // Arrange
            string boundAction =
@"<Action Name=""renew"" IsBound=""true"">
   <Parameter Name=""bindingParameter"" Type=""NS.Customer"" />
     <ReturnType Type=""Edm.Boolean"" />
</Action>";
            IEdmModel model = GetEdmModel(boundAction, "");
            ODataPathProvider provider = new ODataPathProvider();

            // Act
            var paths = provider.GetPaths(model);

            // Assert
            Assert.NotNull(paths);
            Assert.Equal(3, paths.Count());
            Assert.Contains("/Customers({ID})/NS.renew", paths.Select(p => p.GetPathItemName()));
        }

        [Fact]
        public void GetPathsWithUnboundOperationImportWorks()
        {
            // Arrange
            string boundAction =
@"<Function Name=""GetNearestCustomers"">
   <ReturnType Type=""NS.Customer"" />
  </Function >
  <Action Name=""ResetDataSource"" />";

            string unbounds = @"
<FunctionImport Name=""GetNearestCustomers"" Function=""NS.GetNearestCustomers"" EntitySet=""Customers"" />
<ActionImport Name =""ResetDataSource"" Action =""NS.ResetDataSource"" />";
            IEdmModel model = GetEdmModel(boundAction, unbounds);

            ODataPathProvider provider = new ODataPathProvider();

            // Act
            var paths = provider.GetPaths(model);

            // Assert
            Assert.NotNull(paths);
            Assert.Equal(4, paths.Count());
            Assert.Contains("/GetNearestCustomers()", paths.Select(p => p.GetPathItemName()));
            Assert.Contains("/ResetDataSource", paths.Select(p => p.GetPathItemName()));
        }

        [Fact]
        public void GetPathsWithNonContainedNavigationPropertytWorks()
        {
            // Arrange
            string entityType =
@"<EntityType Name=""Order"">
    <Key>
      <PropertyRef Name=""id"" />
    </Key>
    <NavigationProperty Name=""MultipleCustomers"" Type=""Collection(NS.Customer)"" />
    <NavigationProperty Name=""SingleCustomer"" Type=""NS.Customer"" />
  </EntityType>";

            string entitySet = @"<EntitySet Name=""Orders"" EntityType=""NS.Order"" />";
            IEdmModel model = GetEdmModel(entityType, entitySet);

            ODataPathProvider provider = new ODataPathProvider();

            // Act
            var paths = provider.GetPaths(model);

            // Assert
            Assert.NotNull(paths);
            Assert.Equal(8, paths.Count());

            var pathItems = paths.Select(p => p.GetPathItemName()).ToList();
            Assert.Contains("/Orders({id})/MultipleCustomers", pathItems);
            Assert.Contains("/Orders({id})/SingleCustomer", pathItems);
            Assert.Contains("/Orders({id})/SingleCustomer/$ref", pathItems);
            Assert.Contains("/Orders({id})/MultipleCustomers/$ref", pathItems);
        }

        [Fact]
        public void GetPathsWithContainedNavigationPropertytWorks()
        {
            // Arrange
            string entityType =
@"<EntityType Name=""Order"">
    <Key>
      <PropertyRef Name=""id"" />
    </Key>
    <NavigationProperty Name=""MultipleCustomers"" Type=""Collection(NS.Customer)"" ContainsTarget=""true"" />
    <NavigationProperty Name=""SingleCustomer"" Type=""NS.Customer"" ContainsTarget=""true"" />
  </EntityType>";

            string entitySet = @"<EntitySet Name=""Orders"" EntityType=""NS.Order"" />";
            IEdmModel model = GetEdmModel(entityType, entitySet);
            ODataPathProvider provider = new ODataPathProvider();

            // Act
            var paths = provider.GetPaths(model);

            // Assert
            Assert.NotNull(paths);
            Assert.Equal(7, paths.Count());

            var pathItems = paths.Select(p => p.GetPathItemName()).ToList();
            Assert.Contains("/Orders({id})/MultipleCustomers", pathItems);
            Assert.Contains("/Orders({id})/MultipleCustomers({ID})", pathItems);
            Assert.Contains("/Orders({id})/SingleCustomer", pathItems);
        }

        [Theory]
        [InlineData(true, "Logo")]
        [InlineData(false, "Logo")]
        [InlineData(true, "Content")]
        [InlineData(false, "Content")]
        public void GetPathsWithStreamPropertyAndWithEntityHasStreamWorks(bool hasStream, string streamPropName)
        {
            // Arrange
            IEdmModel model = GetEdmModel(hasStream, streamPropName);
            ODataPathProvider provider = new ODataPathProvider();

            // Act
            var paths = provider.GetPaths(model);

            // Assert
            Assert.NotNull(paths);

            if (hasStream && !streamPropName.Equals("Content", StringComparison.OrdinalIgnoreCase))
            {
                Assert.Equal(7, paths.Count());
                Assert.Equal(new[] { "/me", "/me/photo", "/me/photo/$value", "/Todos", "/Todos({Id})", "/Todos({Id})/$value", "/Todos({Id})/Logo" },
                    paths.Select(p => p.GetPathItemName()));
            }
            else if ((hasStream && streamPropName.Equals("Content", StringComparison.OrdinalIgnoreCase)) ||
                    (!hasStream && streamPropName.Equals("Content", StringComparison.OrdinalIgnoreCase)))
            {
                Assert.Equal(6, paths.Count());
                Assert.Equal(new[] { "/me", "/me/photo", "/me/photo/$value", "/Todos", "/Todos({Id})", "/Todos({Id})/Content" },
                    paths.Select(p => p.GetPathItemName()));
            }
            else // !hasStream && !streamPropName.Equals("Content")
            {
                Assert.Equal(6, paths.Count());
                Assert.Equal(new[] { "/me", "/me/photo", "/me/photo/$value", "/Todos", "/Todos({Id})", "/Todos({Id})/Logo"},
                    paths.Select(p => p.GetPathItemName()));
            }
        }

        private static IEdmModel GetEdmModel(string schemaElement, string containerElement)
        {
            string template = @"<?xml version=""1.0"" encoding=""utf-16""?>
<Schema Namespace=""NS"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
  <EntityType Name=""Customer"">
    <Key>
      <PropertyRef Name=""ID"" />
    </Key>
    <Property Name=""ID"" Type=""Edm.Int32"" Nullable=""false"" />
  </EntityType>
  {0}
  <EntityContainer Name =""Default"">
    <EntitySet Name=""Customers"" EntityType=""NS.Customer"" />
    {1}
  </EntityContainer>
</Schema>";
            string schema = string.Format(template, schemaElement, containerElement);
            bool parsed = SchemaReader.TryParse(new XmlReader[] { XmlReader.Create(new StringReader(schema)) }, out IEdmModel parsedModel, out _);
            Assert.True(parsed);
            return parsedModel;
        }

        private static IEdmModel GetEdmModel(bool hasStream, string streamPropName)
        {
            string template = @"<edmx:Edmx Version=""4.0"" xmlns:edmx=""http://docs.oasis-open.org/odata/ns/edmx"">
  <edmx:DataServices>
    <Schema Namespace=""microsoft.graph"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
      <EntityType Name=""Todo"" HasStream=""{0}"">
        <Key>
          <PropertyRef Name=""Id"" />
        </Key>
        <Property Name=""Id"" Type=""Edm.Int32"" Nullable=""false"" />
        <Property Name=""{1}"" Type=""Edm.Stream""/>
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
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>";
            string modelText = string.Format(template, hasStream, streamPropName);
            bool result = CsdlReader.TryParse(XElement.Parse(modelText).CreateReader(), out IEdmModel model, out _);
            Assert.True(result);
            return model;
        }
    }
}
