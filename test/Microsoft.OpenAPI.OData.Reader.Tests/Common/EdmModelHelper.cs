﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Xml.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OData.Edm.Vocabularies.V1;
using Xunit;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Text;
using Microsoft.OData.Edm.Validation;
using Xunit.Abstractions;

namespace Microsoft.OpenApi.OData.Tests
{
    /// <summary>
    /// Edm model helpers
    /// </summary>
    public class EdmModelHelper(ITestOutputHelper output)
    {
        public static IEdmModel EmptyModel { get; } = new EdmModel();

        public static IEdmModel MultipleInheritanceEdmModel { get; } = CreateMultipleInheritanceEdmModel();

        public static IEdmModel InheritanceEdmModelAcrossReferences { get; } = CreateInheritanceEdmModelAcrossReferences();

        public static IEdmModel BasicEdmModel { get; } = CreateEdmModel();

        public static IEdmModel MultipleSchemasEdmModel { get; } = LoadEdmModel("Multiple.Schema.OData.xml");

        public static IEdmModel CompositeKeyModel { get; } = CreateCompositeKeyModel();

        public static IEdmModel TripServiceModel { get; } = LoadEdmModel("TripService.OData.xml");

        public static IEdmModel ContractServiceModel { get; } = LoadEdmModel("Contract.OData.xml");

        public static IEdmModel GraphBetaModel { get; } = LoadEdmModel("Graph.Beta.OData.xml");

        public static IEdmModel ComposableFunctionsModel { get; } = LoadEdmModel("ComposableFunctions.OData.xml");

        private static IEdmModel LoadEdmModel(string source)
        {
            string csdl = Resources.GetString(source);
            return CsdlReader.Parse(XElement.Parse(csdl).CreateReader());
        }

        private static EdmModel CreateMultipleInheritanceEdmModel()
        {
            var model = new EdmModel();

            // enum type
            var colorEnumType = new EdmEnumType("NS", "Color");
            colorEnumType.AddMember("Blue", new EdmEnumMemberValue(0));
            colorEnumType.AddMember("While", new EdmEnumMemberValue(1));
            colorEnumType.AddMember("Red", new EdmEnumMemberValue(2));
            colorEnumType.AddMember("Yellow", new EdmEnumMemberValue(3));
            model.AddElement(colorEnumType);

            var oceanEnumType = new EdmEnumType("NS", "Ocean");
            oceanEnumType.AddMember("Atlantic", new EdmEnumMemberValue(0));
            oceanEnumType.AddMember("Pacific", new EdmEnumMemberValue(1));
            oceanEnumType.AddMember("India", new EdmEnumMemberValue(2));
            oceanEnumType.AddMember("Arctic", new EdmEnumMemberValue(3));
            model.AddElement(oceanEnumType);

            var continentEnumType = new EdmEnumType("NS", "Continent");
            continentEnumType.AddMember("Asia", new EdmEnumMemberValue(0));
            continentEnumType.AddMember("Europe", new EdmEnumMemberValue(1));
            continentEnumType.AddMember("Antarctica", new EdmEnumMemberValue(2));
            model.AddElement(continentEnumType);

            // top level entity type
            var zoo = new EdmEntityType("NS", "Zoo");
            var zooId = zoo.AddStructuralProperty("Id", EdmCoreModel.Instance.GetInt32(false));
            zoo.AddKeys(zooId);
            model.AddElement(zoo);

            // abstract entity type "Creature"
            var creature = new EdmEntityType("NS", "Creature", null, true, true);
            creature.AddKeys(creature.AddStructuralProperty("Id", EdmCoreModel.Instance.GetInt32(false)));
            model.AddElement(creature);

            var animal = new EdmEntityType("NS", "Animal", creature, true, true);
            animal.AddStructuralProperty("Age", EdmCoreModel.Instance.GetInt32(false));
            model.AddElement(animal);

            var human = new EdmEntityType("NS", "Human", animal, false, true);
            human.AddStructuralProperty("Name", EdmCoreModel.Instance.GetString(false));
            model.AddElement(human);

            var horse = new EdmEntityType("NS", "Horse", animal, false, true);
            horse.AddStructuralProperty("Height", EdmCoreModel.Instance.GetDecimal(false));
            model.AddElement(horse);

            EdmNavigationPropertyInfo navInfo = new()
            {
                Name = "Creatures",
                Target = creature,
                TargetMultiplicity = EdmMultiplicity.Many,
            };
            zoo.AddUnidirectionalNavigation(navInfo);

            // complex type
            var plant = new EdmComplexType("NS", "Plant", null, true, true);
            plant.AddStructuralProperty("Color", new EdmEnumTypeReference(colorEnumType, isNullable: false));
            model.AddElement(plant);

            // ocean plant
            var oceanPlant = new EdmComplexType("NS", "OceanPlant", plant, true, true);
            oceanPlant.AddStructuralProperty("Ocean", new EdmEnumTypeReference(oceanEnumType, isNullable: false));
            model.AddElement(oceanPlant);

            var kelp = new EdmComplexType("NS", "Kelp", oceanPlant, false, true);
            kelp.AddStructuralProperty("Length", EdmCoreModel.Instance.GetDouble(false));
            model.AddElement(kelp);

            // land plant
            var landPlant = new EdmComplexType("NS", "LandPlant", plant, true, true);
            landPlant.AddStructuralProperty("Continent", new EdmEnumTypeReference(continentEnumType, isNullable: false));
            landPlant.AddStructuralProperty("Name", EdmCoreModel.Instance.GetString(false));
            model.AddElement(landPlant);

            var tree = new EdmComplexType("NS", "Tree", landPlant, false, true);
            tree.AddStructuralProperty("Price", EdmCoreModel.Instance.GetDecimal(false));
            model.AddElement(tree);

            var flower = new EdmComplexType("NS", "Flower", landPlant, false, true);
            flower.AddStructuralProperty("Height", EdmCoreModel.Instance.GetDouble(false));
            model.AddElement(flower);

            // address
            var address = new EdmComplexType("NS", "Address");
            address.AddStructuralProperty("Street", EdmCoreModel.Instance.GetString(true));
            address.AddStructuralProperty("City", EdmCoreModel.Instance.GetString(true));
            model.AddElement(address);

            var coreDescription = CoreVocabularyModel.DescriptionTerm;
            var annotation = new EdmVocabularyAnnotation(address, coreDescription, new EdmStringConstant("Complex type 'Address' description."));
            model.AddVocabularyAnnotation(annotation);

            annotation = new EdmVocabularyAnnotation(tree, coreDescription, new EdmStringConstant("Complex type 'Tree' description."));
            model.AddVocabularyAnnotation(annotation);

            annotation = new EdmVocabularyAnnotation(zoo, coreDescription, new EdmStringConstant("Entity type 'Zoo' description."));
            model.AddVocabularyAnnotation(annotation);

            annotation = new EdmVocabularyAnnotation(human, coreDescription, new EdmStringConstant("Entity type 'Human' description."));
            model.AddVocabularyAnnotation(annotation);

            return model;
        }

