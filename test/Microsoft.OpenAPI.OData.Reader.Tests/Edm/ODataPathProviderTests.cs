// --------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// --------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Validation;
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
            var settings = new OpenApiConvertSettings();

            // Act
            var paths = provider.GetPaths(model, settings);

            // Assert
            Assert.NotNull(paths);
            Assert.Empty(paths);
        }

        [Fact]
        public void GetPathsForGraphBetaModelReturnsAllPaths()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.GraphBetaModel;
            ODataPathProvider provider = new();
            OpenApiConvertSettings settings = new()
            {
                AddAlternateKeyPaths = true,
                PrefixEntityTypeNameBeforeKey = true
            };

            // Act
            var paths = provider.GetPaths(model, settings);

            // Assert
            Assert.NotNull(paths);
            Assert.Equal(15210, paths.Count());
            AssertGraphBetaModelPaths(paths);
        }

        private void AssertGraphBetaModelPaths(IEnumerable<ODataPath> paths)
        {
            // Test that $count and microsoft.graph.count() segments are not both created for the same path.
            Assert.Null(paths.FirstOrDefault(p => p.GetPathItemName().Equals("/drives({id})/items({id1})/workbook/tables/$count")));
            Assert.NotNull(paths.FirstOrDefault(p => p.GetPathItemName().Equals("/drives({id})/items({id1})/workbook/tables/microsoft.graph.count()")));

            // Test that $value segments are created for entity types with base types with HasStream="true"
            Assert.NotNull(paths.FirstOrDefault(p => p.GetPathItemName().Equals("/me/chats({id})/messages({id1})/hostedContents({id2})/$value")));

            // Test that count restrictions annotations for navigation properties work
            Assert.Null(paths.FirstOrDefault(p => p.GetPathItemName().Equals("/me/drives/$count")));

            // Test that navigation properties on base types are created
            Assert.NotNull(paths.FirstOrDefault(p => p.GetPathItemName().Equals("/print/printers({id})/jobs")));

            // Test that RequiresExplicitBinding and ExplicitOperationBindings annotations work
            Assert.Null(paths.FirstOrDefault(p => p.GetPathItemName().Equals("/directory/deletedItems({id})/microsoft.graph.checkMemberGroups")));
            Assert.Null(paths.FirstOrDefault(p => p.GetPathItemName().Equals("/directory/deletedItems({id})/microsoft.graph.checkMemberObjects")));
            Assert.Null(paths.FirstOrDefault(p => p.GetPathItemName().Equals("/directory/deletedItems({id})/microsoft.graph.getMemberGroups")));
            Assert.Null(paths.FirstOrDefault(p => p.GetPathItemName().Equals("/directory/deletedItems({id})/microsoft.graph.getMemberObjects")));
            Assert.NotNull(paths.FirstOrDefault(p => p.GetPathItemName().Equals("/directory/deletedItems({id})/microsoft.graph.restore")));

            Assert.NotNull(paths.FirstOrDefault(p => p.GetPathItemName().Equals("/directoryObjects({id})/microsoft.graph.checkMemberGroups")));
            Assert.NotNull(paths.FirstOrDefault(p => p.GetPathItemName().Equals("/directoryObjects({id})/microsoft.graph.checkMemberObjects")));
            Assert.NotNull(paths.FirstOrDefault(p => p.GetPathItemName().Equals("/directoryObjects({id})/microsoft.graph.getMemberGroups")));
            Assert.NotNull(paths.FirstOrDefault(p => p.GetPathItemName().Equals("/directoryObjects({id})/microsoft.graph.getMemberObjects")));
            Assert.Null(paths.FirstOrDefault(p => p.GetPathItemName().Equals("/directoryObjects({id})/microsoft.graph.restore")));

            // Test that complex and navigation properties within derived types are appended
            Assert.NotNull(paths.FirstOrDefault(p => p.GetPathItemName().Equals(
                "/identity/authenticationEventsFlows({id})/microsoft.graph.externalUsersSelfServiceSignUpEventsFlow/onAttributeCollection/microsoft.graph.onAttributeCollectionExternalUsersSelfServiceSignUp/attributes")));
            Assert.NotNull(paths.FirstOrDefault(p => p.GetPathItemName().Equals(
                "/identity/authenticationEventsFlows({id})/microsoft.graph.externalUsersSelfServiceSignUpEventsFlow/onAuthenticationMethodLoadStart/microsoft.graph.onAuthenticationMethodLoadStartExternalUsersSelfServiceSignUp/identityProviders")));

            // Test that navigation properties within nested complex properties are appended
            Assert.NotNull(paths.FirstOrDefault(p => p.GetPathItemName().Equals(
                "/identity/authenticationEventsFlows({id})/conditions/applications/includeApplications")));

            // Test that alternate keys are appended for collection navigation properties
            Assert.NotNull(paths.FirstOrDefault(p => p.GetPathItemName().Equals(
                "/employeeExperience/learningProviders({id})/learningContents(externalId='{externalId}')")));
        }

        [Fact]
        public void GetPathsForGraphBetaModelWithDerivedTypesConstraintReturnsAllPaths()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.GraphBetaModel;
            ODataPathProvider provider = new ODataPathProvider();
            var settings = new OpenApiConvertSettings
            {
                RequireDerivedTypesConstraintForBoundOperations = true,
                AppendBoundOperationsOnDerivedTypeCastSegments = true
            };

            // Act
            var paths = provider.GetPaths(model, settings);


            // Assert
            Assert.NotNull(paths);
            Assert.Equal(15861, paths.Count());
        }
                
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetEntitySetPathsWithIndexableByKeyAnnotationWorks(bool indexable)
        {
            // Arrange
            string indexableAnnotation = @"<Annotation Term=""Org.OData.Capabilities.V1.IndexableByKey"" Bool=""{0}"" />";
            indexableAnnotation = string.Format(indexableAnnotation, indexable);
            IEdmModel model = GetInheritanceModel(indexableAnnotation);
            ODataPathProvider provider = new();
            var settings = new OpenApiConvertSettings();

            // Act & Assert
            var paths = provider.GetPaths(model, settings);
            Assert.NotNull(paths);

            if (indexable)
            {
                Assert.Equal(3, paths.Count());
                Assert.Contains("/Customers({ID})", paths.Select(p => p.GetPathItemName()));
            }
            else
            {
                Assert.Equal(2, paths.Count());
                Assert.DoesNotContain("/Customers({ID})", paths.Select(p => p.GetPathItemName()));
            }
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, false)]
        public void UseCountRestrictionsAnnotationsToAppendDollarCountSegmentsToNavigationPropertyPaths(bool useCountRestrictionsAnnotation, bool countable)
        {
            // Arrange
            string countRestrictionAnnotation = @"
<Annotation Term=""Org.OData.Capabilities.V1.CountRestrictions"">
    <Record>
        <PropertyValue Property=""Countable"" Bool=""{0}"" />
    </Record>
</Annotation>";
            countRestrictionAnnotation = string.Format(countRestrictionAnnotation, countable);
            IEdmModel model = useCountRestrictionsAnnotation ? GetNavPropModel(countRestrictionAnnotation)
                : GetNavPropModel(string.Empty);
            ODataPathProvider provider = new();
            var settings = new OpenApiConvertSettings();

            // Act
            var paths = provider.GetPaths(model, settings);

            // Assert
            Assert.NotNull(paths);
            var testPath = paths.FirstOrDefault(p => p.GetPathItemName().Equals("/Root/Customers/$count"));

            if (useCountRestrictionsAnnotation)
            {
                if (countable)
                {
                    Assert.NotNull(testPath);
                }                    
                else
                {
                    Assert.Null(testPath);
                }                    
            }
            else
            {
                Assert.NotNull(testPath);
            }
        }

        [Fact]
        public void GetPathsForComposableFunctionsReturnsAllPaths()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.ComposableFunctionsModel;
            ODataPathProvider provider = new ODataPathProvider();
            var settings = new OpenApiConvertSettings
            {
                RequireDerivedTypesConstraintForBoundOperations = true,
                AppendBoundOperationsOnDerivedTypeCastSegments = true
            };

            // Act
            var paths = provider.GetPaths(model, settings);

            // Assert
            Assert.NotNull(paths);
            Assert.Equal(26, paths.Count());
            Assert.Equal(17, paths.Where(p => p.LastSegment is ODataOperationSegment).Count());
            Assert.Equal(3, paths.Where(p => p.Segments.Count > 1 && p.LastSegment is ODataNavigationPropertySegment && p.Segments[p.Segments.Count - 2] is ODataOperationSegment).Count());
        }

        [Fact]
        public void GetPathsDoesntReturnPathsForCountWhenDisabled()
        {
            // Arrange
            IEdmModel model = GetInheritanceModel(string.Empty);
            ODataPathProvider provider = new ODataPathProvider();
            var settings = new OpenApiConvertSettings {
                EnableDollarCountPath = false,
                AppendBoundOperationsOnDerivedTypeCastSegments = true
            };

            // Act
            var paths = provider.GetPaths(model, settings);

            // Assert
            Assert.NotNull(paths);
            Assert.Equal(3, paths.Count());
        }
        private const string derivedTypeAnnotation = @"
