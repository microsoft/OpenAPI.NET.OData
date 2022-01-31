// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Validation;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Properties;
using Xunit;

namespace Microsoft.OpenApi.OData.PathItem.Tests
{
    public class NavigationPropertyPathItemHandlerTest
    {
        private NavigationPropertyPathItemHandler _pathItemHandler = new NavigationPropertyPathItemHandler();

        [Fact]
        public void CreatePathItemThrowsForNullContext()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>("context",
                () => _pathItemHandler.CreatePathItem(context: null, path: new ODataPath()));
        }

        [Fact]
        public void CreatePathItemThrowsForNullPath()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>("path",
                () => _pathItemHandler.CreatePathItem(new ODataContext(EdmCoreModel.Instance), path: null));
        }

        [Fact]
        public void CreatePathItemThrowsForNonNavigationPropertyPath()
        {
            // Arrange
            IEdmModel model = EntitySetPathItemHandlerTests.GetEdmModel(annotation: "");
            ODataContext context = new ODataContext(model);
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(entitySet); // guard
            var path = new ODataPath(new ODataNavigationSourceSegment(entitySet));
            Assert.Equal(ODataPathKind.EntitySet, path.Kind); // guard

            // Act
            Action test = () => _pathItemHandler.CreatePathItem(context, path);

            // Assert
            var exception = Assert.Throws<InvalidOperationException>(test);
            Assert.Equal(String.Format(SRResource.InvalidPathKindForPathItemHandler, "NavigationPropertyPathItemHandler", path.Kind), exception.Message);
        }

        [Theory]
        [InlineData(true, true, new OperationType[] { OperationType.Get, OperationType.Patch, OperationType.Delete })]
        [InlineData(true, false, new OperationType[] { OperationType.Get, OperationType.Post })]
        [InlineData(false, true, new OperationType[] { OperationType.Get })]
        [InlineData(false, false, new OperationType[] { OperationType.Get})]
        public void CreateCollectionNavigationPropertyPathItemReturnsCorrectPathItem(bool containment, bool keySegment, OperationType[] expected)
        {
            // Arrange
            IEdmModel model = GetEdmModel("");
            ODataContext context = new ODataContext(model);
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(entitySet); // guard
            IEdmEntityType entityType = entitySet.EntityType();

            IEdmNavigationProperty property = entityType.DeclaredNavigationProperties()
                .FirstOrDefault(c => c.ContainsTarget == containment && c.TargetMultiplicity() == EdmMultiplicity.Many);
            Assert.NotNull(property);

            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(entitySet),
                new ODataKeySegment(entityType),
                new ODataNavigationPropertySegment(property));

            if (keySegment)
            {
                path.Push(new ODataKeySegment(property.ToEntityType()));
            }

            // Act
            var pathItem = _pathItemHandler.CreatePathItem(context, path);

            // Assert
            Assert.NotNull(pathItem);

            Assert.NotNull(pathItem.Operations);
            Assert.NotEmpty(pathItem.Operations);
            Assert.Equal(expected, pathItem.Operations.Select(o => o.Key));
            Assert.NotEmpty(pathItem.Description);
        }

        [Theory]
        [InlineData(true, new OperationType[] { OperationType.Get, OperationType.Patch, OperationType.Delete })]
        [InlineData(false, new OperationType[] { OperationType.Get })]
        public void CreateSingleNavigationPropertyPathItemReturnsCorrectPathItem(bool containment, OperationType[] expected)
        {
            // Arrange
            IEdmModel model = GetEdmModel("");
            ODataContext context = new ODataContext(model);
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(entitySet); // guard
            IEdmEntityType entityType = entitySet.EntityType();

            IEdmNavigationProperty property = entityType.DeclaredNavigationProperties()
                .FirstOrDefault(c => c.ContainsTarget == containment && c.TargetMultiplicity() != EdmMultiplicity.Many);
            Assert.NotNull(property);

            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(entitySet),
                new ODataKeySegment(entityType),
                new ODataNavigationPropertySegment(property));

            // Act
            var pathItem = _pathItemHandler.CreatePathItem(context, path);

            // Assert
            Assert.NotNull(pathItem);

            Assert.NotNull(pathItem.Operations);
            Assert.NotEmpty(pathItem.Operations);
            Assert.Equal(expected, pathItem.Operations.Select(o => o.Key));
        }

        public static IEnumerable<object[]> CollectionNavigationPropertyData
        {
            get
            {
                IList<string> navigationPropertyPaths = new List<string>
                {
                    "ContainedOrders",
                    "Orders",
                    "ContainedOrders/ContainedOrderLines",
                    "ContainedOrders/OrderLines",
                    "Orders/ContainedOrderLines",
                    "Orders/OrderLines",
                    "ContainedMyOrder/ContainedOrderLines",
                    "ContainedMyOrder/OrderLines",
                    "MyOrder/ContainedOrderLines",
                    "MyOrder/OrderLines"
                };
                foreach (var path in navigationPropertyPaths)
                {
                    foreach (var enableAnnotation in new[] { true, false })
                    {
                        yield return new object[] { enableAnnotation, path };
                    }
                }
            }
        }

        public static IEnumerable<object[]> SingleNavigationPropertyData
        {
            get
            {
                IList<string> navigationPropertyPaths = new List<string>
                {
                    "ContainedMyOrder",
                    "MyOrder",
                    "ContainedOrders/ContainedMyOrderLine",
                    "ContainedOrders/MyOrderLine",
                    "Orders/ContainedMyOrderLine",
                    "Orders/MyOrderLine",
                    "ContainedMyOrder/ContainedMyOrderLine",
                    "ContainedMyOrder/MyOrderLine",
                    "MyOrder/ContainedMyOrderLine",
                    "MyOrder/MyOrderLine"
                };
                foreach (var path in navigationPropertyPaths)
                {
                    foreach (var enableAnnotation in new[] { true, false })
                    {
                        yield return new object[] { enableAnnotation, path };
                    }
                }
            }
        }

        [Theory]
        [MemberData(nameof(CollectionNavigationPropertyData))]
        [MemberData(nameof(SingleNavigationPropertyData))]
        public void CreatePathItemForNavigationPropertyAndReadRestrictions(bool hasRestrictions, string navigationPropertyPath)
        {
            // Arrange
            string annotation = String.Format(@"
<Annotation Term=""Org.OData.Capabilities.V1.NavigationRestrictions"">
  <Record>
    <PropertyValue Property=""RestrictedProperties"" >
      <Collection>
        <Record>
          <PropertyValue Property=""NavigationProperty"" NavigationPropertyPath=""{0}"" />
          <PropertyValue Property=""ReadRestrictions"" >
            <Record>
              <PropertyValue Property=""Readable"" Bool=""false"" />
            </Record>
          </PropertyValue>
        </Record>
      </Collection>
    </PropertyValue>
  </Record>
</Annotation>", navigationPropertyPath);

            IEdmModel model = GetEdmModel(hasRestrictions ? annotation : "");
            ODataContext context = new ODataContext(model);
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(entitySet); // guard

            ODataPath path = CreatePath(entitySet, navigationPropertyPath, false);

            // Act
            var pathItem = _pathItemHandler.CreatePathItem(context, path);

            // Assert
            Assert.NotNull(pathItem);
            Assert.NotNull(pathItem.Operations);

            if (hasRestrictions)
            {
                Assert.DoesNotContain(pathItem.Operations, o => o.Key == OperationType.Get);
            }
            else
            {
                Assert.Contains(pathItem.Operations, o => o.Key == OperationType.Get);
            }
        }

        [Theory]
        [MemberData(nameof(CollectionNavigationPropertyData))]
        public void CreatePathItemForNavigationPropertyAndInsertRestrictions(bool hasRestrictions, string navigationPropertyPath)
        {
            // Arrange
            string annotation = String.Format(@"
<Annotation Term=""Org.OData.Capabilities.V1.NavigationRestrictions"">
  <Record>
    <PropertyValue Property=""RestrictedProperties"" >
      <Collection>
        <Record>
          <PropertyValue Property=""NavigationProperty"" NavigationPropertyPath=""{0}"" />
          <PropertyValue Property=""InsertRestrictions"" >
            <Record>
              <PropertyValue Property=""Insertable"" Bool=""false"" />
            </Record>
          </PropertyValue>
        </Record>
      </Collection>
    </PropertyValue>
  </Record>
</Annotation>", navigationPropertyPath);

            IEdmModel model = GetEdmModel(hasRestrictions ? annotation : "");
            ODataContext context = new ODataContext(model);
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(entitySet); // guard

            ODataPath path = CreatePath(entitySet, navigationPropertyPath, false);

            // Act
            var pathItem = _pathItemHandler.CreatePathItem(context, path);

            // Assert
            Assert.NotNull(pathItem);

            Assert.NotNull(pathItem.Operations);
            Assert.NotEmpty(pathItem.Operations);

            bool isContainment = path.Segments.OfType<ODataNavigationPropertySegment>().Last().NavigationProperty.ContainsTarget;

            OperationType[] expected;
            if (!isContainment || hasRestrictions)
            {
                expected = new[] { OperationType.Get };
            }
            else
            {
                expected = new[] { OperationType.Get, OperationType.Post };
            }

            Assert.Equal(expected, pathItem.Operations.Select(o => o.Key));
        }

        [Theory]
        [MemberData(nameof(CollectionNavigationPropertyData))]
        [MemberData(nameof(SingleNavigationPropertyData))]
        public void CreatePathItemForNavigationPropertyAndUpdateRestrictions(bool hasRestrictions, string navigationPropertyPath)
        {
            // Arrange
            string annotation = String.Format(@"
<Annotation Term=""Org.OData.Capabilities.V1.NavigationRestrictions"">
  <Record>
    <PropertyValue Property=""RestrictedProperties"" >
      <Collection>
        <Record>
          <PropertyValue Property=""NavigationProperty"" NavigationPropertyPath=""{0}"" />
          <PropertyValue Property=""UpdateRestrictions"" >
            <Record>
              <PropertyValue Property=""Updatable"" Bool=""false"" />
            </Record>
          </PropertyValue>
        </Record>
      </Collection>
    </PropertyValue>
  </Record>
</Annotation>", navigationPropertyPath);

            IEdmModel model = GetEdmModel(hasRestrictions ? annotation : "");
            ODataContext context = new ODataContext(model);
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(entitySet); // guard

            ODataPath path = CreatePath(entitySet, navigationPropertyPath, true);

            // Act
            var pathItem = _pathItemHandler.CreatePathItem(context, path);

            // Assert
            Assert.NotNull(pathItem);

            Assert.NotNull(pathItem.Operations);
            Assert.NotEmpty(pathItem.Operations);

            var navigationProperty = path.Segments.OfType<ODataNavigationPropertySegment>().Last().NavigationProperty;
            bool isContainment = navigationProperty.ContainsTarget;
            bool isCollection = navigationProperty.TargetMultiplicity() == EdmMultiplicity.Many;

            OperationType[] expected;
            if (isContainment)
            {
                if (hasRestrictions)
                {
                    expected = new[] { OperationType.Get, OperationType.Delete };
                }
                else
                {
                    expected = new[] { OperationType.Get, OperationType.Patch, OperationType.Delete };
                }
            }
            else
            {
                expected = new[] { OperationType.Get };
            }

            //if (!isContainment || hasRestrictions)
            //{
            //    if (isCollection)
            //    {
            //        expected = new[] { OperationType.Get };
            //    }
            //    else
            //    {
            //        expected = new[] { OperationType.Get, OperationType.Delete };
            //    }
            //}
            //else
            //{
            //    if (isCollection)
            //    {
            //        expected = new[] { OperationType.Get, OperationType.Patch };
            //    }
            //    else
            //    {
            //        expected = new[] { OperationType.Get, OperationType.Patch, OperationType.Delete };
            //    }
            //}

            Assert.Equal(expected, pathItem.Operations.Select(o => o.Key));
        }

        [Theory]
        [MemberData(nameof(CollectionNavigationPropertyData))]
        [MemberData(nameof(SingleNavigationPropertyData))]
        public void CreatePathItemForNavigationPropertyAndUpdateMethodUpdateRestrictions(bool updateMethod, string navigationPropertyPath)
        {
            // Arrange
            string annotation = String.Format(@"
<Annotation Term=""Org.OData.Capabilities.V1.NavigationRestrictions"">
  <Record>
    <PropertyValue Property=""RestrictedProperties"" >
      <Collection>
        <Record>
          <PropertyValue Property=""NavigationProperty"" NavigationPropertyPath=""{0}"" />
          <PropertyValue Property=""UpdateRestrictions"" >
            <Record>
              <PropertyValue Property=""UpdateMethod"">
                <EnumMember>Org.OData.Capabilities.V1.HttpMethod/PUT</EnumMember>
              </PropertyValue>
            </Record>
          </PropertyValue>
        </Record>
      </Collection>
    </PropertyValue>
  </Record>
</Annotation>", navigationPropertyPath);

            IEdmModel model = GetEdmModel(updateMethod ? annotation : "");
            ODataContext context = new ODataContext(model);
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(entitySet); // guard

            ODataPath path = CreatePath(entitySet, navigationPropertyPath, true);

            // Act
            var pathItem = _pathItemHandler.CreatePathItem(context, path);

            // Assert
            Assert.NotNull(pathItem);
            Assert.NotNull(pathItem.Operations);
            Assert.NotEmpty(pathItem.Operations);

            var navigationProperty = path.Segments.OfType<ODataNavigationPropertySegment>().Last().NavigationProperty;
            bool isContainment = navigationProperty.ContainsTarget;
            bool isCollection = navigationProperty.TargetMultiplicity() == EdmMultiplicity.Many;

            OperationType[] expected;
            if (isContainment)
            {
                if (updateMethod)
                {
                    expected = new[] { OperationType.Get, OperationType.Put, OperationType.Delete };
                }
                else
                {
                    expected = new[] { OperationType.Get, OperationType.Patch, OperationType.Delete };
                }
            }
            else
            {
                expected = new[] { OperationType.Get };
            }

            Assert.Equal(expected, pathItem.Operations.Select(o => o.Key));
        }

        public static IEdmModel GetEdmModel(string annotation)
        {
            const string template = @"<edmx:Edmx Version=""4.0"" xmlns:edmx=""http://docs.oasis-open.org/odata/ns/edmx"">
  <edmx:DataServices>
    <Schema Namespace=""NS"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
      <EntityType Name=""Customer"">
        <Key>
          <PropertyRef Name=""ID"" />
        </Key>
        <Property Name=""ID"" Type=""Edm.Int32"" Nullable=""false"" />
        <NavigationProperty Name=""ContainedOrders"" Type=""Collection(NS.Order)"" ContainsTarget=""true"" />
        <NavigationProperty Name=""Orders"" Type=""Collection(NS.Order)"" />
        <NavigationProperty Name=""ContainedMyOrder"" Type=""NS.Order"" Nullable=""false"" ContainsTarget=""true"" />
        <NavigationProperty Name=""MyOrder"" Type=""NS.Order"" Nullable=""false"" />
      </EntityType>
      <EntityType Name=""Order"">
        <Key>
          <PropertyRef Name=""ID"" />
        </Key>
        <Property Name=""ID"" Type=""Edm.Int32"" Nullable=""false"" />
        <NavigationProperty Name=""ContainedOrderLines"" Type=""Collection(NS.OrderLine)"" ContainsTarget=""true"" />
        <NavigationProperty Name=""OrderLines"" Type=""Collection(NS.OrderLine)"" />
        <NavigationProperty Name=""ContainedMyOrderLine"" Type=""NS.OrderLine"" Nullable=""false"" ContainsTarget=""true"" />
        <NavigationProperty Name=""MyOrderLine"" Type=""NS.OrderLine"" Nullable=""false"" />
      </EntityType>
      <EntityType Name=""OrderLine"">
        <Key>
          <PropertyRef Name=""ID"" />
        </Key>
        <Property Name=""ID"" Type=""Edm.Int32"" Nullable=""false"" />
      </EntityType>
      <EntityContainer Name =""Default"">
         <EntitySet Name=""Customers"" EntityType=""NS.Customer"" />
         <EntitySet Name=""Orders"" EntityType=""NS.Order"" />
      </EntityContainer>
      <Annotations Target=""NS.Default/Customers"">
        {0}
      </Annotations>
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>";

            string modelText = string.Format(template, annotation);

            IEdmModel model;
            IEnumerable<EdmError> errors;

            bool result = CsdlReader.TryParse(XElement.Parse(modelText).CreateReader(), out model, out errors);
            Assert.True(result);
            return model;
        }

        private static ODataPath CreatePath(IEdmNavigationSource navigationSource, string navigationPropertyPath, bool single)
        {
            Assert.NotNull(navigationSource);
            Assert.NotNull(navigationPropertyPath);

            IEdmEntityType previousEntityType = navigationSource.EntityType();
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(navigationSource), new ODataKeySegment(previousEntityType));

            string[] npPaths = navigationPropertyPath.Split('/');

            IEdmNavigationProperty previousProperty = null;
            foreach (string npPath in npPaths)
            {
                IEdmNavigationProperty property = previousEntityType.DeclaredNavigationProperties().FirstOrDefault(p => p.Name == npPath);
                Assert.NotNull(property);

                if (previousProperty != null)
                {
                    if (previousProperty.TargetMultiplicity() == EdmMultiplicity.Many)
                    {
                        path.Push(new ODataKeySegment(previousProperty.ToEntityType()));
                    }
                }

                path.Push(new ODataNavigationPropertySegment(property));
                previousProperty = property;
                previousEntityType = property.ToEntityType();
            }

            if (single)
            {
                if (previousProperty.TargetMultiplicity() == EdmMultiplicity.Many)
                {
                    path.Push(new ODataKeySegment(previousProperty.ToEntityType()));
                }
            }

            return path;
        }
    }
}