        public static IEdmModel CreateInheritanceEdmModelAcrossReferences()
        {
            const string modelText =
                @"<edmx:Edmx Version=""4.0"" xmlns:edmx=""http://docs.oasis-open.org/odata/ns/edmx"">
  <edmx:Reference Uri=""subModel.csdl"">
    <edmx:Include Alias=""subModel"" Namespace=""SubNS"" />
  </edmx:Reference>
  <edmx:DataServices>
    <Schema Namespace=""NS"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
      <EntityType Name=""Customer"" BaseType=""SubNS.CustomerBase"">
        <Property Name=""Extra"" Type=""Edm.Int32"" Nullable=""false"" />
      </EntityType>
      <EntityContainer Name =""Default"">
         <EntitySet Name=""Customers"" EntityType=""NS.Customer"" />
      </EntityContainer>
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>";
            const string subModelText =
                @"<edmx:Edmx Version=""4.0"" xmlns:edmx=""http://docs.oasis-open.org/odata/ns/edmx"">
  <edmx:DataServices>
    <Schema Namespace=""SubNS"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
      <EntityType Name=""CustomerBase"">
        <Key>
          <PropertyRef Name=""ID"" />
        </Key>
        <Property Name=""ID"" Type=""Edm.Int32"" Nullable=""false"" />
      </EntityType>
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>";


            XElement parsed = XElement.Parse(modelText);
            bool result = CsdlReader.TryParse(parsed.CreateReader(),
                uri => XElement.Parse(subModelText).CreateReader(),
                out var model,
                out var _);
            Assert.True(result);
            return model;
        }

