// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Linq;
using System.Xml.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Annotations;
using Xunit;

namespace Microsoft.OpenApi.OData.Reader.Annotations.Tests
{
    public class HttpRequestTests
    {
#if false
        [Fact]
        public void CtorThrowArgumentNullModel()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>("model", () => new HttpRequestProvider(model: null));
        }

        [Fact]
        public void GetHttpRequestThrowArgumentNullTarget()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>("target",
                () => new HttpRequestProvider(EdmCoreModel.Instance).GetHttpRequest(target: null, method: "GET"));
        }

        [Fact]
        public void GetHttpRequestThrowArgumentNullMethod()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>("method",
                () => new HttpRequestProvider(EdmCoreModel.Instance).GetHttpRequest(new EdmEntityContainer("NS", "Default"), method: null));
        }

        [Theory]
        [InlineData("Customers")]
        [InlineData("Me")]
        public void GetHttpRequestsReturnsNullForTargetWithoutAnnotations(string name)
        {
            // Arrange
            IEdmModel model = GetEdmModel("", "");
            var httpRequestProvider = new HttpRequestProvider(model);
            IEdmNavigationSource navigationSource = model.FindDeclaredNavigationSource(name);
            Assert.NotNull(navigationSource);

            // Act & Assert
            var requests = httpRequestProvider.GetHttpRequests(navigationSource as IEdmVocabularyAnnotatable);

            // Assert
            Assert.Null(requests);
        }

        [Theory]
        [InlineData("Customers")]
        [InlineData("Me")]
        public void GetHttpRequestsReturnsForEdmModelNavigationSourceWithAnnotations(string name)
        {
            // Arrange
            string annotation = @"
          <Annotation Term=""Org.OData.Core.V1.HttpRequests"">
            <Collection>
              <Record>
                <PropertyValue Property=""Description"" />
                <PropertyValue Property=""MethodDescription"" String=""Example"" />
                <PropertyValue Property=""MethodType"" String=""DELETE"" />
                <PropertyValue Property=""CustomHeaders"">
                  <Collection>
                    <Record>
                      <PropertyValue Property=""Name"" String=""Authorization"" />
                      <PropertyValue Property=""Description"" String=""String"" />
                      <PropertyValue Property=""Required"" Bool=""false"" />
                    </Record>
                  </Collection>
                </PropertyValue>
                <PropertyValue Property=""CustomQueryOptions"">
                  <Collection />
                </PropertyValue>
                <PropertyValue Property=""HttpResponses"">
                  <Collection>
                    <Record>
                      <PropertyValue Property=""ResponseCode"" String=""204"" />
                      <PropertyValue Property=""Examples"">
                        <Record Type=""Org.OData.Core.V1.InlineExample"">
                          <PropertyValue Property=""InlineExample"" />
                          <PropertyValue Property=""Description"" />
                        </Record>
                      </PropertyValue>
                    </Record>
                  </Collection>
                </PropertyValue>
                <PropertyValue Property=""SecuritySchemes"">
                  <Collection>
                    <Record>
                      <PropertyValue Property=""Authorization"" String=""Delegated (work or school account)"" />
                      <PropertyValue Property=""RequiredScopes"">
                        <Collection>
                          <String>Directory.AccessAsUser.All</String>
                        </Collection>
                      </PropertyValue>
                    </Record>
                  </Collection>
                </PropertyValue>
              </Record>
              <Record>
                <PropertyValue Property=""Description"" />
                <PropertyValue Property=""MethodDescription"" String=""Example"" />
                <PropertyValue Property=""MethodType"" String=""GET"" />
                <PropertyValue Property=""CustomHeaders"">
                  <Collection>
                    <Record>
                      <PropertyValue Property=""Name"" String=""Authorization"" />
                      <PropertyValue Property=""Description"" String=""String"" />
                      <PropertyValue Property=""Required"" Bool=""false"" />
                    </Record>
                  </Collection>
                </PropertyValue>
                <PropertyValue Property=""CustomQueryOptions"">
                  <Collection />
                </PropertyValue>
                <PropertyValue Property=""HttpResponses"">
                  <Collection>
                    <Record>
                      <PropertyValue Property=""ResponseCode"" String=""200"" />
                      <PropertyValue Property=""Examples"">
                        <Record Type=""Org.OData.Core.V1.InlineExample"">
                          <PropertyValue Property=""InlineExample"" String=""{&#xD;&#xA;  &quot;accountEnabled&quot;:false,&#xD;&#xA;  &quot;deviceId&quot;:&quot;4c299165-6e8f-4b45-a5ba-c5d250a707ff&quot;,&#xD;&#xA;  &quot;displayName&quot;:&quot;Test device&quot;,&#xD;&#xA;  &quot;id&quot;: &quot;id-value&quot;,&#xD;&#xA;  &quot;operatingSystem&quot;:&quot;linux&quot;,&#xD;&#xA;  &quot;operatingSystemVersion&quot;:&quot;1&quot;&#xD;&#xA;}&#xA;"" />
                          <PropertyValue Property=""Description"" String=""application/json"" />
                        </Record>
                      </PropertyValue>
                    </Record>
                  </Collection>
                </PropertyValue>
                <PropertyValue Property=""SecuritySchemes"">
                  <Collection>
                    <Record>
                      <PropertyValue Property=""Authorization"" String=""Delegated (work or school account)"" />
                      <PropertyValue Property=""RequiredScopes"">
                        <Collection>
                          <String>Directory.Read.All</String>
                          <String>Directory.ReadWrite.All</String>
                          <String>Directory.AccessAsUser.All</String>
                        </Collection>
                      </PropertyValue>
                    </Record>
                    <Record>
                      <PropertyValue Property=""Authorization"" String=""Application"" />
                      <PropertyValue Property=""RequiredScopes"">
                        <Collection>
                          <String>Device.ReadWrite.All</String>
                          <String>Directory.Read.All</String>
                          <String>Directory.ReadWrite.All</String>
                        </Collection>
                      </PropertyValue>
                    </Record>
                  </Collection>
                </PropertyValue>
              </Record>
              <Record>
                <PropertyValue Property=""Description"" />
                <PropertyValue Property=""MethodDescription"" String=""PATCH Example"" />
                <PropertyValue Property=""MethodType"" String=""PATCH"" />
                <PropertyValue Property=""CustomHeaders"">
                  <Collection>
                    <Record>
                      <PropertyValue Property=""Name"" String=""Authorization"" />
                      <PropertyValue Property=""Description"" String=""String"" />
                      <PropertyValue Property=""Required"" Bool=""false"" />
                    </Record>
                  </Collection>
                </PropertyValue>
                <PropertyValue Property=""CustomQueryOptions"">
                  <Collection />
                </PropertyValue>
                <PropertyValue Property=""HttpResponses"">
                  <Collection>
                    <Record>
                      <PropertyValue Property=""ResponseCode"" String=""204"" />
                      <PropertyValue Property=""Examples"">
                        <Record Type=""Org.OData.Core.V1.InlineExample"">
                          <PropertyValue Property=""InlineExample"" />
                          <PropertyValue Property=""Description"" />
                        </Record>
                      </PropertyValue>
                    </Record>
                  </Collection>
                </PropertyValue>
                <PropertyValue Property=""SecuritySchemes"">
                  <Collection>
                    <Record>
                      <PropertyValue Property=""Authorization"" String=""Delegated (work or school account)"" />
                      <PropertyValue Property=""RequiredScopes"">
                        <Collection>
                          <String>Directory.ReadWrite.All</String>
                          <String>Directory.AccessAsUser.All</String>
                        </Collection>
                      </PropertyValue>
                    </Record>
                  </Collection>
                </PropertyValue>
              </Record>
            </Collection>
          </Annotation>";

            IEdmModel model = GetEdmModel(annotation, annotation);
            var httpRequestProvider = new HttpRequestProvider(model);

            IEdmNavigationSource navigationSource = model.FindDeclaredNavigationSource(name);
            Assert.NotNull(navigationSource);

            // Act
            var requests = httpRequestProvider.GetHttpRequests(navigationSource as IEdmVocabularyAnnotatable);

            // Assert
            Assert.NotEmpty(requests);
            Assert.Equal(3, requests.Count());

            // Act
            var anotherRequests = httpRequestProvider.GetHttpRequests(navigationSource as IEdmVocabularyAnnotatable);

            // Assert
            Assert.True(ReferenceEquals(requests, anotherRequests));

            // Act (PATCH)
            var request = httpRequestProvider.GetHttpRequest(navigationSource as IEdmVocabularyAnnotatable, "PATCH");

            // Assert
            Assert.NotNull(request);
            Assert.Equal("PATCH Example", request.MethodDescription);
            Assert.NotNull(request.SecuritySchemes);
            var securityScheme = Assert.Single(request.SecuritySchemes);
            Assert.Equal("Delegated (work or school account)", securityScheme.Authorization);
            Assert.NotNull(securityScheme.RequiredScopes);
            Assert.Equal(new[] { "Directory.ReadWrite.All", "Directory.AccessAsUser.All" }, securityScheme.RequiredScopes);
        }

        private static IEdmModel GetEdmModel(string entitySetAnnotation, string singletonAnnotation)
        {
            string template = @"<?xml version=""1.0"" encoding=""utf-16""?>
<edmx:Edmx xmlns:ags=""http://aggregator.microsoft.com/internal"" Version=""4.0"" xmlns:edmx=""http://docs.oasis-open.org/odata/ns/edmx"">
  <edmx:DataServices>
    <Schema Namespace=""NS"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
      <EntityType Name=""Customer"">
        <Key>
          <PropertyRef Name=""Id"" />
        </Key>
        <Property Name=""Id"" Type=""Edm.Int32"" Nullable=""false"" />
      </EntityType>
      <EntityContainer Name=""Container"">
        <EntitySet Name=""Customers"" EntityType=""NS.Customer"">
          {0}
        </EntitySet>
        <Singleton Name=""Me"" Type=""NS.Customer"" >
          {1}
        </Singleton>
      </EntityContainer>
    </Schema>
    <Schema Namespace=""Org.OData.Core.V1"" Alias=""Core"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
      <Term Name=""HttpRequests"" Type=""Collection(Core.HttpRequest)"" AppliesTo=""EntitySet Singleton ActionImport FunctionImport Action Function"">
        <Annotation Term=""Core.Description"" String=""Describes possible HTTP requests"" />
        <Annotation Term=""Core.LongDescription"" String=""The list need not be complete. It may be used to generate API documentation, so restricting it to the most common and most important responses may increase readability."" />
      </Term>
      <ComplexType Name=""HttpRequest"">
        <Property Name=""Description"" Type=""Edm.String"" />
        <!-- text such as ""For a specific user:"" to describe the http request-->
        <Property Name=""Method"" Type=""Edm.String"" />
        <Property Name=""CustomQueryOptions"" Type=""Collection(Core.CustomParameter)"" />
        <Property Name=""CustomHeaders"" Type=""Collection(Core.CustomParameter)"" />    <!-- Map to Parameter in Operation as Header object.-->
        <Property Name=""HttpResponses"" Type=""Collection(Core.HttpResponse)"" />    <!--  Map to Response object in Operation.     -->
        <Property Name=""SecuritySchemes"" Type=""Collection(Auth.SecurityScheme)""/>
      </ComplexType>
      <ComplexType Name=""HttpResponse"">
        <Property Name=""ResponseCode"" Type=""Edm.String"" />
        <Property Name=""Examples"" Type=""Collection(Core.Example)"" />
        <Property Name=""Description"" Type=""Edm.String"" />
      </ComplexType>
      <ComplexType Name=""CustomParameter"">
        <Property Name=""Name"" Type=""Edm.String"" Nullable=""false"" />
        <Property Name=""Description"" Type=""Edm.String"" />
        <Property Name=""DocumentationURL"" Type=""Edm.String"" />
        <Property Name=""Required"" Type=""Edm.Boolean"" Nullable=""false"" />
        <Property Name=""ExampleValues"" Type=""Collection(Core.Example)"" Nullable=""false"" />
      </ComplexType>
      <ComplexType Name=""Example"" Abstract=""true"">
        <Property Name=""Description"" Type=""Edm.String"" Nullable=""false"" />
      </ComplexType>
      <ComplexType Name=""ExternalExample"" BaseType=""Core.Example"">
        <Property Name=""ExternalValue"" Type=""Edm.String"" Nullable=""false"" />
      </ComplexType>
      <ComplexType Name=""InlineExample"" BaseType=""Core.Example"">
        <Property Name=""InlineValue"" Type=""Edm.String"" Nullable=""false"" />
      </ComplexType>
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>
";
            string modelText = string.Format(template, entitySetAnnotation, singletonAnnotation);

            return CsdlReader.Parse(XElement.Parse(modelText).CreateReader());
        }
#endif
    }
}