<Annotation Term=""Org.OData.Validation.V1.DerivedTypeConstraint"">
<Collection>
  <String>NS.Customer</String>
  <String>NS.NiceCustomer</String>
</Collection>
</Annotation>";

        [Theory]
        [InlineData(false, false, true, true, 3)]
        [InlineData(false, false, false, true, 4)]
        [InlineData(false, false, false, false, 3)]
        [InlineData(true, false, true, true, 7)]
        [InlineData(true, false, true, false, 6)]
        [InlineData(true, false, false, true, 7)]
        [InlineData(true, false, false, false, 6)]
        [InlineData(false, true, false, true, 5)]
        [InlineData(false, true, false, false, 4)]
        [InlineData(false, true, true, true, 4)]
        [InlineData(true, true, true, true, 8)]
        [InlineData(true, true, true, false, 7)]
        [InlineData(true, true, false, true, 8)]
        [InlineData(true, true, false, false, 7)]
        public void GetOperationPathsForModelWithDerivedTypesConstraint(
            bool addAnnotation,
            bool getNavPropModel,
            bool requireConstraint,
            bool appendBoundOperationsOnDerivedTypes,
            int expectedCount)
        {
            // Arrange
            var annotation = addAnnotation ? derivedTypeAnnotation : string.Empty;
            IEdmModel model = getNavPropModel ? GetNavPropModel(annotation) : GetInheritanceModel(annotation);
            ODataPathProvider provider = new();
            var settings = new OpenApiConvertSettings
            {
                RequireDerivedTypesConstraintForBoundOperations = requireConstraint,
                AppendBoundOperationsOnDerivedTypeCastSegments = appendBoundOperationsOnDerivedTypes
            };

            // Act
            var paths = provider.GetPaths(model, settings);

            // Assert
            Assert.NotNull(paths);
            Assert.Equal(expectedCount, paths.Count());
            var dollarCountPathsWithCastSegment = paths.Where(x => x.Kind == ODataPathKind.DollarCount && x.Any(y => y.Kind == ODataSegmentKind.TypeCast));
            if(addAnnotation && !getNavPropModel)
              Assert.Single(dollarCountPathsWithCastSegment);
        }
        [Theory]
        [InlineData(false, false, true, true, 4)]
        [InlineData(false, false, true, false, 3)]
        [InlineData(false, false, false, true, 7)]
        [InlineData(false, false, false, false, 6)]
        [InlineData(true, false, true, true, 7)]
        [InlineData(true, false, true, false, 6)]
        [InlineData(true, false, false, true, 7)]
        [InlineData(false, true, false, true, 8)]
        [InlineData(false, true, false, false, 7)]
        [InlineData(false, true, true, true, 5)]
        [InlineData(false, true, true, false, 4)]
        [InlineData(true, true, true, true, 8)]
        [InlineData(true, true, true, false, 7)]
        [InlineData(true, true, false, true, 8)]
        [InlineData(true, true, false, false, 7)]
        public void GetTypeCastPathsForModelWithDerivedTypesConstraint(
            bool addAnnotation,
            bool getNavPropModel,
            bool requireConstraint,
            bool appendBoundOperationsOnDerivedTypes,
            int expectedCount)
        {
            // Arrange
            var annotation = addAnnotation ? derivedTypeAnnotation : string.Empty;
            IEdmModel model = getNavPropModel ? GetNavPropModel(annotation) : GetInheritanceModel(annotation);
            ODataPathProvider provider = new();
            var settings = new OpenApiConvertSettings
            {
                RequireDerivedTypesConstraintForODataTypeCastSegments = requireConstraint,
                AppendBoundOperationsOnDerivedTypeCastSegments = appendBoundOperationsOnDerivedTypes
            };

            // Act
            var paths = provider.GetPaths(model, settings);

            // Assert
            Assert.NotNull(paths);
            Assert.Equal(expectedCount, paths.Count());
            var dollarCountPathsWithCastSegment = paths.Where(x => x.Kind == ODataPathKind.DollarCount && x.Any(y => y.Kind == ODataSegmentKind.TypeCast));
            if(addAnnotation || !requireConstraint)
              Assert.Single(dollarCountPathsWithCastSegment);
            else
              Assert.Empty(dollarCountPathsWithCastSegment);
        }