        private static EdmModel CreateEdmModel()
        {
            var model = new EdmModel();

            var coreDescription = CoreVocabularyModel.DescriptionTerm;

            var enumType = new EdmEnumType("DefaultNs", "Color");
            var blue = enumType.AddMember("Blue", new EdmEnumMemberValue(0));
            enumType.AddMember("White", new EdmEnumMemberValue(1));
            model.AddElement(enumType);

            var annotation = new EdmVocabularyAnnotation(enumType, coreDescription, new EdmStringConstant("Enum type 'Color' description."));
            model.AddVocabularyAnnotation(annotation);

            var person = new EdmEntityType("DefaultNs", "Person");
            var entityId = person.AddStructuralProperty("UserName", EdmCoreModel.Instance.GetString(false));
            person.AddKeys(entityId);

            var city = new EdmEntityType("DefaultNs", "City");
            var cityId = city.AddStructuralProperty("Name", EdmCoreModel.Instance.GetString(false));
            city.AddKeys(cityId);

            var countryOrRegion = new EdmEntityType("DefaultNs", "CountryOrRegion");
            var countryId = countryOrRegion.AddStructuralProperty("Name", EdmCoreModel.Instance.GetString(false));
            countryOrRegion.AddKeys(countryId);

            var complex = new EdmComplexType("DefaultNs", "Address");
            complex.AddStructuralProperty("Id", EdmCoreModel.Instance.GetInt32(false));
            var navP = complex.AddUnidirectionalNavigation(
                new EdmNavigationPropertyInfo()
                {
                    Name = "City",
                    Target = city,
                    TargetMultiplicity = EdmMultiplicity.One,
                });

            var derivedComplex = new EdmComplexType("DefaultNs", "WorkAddress", complex);
            var navP2 = derivedComplex.AddUnidirectionalNavigation(
                new EdmNavigationPropertyInfo()
                {
                    Name = "CountryOrRegion",
                    Target = countryOrRegion,
                    TargetMultiplicity = EdmMultiplicity.One,
                });

            person.AddStructuralProperty("HomeAddress", new EdmComplexTypeReference(complex, false));
            person.AddStructuralProperty("WorkAddress", new EdmComplexTypeReference(complex, false));
            person.AddStructuralProperty("Addresses",
                new EdmCollectionTypeReference(new EdmCollectionType(new EdmComplexTypeReference(complex, false))));

            model.AddElement(person);
            model.AddElement(city);
            model.AddElement(countryOrRegion);
            model.AddElement(complex);
            model.AddElement(derivedComplex);

            var entityContainer = new EdmEntityContainer("DefaultNs", "Container");
            model.AddElement(entityContainer);
            EdmSingleton me = new(entityContainer, "Me", person);
            EdmEntitySet people = new(entityContainer, "People", person);
            EdmEntitySet cities = new(entityContainer, "City", city);
            EdmEntitySet countriesOrRegions = new(entityContainer, "CountryOrRegion", countryOrRegion);
            people.AddNavigationTarget(navP, cities, new EdmPathExpression("HomeAddress/City"));
            people.AddNavigationTarget(navP, cities, new EdmPathExpression("Addresses/City"));
            people.AddNavigationTarget(navP2, countriesOrRegions,
                new EdmPathExpression("WorkAddress/DefaultNs.WorkAddress/CountryOrRegion"));
            entityContainer.AddElement(people);
            entityContainer.AddElement(cities);
            entityContainer.AddElement(countriesOrRegions);
            entityContainer.AddElement(me);

            annotation = new EdmVocabularyAnnotation(people, coreDescription, new EdmStringConstant("People's description."));
            model.AddVocabularyAnnotation(annotation);

            var coreLongDescription = CoreVocabularyModel.LongDescriptionTerm;
            annotation = new EdmVocabularyAnnotation(people, coreLongDescription, new EdmStringConstant("People's Long description."));
            model.AddVocabularyAnnotation(annotation);

            return model;
        }

        private static EdmModel CreateCompositeKeyModel()
        {
            var model = new EdmModel();

            var customer = new EdmEntityType("NS", "Customer", null, false, true);
            var customerId = customer.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32, false);
            var customerName = customer.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String, true);
            customer.AddKeys(customerId, customerName);
            model.AddElement(customer);

            var container = new EdmEntityContainer("NS", "Container");
            container.AddEntitySet("Customers", customer);
            model.AddElement(container);
            return model;
        }

        private readonly ITestOutputHelper _output = output;

        [Fact]
        public void MultipleInheritanceEdmModelMetadataDocumentTest()
        {
            // Arrange
            string expected = @"<?xml version=""1.0"" encoding=""utf-16""?>
<edmx:Edmx Version=""4.0"" xmlns:edmx=""http://docs.oasis-open.org/odata/ns/edmx"">
  <edmx:DataServices>
    <Schema Namespace=""NS"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
      <EnumType Name=""Color"">
        <Member Name=""Blue"" Value=""0"" />
        <Member Name=""While"" Value=""1"" />
        <Member Name=""Red"" Value=""2"" />
        <Member Name=""Yellow"" Value=""3"" />
      </EnumType>
      <EnumType Name=""Ocean"">
        <Member Name=""Atlantic"" Value=""0"" />
        <Member Name=""Pacific"" Value=""1"" />
        <Member Name=""India"" Value=""2"" />
        <Member Name=""Arctic"" Value=""3"" />
      </EnumType>
      <EnumType Name=""Continent"">
        <Member Name=""Asia"" Value=""0"" />
        <Member Name=""Europe"" Value=""1"" />
        <Member Name=""Antarctica"" Value=""2"" />
      </EnumType>
      <EntityType Name=""Zoo"">
        <Key>
          <PropertyRef Name=""Id"" />
        </Key>
        <Property Name=""Id"" Type=""Edm.Int32"" Nullable=""false"" />
        <NavigationProperty Name=""Creatures"" Type=""Collection(NS.Creature)"" />
      </EntityType>
      <EntityType Name=""Creature"" Abstract=""true"" OpenType=""true"">
        <Key>
          <PropertyRef Name=""Id"" />
        </Key>
        <Property Name=""Id"" Type=""Edm.Int32"" Nullable=""false"" />
      </EntityType>
      <EntityType Name=""Animal"" BaseType=""NS.Creature"" Abstract=""true"" OpenType=""true"">
        <Property Name=""Age"" Type=""Edm.Int32"" Nullable=""false"" />
      </EntityType>
      <EntityType Name=""Human"" BaseType=""NS.Animal"" OpenType=""true"">
        <Property Name=""Name"" Type=""Edm.String"" Nullable=""false"" />
      </EntityType>
      <EntityType Name=""Horse"" BaseType=""NS.Animal"" OpenType=""true"">
        <Property Name=""Height"" Type=""Edm.Decimal"" Nullable=""false"" Scale=""variable"" />
      </EntityType>
      <ComplexType Name=""Plant"" Abstract=""true"" OpenType=""true"">
        <Property Name=""Color"" Type=""NS.Color"" Nullable=""false"" />
      </ComplexType>
      <ComplexType Name=""OceanPlant"" BaseType=""NS.Plant"" Abstract=""true"" OpenType=""true"">
        <Property Name=""Ocean"" Type=""NS.Ocean"" Nullable=""false"" />
      </ComplexType>
      <ComplexType Name=""Kelp"" BaseType=""NS.OceanPlant"" OpenType=""true"">
        <Property Name=""Length"" Type=""Edm.Double"" Nullable=""false"" />
      </ComplexType>
      <ComplexType Name=""LandPlant"" BaseType=""NS.Plant"" Abstract=""true"" OpenType=""true"">
        <Property Name=""Continent"" Type=""NS.Continent"" Nullable=""false"" />
        <Property Name=""Name"" Type=""Edm.String"" Nullable=""false"" />
      </ComplexType>
      <ComplexType Name=""Tree"" BaseType=""NS.LandPlant"" OpenType=""true"">
        <Property Name=""Price"" Type=""Edm.Decimal"" Nullable=""false"" Scale=""variable"" />
      </ComplexType>
      <ComplexType Name=""Flower"" BaseType=""NS.LandPlant"" OpenType=""true"">
        <Property Name=""Height"" Type=""Edm.Double"" Nullable=""false"" />
      </ComplexType>
      <ComplexType Name=""Address"">
        <Property Name=""Street"" Type=""Edm.String"" />
        <Property Name=""City"" Type=""Edm.String"" />
      </ComplexType>
      <Annotations Target=""NS.Address"">
        <Annotation Term=""Org.OData.Core.V1.Description"" String=""Complex type 'Address' description."" />
      </Annotations>
      <Annotations Target=""NS.Tree"">
        <Annotation Term=""Org.OData.Core.V1.Description"" String=""Complex type 'Tree' description."" />
      </Annotations>
      <Annotations Target=""NS.Zoo"">
        <Annotation Term=""Org.OData.Core.V1.Description"" String=""Entity type 'Zoo' description."" />
      </Annotations>
      <Annotations Target=""NS.Human"">
        <Annotation Term=""Org.OData.Core.V1.Description"" String=""Entity type 'Human' description."" />
      </Annotations>
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>";

            // Act
            string actual = GetCsdl(MultipleInheritanceEdmModel);
            _output.WriteLine(actual);

            // Assert
            Assert.Equal(expected, actual);
        }

        public static string GetCsdl(IEdmModel model)
        {
            using StringWriter sw = new();
            XmlWriterSettings settings = new()
            {
                Encoding = Encoding.UTF8,
                Indent = true
            };

            using XmlWriter xw = XmlWriter.Create(sw, settings);
            CsdlWriter.TryWriteCsdl(model, xw, CsdlTarget.OData, out var _);
            xw.Flush();

            string edmx = sw.ToString();
            return edmx;
        }
    }
}