#if DEBUG
        // Super useful for debugging tests.
        private string ListToString(IEnumerable<ODataPath> paths)
        {
            return string.Join(Environment.NewLine,
                paths.Select(p => string.Join("/", p.Segments.Select(s => s.Identifier))));
        }
#endif

        [Fact]
        public void GetPathsForSingleEntitySetWorks()
        {
            // Arrange
            IEdmModel model = GetEdmModel("", "");
            ODataPathProvider provider = new ODataPathProvider();
            var settings = new OpenApiConvertSettings();

            // Act
            var paths = provider.GetPaths(model, settings);

            // Assert
            Assert.NotNull(paths);
            Assert.Equal(3, paths.Count());
            Assert.Equal(new[] { "/Customers", "/Customers({ID})", "/Customers/$count" }, paths.Select(p => p.GetPathItemName()));
        }

        [Fact]
        public void GetPathsWithSingletonWorks()
        {
            // Arrange
            IEdmModel model = GetEdmModel("", @"<Singleton Name=""Me"" Type=""NS.Customer"" />");
            ODataPathProvider provider = new ODataPathProvider();
            var settings = new OpenApiConvertSettings();

            // Act
            var paths = provider.GetPaths(model, settings);

            // Assert
            Assert.NotNull(paths);
            Assert.Equal(4, paths.Count());
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
            var settings = new OpenApiConvertSettings();

            // Act
            var paths = provider.GetPaths(model, settings);

            // Assert
            Assert.NotNull(paths);
            Assert.Equal(4, paths.Count());
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
            var settings = new OpenApiConvertSettings();

            // Act
            var paths = provider.GetPaths(model, settings);

            // Assert
            Assert.NotNull(paths);
            Assert.Equal(4, paths.Count());
            Assert.Contains("/Customers({ID})/NS.renew", paths.Select(p => p.GetPathItemName()));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetPathsWithBoundActionOperationForContainmentNavigationPropertyPathsWorks(bool containsTarget)
        {
            // Arrange
            string navProp = $@"<NavigationProperty Name=""Referral"" Type=""NS.NiceCustomer"" ContainsTarget=""{containsTarget}""/>";
            string boundAction =
@"<Action Name=""Ack"" IsBound=""true"">
   <Parameter Name=""bindingParameter"" Type=""NS.NiceCustomer"" />
     <ReturnType Type=""Edm.Boolean"" />
</Action>
<EntityType Name=""NiceCustomer"">
    <Property Name=""Other"" Type=""Edm.Int32"" Nullable=""true"" />
</EntityType>";

            IEdmModel model = GetEdmModel(boundAction, "", navProp);
            ODataPathProvider provider = new ODataPathProvider();
            var settings = new OpenApiConvertSettings();

            // Act
            var paths = provider.GetPaths(model, settings);

            // Assert
            Assert.NotNull(paths);

            if (containsTarget)
            {
                Assert.Equal(5, paths.Count());
                Assert.Contains("/Customers({ID})/Referral/NS.Ack", paths.Select(p => p.GetPathItemName()));
            }
            else
            {
                Assert.Equal(4, paths.Count());
                Assert.DoesNotContain("/Customers({ID})/Referral/NS.Ack", paths.Select(p => p.GetPathItemName()));
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetPathsWithBoundFunctionOperationForContainmentNavigationPropertyPathsWorks(bool containsTarget)
        {
            // Arrange
            string navProp = $@"<NavigationProperty Name=""Referral"" Type=""NS.NiceCustomer"" ContainsTarget=""{containsTarget}""/>";
            string boundAction =
@"<Function Name=""Search"" IsBound=""true"">
   <Parameter Name=""bindingParameter"" Type=""NS.NiceCustomer"" />
     <ReturnType Type=""Collection(NS.Customer)"" />
</Function>
<EntityType Name=""NiceCustomer"">
    <Property Name=""Other"" Type=""Edm.Int32"" Nullable=""true"" />
</EntityType>";

            IEdmModel model = GetEdmModel(boundAction, "", navProp);
            ODataPathProvider provider = new ODataPathProvider();
            var settings = new OpenApiConvertSettings();

            // Act
            var paths = provider.GetPaths(model, settings);

            // Assert
            Assert.NotNull(paths);

            if (containsTarget)
            {
                Assert.Equal(5, paths.Count());
                Assert.Contains("/Customers({ID})/Referral/NS.Search()", paths.Select(p => p.GetPathItemName()));
            }
            else
            {
                Assert.Equal(4, paths.Count());
                Assert.DoesNotContain("/Customers({ID})/Referral/NS.Search()", paths.Select(p => p.GetPathItemName()));
            }
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
            var settings = new OpenApiConvertSettings();

            // Act
            var paths = provider.GetPaths(model, settings);

            // Assert
            Assert.NotNull(paths);
            Assert.Equal(5, paths.Count());
            Assert.Contains("/GetNearestCustomers()", paths.Select(p => p.GetPathItemName()));
            Assert.Contains("/ResetDataSource", paths.Select(p => p.GetPathItemName()));
        }

        [Fact]
        public void GetPathsWithFalseNavigabilityInNavigationRestrictionsAnnotationWorks()
        {
            // Arrange
            string entityType =
@"<EntityType Name=""Order"">
    <Key>
      <PropertyRef Name=""id"" />
    </Key>
    <NavigationProperty Name=""MultipleCustomers"" Type=""Collection(NS.Customer)"" />
    <NavigationProperty Name=""SingleCustomer"" Type=""NS.Customer"" >
        <Annotation Term=""Org.OData.Capabilities.V1.NavigationRestrictions"">
            <Record>
                <PropertyValue Property = ""Navigability"">
                    <EnumMember>Org.OData.Capabilities.V1.NavigationType/None</EnumMember>
                </PropertyValue>
            </Record>
        </Annotation>
     </NavigationProperty>
  </EntityType>";

            string entitySet = @"<EntitySet Name=""Orders"" EntityType=""NS.Order"" />";
            IEdmModel model = GetEdmModel(entityType, entitySet);

            ODataPathProvider provider = new ODataPathProvider();
            var settings = new OpenApiConvertSettings();

            // Act
            var paths = provider.GetPaths(model, settings);

            // Assert
            Assert.NotNull(paths);
            Assert.Equal(9, paths.Count());

            var pathItems = paths.Select(p => p.GetPathItemName()).ToList();
            Assert.DoesNotContain("/Orders({id})/SingleCustomer", pathItems);
            Assert.DoesNotContain("/Orders({id})/SingleCustomer/$ref", pathItems);
        }

        [Fact]
        public void GetPathsWithFalseIndexabilityByKeyInNavigationRestrictionsAnnotationWorks()
        {
            // Arrange
            string entityType =
@"<EntityType Name=""Order"">
    <Key>
      <PropertyRef Name=""id"" />
    </Key>
    <NavigationProperty Name=""MultipleCustomers"" Type=""Collection(NS.Customer)"" ContainsTarget=""true"" >
        <Annotation Term=""Org.OData.Capabilities.V1.NavigationRestrictions"">
            <Record>
                <PropertyValue Property=""RestrictedProperties"" >
                    <Collection>
                        <Record>
                            <PropertyValue Property=""IndexableByKey"" Bool=""false"" />
                        </Record>
                    </Collection>
                </PropertyValue>
            </Record>
        </Annotation>
    </NavigationProperty>
    <NavigationProperty Name=""SingleCustomer"" Type=""NS.Customer"" />
  </EntityType>";

            string entitySet = @"<EntitySet Name=""Orders"" EntityType=""NS.Order"" />";
            IEdmModel model = GetEdmModel(entityType, entitySet);

            ODataPathProvider provider = new ODataPathProvider();
            var settings = new OpenApiConvertSettings();

            // Act
            var paths = provider.GetPaths(model, settings);

            // Assert
            Assert.NotNull(paths);
            Assert.Equal(8, paths.Count());

            var pathItems = paths.Select(p => p.GetPathItemName()).ToList();
            Assert.DoesNotContain("/Orders({id})/MultipleCustomers({ID})", pathItems);
        }

        [Fact]
        public void GetPathsWithReferenceableNavigationPropertyWorks()
        {
            // Arrange
            string entityType =
@"<EntityType Name=""Order"">
    <Key>
      <PropertyRef Name=""id"" />
    </Key>
    <NavigationProperty Name=""MultipleCustomers"" Type=""Collection(NS.Customer)"">
        <Annotation Term=""Org.OData.Capabilities.V1.NavigationRestrictions"">
          <Record>
            <PropertyValue Property=""Referenceable"" Bool=""true"" />
          </Record>
        </Annotation>
     </NavigationProperty>
    <NavigationProperty Name=""SingleCustomer"" Type=""NS.Customer"">
        <Annotation Term=""Org.OData.Capabilities.V1.NavigationRestrictions"">
          <Record>
            <PropertyValue Property=""Referenceable"" Bool=""true"" />
          </Record> 
        </Annotation>
     </NavigationProperty>
  </EntityType>";

            string entitySet = @"<EntitySet Name=""Orders"" EntityType=""NS.Order"" />";
            IEdmModel model = GetEdmModel(entityType, entitySet);

            ODataPathProvider provider = new ODataPathProvider();
            var settings = new OpenApiConvertSettings();

            // Act
            var paths = provider.GetPaths(model, settings);

            // Assert
            Assert.NotNull(paths);
            Assert.Equal(12, paths.Count());

            var pathItems = paths.Select(p => p.GetPathItemName()).ToList();
            Assert.Contains("/Orders({id})/MultipleCustomers", pathItems);
            Assert.Contains("/Orders({id})/SingleCustomer", pathItems);
            Assert.Contains("/Orders({id})/SingleCustomer/$ref", pathItems);
            Assert.Contains("/Orders({id})/MultipleCustomers/$ref", pathItems);
            Assert.Contains("/Orders({id})/MultipleCustomers({ID})/$ref", pathItems);
        }

        [Fact]
        public void GetPathsWithNonReferenceableNavigationPropertyWorks()
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
            var settings = new OpenApiConvertSettings();

            // Act
            var paths = provider.GetPaths(model, settings);

            // Assert
            Assert.NotNull(paths);
            Assert.Equal(10, paths.Count());

            var pathItems = paths.Select(p => p.GetPathItemName()).ToList();
            Assert.Contains("/Orders({id})/MultipleCustomers", pathItems);
            Assert.Contains("/Orders({id})/SingleCustomer", pathItems);
            Assert.Contains("/Orders({id})/SingleCustomer", pathItems);
            Assert.Contains("/Orders({id})/MultipleCustomers", pathItems);
            Assert.Contains("/Orders({id})/MultipleCustomers({ID})", pathItems);
        }

        [Fact]
        public void GetPathsWithContainedNavigationPropertyWorks()
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
            var settings = new OpenApiConvertSettings();

            // Act
            var paths = provider.GetPaths(model, settings);

            // Assert
            Assert.NotNull(paths);
            Assert.Equal(10, paths.Count());

            var pathItems = paths.Select(p => p.GetPathItemName()).ToList();
            Assert.Contains("/Orders({id})/MultipleCustomers", pathItems);
            Assert.Contains("/Orders({id})/MultipleCustomers({ID})", pathItems);
            Assert.Contains("/Orders({id})/SingleCustomer", pathItems);
        }

        [Theory]
        [InlineData(true, "logo")]
        [InlineData(false, "logo")]
        [InlineData(true, "content")]
        [InlineData(false, "content")]
        public void GetPathsWithStreamPropertyAndWithEntityHasStreamWorks(bool hasStream, string streamPropName)
        {
            // Arrange
            IEdmModel model = GetEdmModel(hasStream, streamPropName);
            ODataPathProvider provider = new ODataPathProvider();
            var settings = new OpenApiConvertSettings();
            const string TodosContentPath = "/todos({id})/content";
            const string TodosValuePath = "/todos({id})/$value";
            const string TodosLogoPath = "/todos({id})/logo";

            // Act
            var paths = provider.GetPaths(model, settings);

            // Assert
            Assert.NotNull(paths);
            Assert.Contains("/catalog/content", paths.Select(p => p.GetPathItemName()));
            Assert.Contains("/catalog/thumbnailPhoto", paths.Select(p => p.GetPathItemName()));
            Assert.Contains("/me/photo/$value", paths.Select(p => p.GetPathItemName()));

            if (streamPropName.Equals("logo"))
            {
                if (hasStream)
                {
                    Assert.Equal(14, paths.Count());
                    Assert.Contains(TodosValuePath, paths.Select(p => p.GetPathItemName()));
                    Assert.Contains(TodosLogoPath, paths.Select(p => p.GetPathItemName()));
                    Assert.DoesNotContain(TodosContentPath, paths.Select(p => p.GetPathItemName()));
                }
                else
                {
                    Assert.Equal(13, paths.Count());
                    Assert.Contains(TodosLogoPath, paths.Select(p => p.GetPathItemName()));
                    Assert.DoesNotContain(TodosContentPath, paths.Select(p => p.GetPathItemName()));
                    Assert.DoesNotContain(TodosValuePath, paths.Select(p => p.GetPathItemName()));
                }
            }
            else if (streamPropName.Equals("content"))
            {
                Assert.Equal(13, paths.Count());
                Assert.Contains(TodosContentPath, paths.Select(p => p.GetPathItemName()));
                Assert.DoesNotContain(TodosLogoPath, paths.Select(p => p.GetPathItemName()));
                Assert.DoesNotContain(TodosValuePath, paths.Select(p => p.GetPathItemName()));
            }
        }

        [Fact]
        public void GetPathsWithAlternateKeyParametersWorks()
        {
            string alternateKeyProperty =
@"<Property Name=""SSN"" Type=""Edm.String""/>
    <Annotation Term=""Org.OData.Core.V1.AlternateKeys"">
        <Collection>
            <Record Type=""Org.OData.Core.V1.AlternateKey"">
                <PropertyValue Property=""Key"">
                <Collection>
                    <Record Type=""Org.OData.Core.V1.PropertyRef"">
                        <PropertyValue Property=""Alias"" String=""SSN""/>
                        <PropertyValue Property=""Name"" PropertyPath=""SSN""/>
                    </Record>
                </Collection>
                </PropertyValue>
            </Record>
        </Collection>
    </Annotation>";

            IEdmModel model = GetEdmModel(null, null, alternateKeyProperty);
            ODataPathProvider provider = new();
            OpenApiConvertSettings settings = new()
            {
                EnableKeyAsSegment = true,
                AddAlternateKeyPaths= true
            };

            // Act
            IEnumerable<ODataPath> paths = provider.GetPaths(model, settings);

            // Assert
            Assert.NotNull(paths);
            Assert.Equal(4, paths.Count());

            List<string> pathItems = paths.Select(p => p.GetPathItemName(settings)).ToList();
            Assert.Contains("/Customers/{ID}", pathItems);
            Assert.Contains("/Customers(SSN='{SSN}')", pathItems);
        }

        [Fact]
        public void GetPathsWithCompositeAlternateKeyParametersWorks()
        {
            string alternateKeyProperties =
@"<Property Name=""UserName"" Type=""Edm.String"" Nullable=""false"" />
    <Property Name=""AppID"" Type=""Edm.String"" Nullable=""false"" />
    <Annotation Term=""Org.OData.Core.V1.AlternateKeys"">
        <Collection>
            <Record Type=""Org.OData.Core.V1.AlternateKey"">
                <PropertyValue Property=""Key"">
                <Collection>
                    <Record Type=""Org.OData.Core.V1.PropertyRef"">
                        <PropertyValue Property=""Alias"" String=""username""/>
                        <PropertyValue Property=""Name"" PropertyPath=""UserName""/>
                    </Record>
                    <Record Type=""Org.OData.Core.V1.PropertyRef"">
                        <PropertyValue Property=""Alias"" String=""appId""/>
                        <PropertyValue Property=""Name"" PropertyPath=""AppID""/>
                    </Record>
                </Collection>
                </PropertyValue>
            </Record>
        </Collection>
    </Annotation>";

            IEdmModel model = GetEdmModel(null, null, alternateKeyProperties);
            ODataPathProvider provider = new();
            OpenApiConvertSettings settings = new()
            {
                EnableKeyAsSegment = true,
                AddAlternateKeyPaths = true
            };

            // Act
            IEnumerable<ODataPath> paths = provider.GetPaths(model, settings);

            // Assert
            Assert.NotNull(paths);
            Assert.Equal(4, paths.Count());

            List<string> pathItems = paths.Select(p => p.GetPathItemName(settings)).ToList();
            Assert.Contains("/Customers/{ID}", pathItems);
            Assert.Contains("/Customers(username='{UserName}',appId='{AppID}')", pathItems);
        }

        private static IEdmModel GetEdmModel(string schemaElement, string containerElement, string propertySchema = null)
        {
            string template = $@"<?xml version=""1.0"" encoding=""utf-16""?>
<Schema Namespace=""NS"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
  <EntityType Name=""Customer"">
    <Key>
      <PropertyRef Name=""ID"" />
    </Key>
    <Property Name=""ID"" Type=""Edm.Int32"" Nullable=""false"" />
    {propertySchema}
  </EntityType>
  {schemaElement}
  <EntityContainer Name =""Default"">
    <EntitySet Name=""Customers"" EntityType=""NS.Customer"" />
    {containerElement}
  </EntityContainer>
</Schema>";
            return GetEdmModel(template);
        }

        private static IEdmModel GetInheritanceModel(string annotation)
        {
            string template = $@"<?xml version=""1.0"" encoding=""utf-16""?>
<Schema Namespace=""NS"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
  <EntityType Name=""Customer"">
    <Key>
      <PropertyRef Name=""ID"" />
    </Key>
    <Property Name=""ID"" Type=""Edm.Int32"" Nullable=""false"" />
  </EntityType>
  <EntityType Name=""NiceCustomer"" BaseType=""NS.Customer"">
    <Property Name=""Other"" Type=""Edm.Int32"" Nullable=""true"" />
  </EntityType>
  <Action Name=""Ack"" IsBound=""true"" >
    <Parameter Name = ""bindingParameter"" Type=""NS.NiceCustomer"" />
  </Action>
  <EntityContainer Name =""Default"">
    <EntitySet Name=""Customers"" EntityType=""NS.Customer"">
      {annotation}
    </EntitySet>
  </EntityContainer>
</Schema>";
            return GetEdmModel(template);
        }

        private static IEdmModel GetNavPropModel(string annotation)
        {
            string template = $@"<?xml version=""1.0"" encoding=""utf-16""?>
<Schema Namespace=""NS"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
  <EntityType Name=""Root"">
    <Key>
      <PropertyRef Name=""ID"" />
    </Key>
    <Property Name=""ID"" Type=""Edm.Int32"" Nullable=""false"" />
    <NavigationProperty Name=""Customers"" Type=""Collection(NS.Customer)"" ContainsTarget=""true"">
      {annotation}
    </NavigationProperty>
  </EntityType>
  <EntityType Name=""Customer"">
    <Key>
      <PropertyRef Name=""ID"" />
    </Key>
    <Property Name=""ID"" Type=""Edm.Int32"" Nullable=""false"" />
  </EntityType>
  <EntityType Name=""NiceCustomer"" BaseType=""NS.Customer"">
    <Property Name=""Other"" Type=""Edm.Int32"" Nullable=""true"" />
  </EntityType>
  <Action Name=""Ack"" IsBound=""true"" >
    <Parameter Name = ""bindingParameter"" Type=""NS.NiceCustomer"" />
  </Action>
  <EntityContainer Name =""Default"">
    <Singleton Name=""Root"" Type=""NS.Root"" />
  </EntityContainer>
</Schema>";
            return GetEdmModel(template);
        }

        private static IEdmModel GetEdmModel(string schema)
        {
            bool parsed = SchemaReader.TryParse(new XmlReader[] { XmlReader.Create(new StringReader(schema)) }, out IEdmModel parsedModel, out IEnumerable<EdmError> errors);
            Assert.True(parsed, $"Parse failure. {string.Join(Environment.NewLine, errors.Select(e => e.ToString()))}");
            return parsedModel;
        }

        private static IEdmModel GetEdmModel(bool hasStream, string streamPropName)
        {
            string template = @"<edmx:Edmx Version=""4.0"" xmlns:edmx=""http://docs.oasis-open.org/odata/ns/edmx"">
  <edmx:DataServices>
    <Schema Namespace=""microsoft.graph"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
      <EntityType Name=""todo"" HasStream=""{0}"">
        <Key>
          <PropertyRef Name=""id"" />
        </Key>
        <Property Name=""id"" Type=""Edm.Int32"" Nullable=""false"" />
        <Property Name=""{1}"" Type=""Edm.Stream""/>
        <Property Name = ""description"" Type = ""Edm.String"" />
      </EntityType>
      <EntityType Name=""user"" OpenType=""true"">
        <NavigationProperty Name = ""photo"" Type = ""microsoft.graph.profilePhoto"" ContainsTarget = ""true"" />
      </EntityType>
      <EntityType Name=""profilePhoto"" HasStream=""true"">
        <Property Name = ""height"" Type = ""Edm.Int32"" />
        <Property Name = ""width"" Type = ""Edm.Int32"" />
      </EntityType >
      <EntityType Name=""document"">
        <Property Name=""content"" Type=""Edm.Stream""/>
        <Property Name=""thumbnailPhoto"" Type=""Edm.Stream""/>
      </EntityType>
      <EntityType Name=""catalog"" BaseType=""microsoft.graph.document"">
        <NavigationProperty Name=""reports"" Type = ""Collection(microsoft.graph.report)"" />
      </EntityType>
      <EntityType Name=""report"">
        <Key>
          <PropertyRef Name=""id"" />
        </Key>
        <Property Name=""id"" Type=""Edm.Int32"" Nullable=""false"" />
      </EntityType>
      <EntityContainer Name =""GraphService"">
        <EntitySet Name=""todos"" EntityType=""microsoft.graph.todo"" />
        <Singleton Name=""me"" Type=""microsoft.graph.user"" />
        <Singleton Name=""catalog"" Type=""microsoft.graph.catalog"" />
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
