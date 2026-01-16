// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OData.Edm.Vocabularies.V1;
using Xunit;

namespace Microsoft.OpenApi.OData.Tests
{
    /// <summary>
    /// Tests for EdmModelOpenApiExtensions.ShouldRequestBodyBeRequired extension methods.
    /// </summary>
    public class EdmModelOpenApiExtensionsTests
    {
        #region Action Tests

        [Fact]
        public void ActionWithAllNullableParameters_ReturnsOptional()
        {
            // Arrange
            var action = CreateAction("TestAction", isNullable: true);

            // Act
            var result = action.ShouldRequestBodyBeRequired();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ActionWithAllRequiredParameters_ReturnsRequired()
        {
            // Arrange
            var action = CreateAction("TestAction", isNullable: false);

            // Act
            var result = action.ShouldRequestBodyBeRequired();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ActionWithMixedNullableAndRequiredParameters_ReturnsRequired()
        {
            // Arrange
            var model = new EdmModel();
            var action = new EdmAction("NS", "TestAction", null);
            action.AddParameter("nullableParam", EdmCoreModel.Instance.GetString(true));
            action.AddParameter("requiredParam", EdmCoreModel.Instance.GetString(false));
            model.AddElement(action);

            // Act
            var result = action.ShouldRequestBodyBeRequired();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ActionWithOptionalParameter_ReturnsOptional()
        {
            // Arrange
            var model = new EdmModel();
            var action = new EdmAction("NS", "TestAction", null);
            action.AddOptionalParameter("optionalParam", EdmCoreModel.Instance.GetString(false));
            model.AddElement(action);

            // Act
            var result = action.ShouldRequestBodyBeRequired();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void BoundActionWithNullableParameter_ExcludesBindingParameter()
        {
            // Arrange
            var model = new EdmModel();
            var entityType = new EdmEntityType("NS", "Entity");
            var action = new EdmAction("NS", "TestAction", null, true, null);
            action.AddParameter("bindingParam", new EdmEntityTypeReference(entityType, false));
            action.AddParameter("nullableParam", EdmCoreModel.Instance.GetString(true));
            model.AddElement(entityType);
            model.AddElement(action);

            // Act
            var result = action.ShouldRequestBodyBeRequired();

            // Assert
            Assert.False(result); // Only non-binding parameter is nullable
        }

        [Fact]
        public void ActionWithNoParameters_ReturnsRequired()
        {
            // Arrange
            var model = new EdmModel();
            var action = new EdmAction("NS", "TestAction", null);
            model.AddElement(action);

            // Act
            var result = action.ShouldRequestBodyBeRequired();

            // Assert
            Assert.True(result); // No parameters means no request body needed, but returns true (existing behavior)
        }

        [Fact]
        public void NullAction_ReturnsRequired()
        {
            // Act
            var result = ((IEdmAction)null).ShouldRequestBodyBeRequired();

            // Assert
            Assert.True(result); // Safe default
        }

        #endregion

        #region Entity Type Tests - Create Operations

        [Fact]
        public void EntityTypeWithAllNullableProperties_CreateOperation_ReturnsOptional()
        {
            // Arrange
            var entityType = CreateEntityType("Customer", hasRequiredProperties: false);

            // Act
            var result = entityType.ShouldRequestBodyBeRequired(
                isUpdateOperation: false,
                model: null);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void EntityTypeWithRequiredProperty_CreateOperation_ReturnsRequired()
        {
            // Arrange
            var entityType = CreateEntityType("Customer", hasRequiredProperties: true);

            // Act
            var result = entityType.ShouldRequestBodyBeRequired(
                isUpdateOperation: false,
                model: null);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void EntityTypeWithOnlyKeyProperty_CreateOperation_ReturnsRequired()
        {
            // Arrange
            var entityType = new EdmEntityType("NS", "Entity");
            entityType.AddKeys(entityType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32, false));

            // Act - Create operation includes key properties
            var result = entityType.ShouldRequestBodyBeRequired(
                isUpdateOperation: false,
                model: null);

            // Assert
            Assert.True(result); // Key is not nullable, so body is required
        }

        #endregion

        #region Entity Type Tests - Update Operations

        [Fact]
        public void EntityTypeWithOnlyKeyProperty_UpdateOperation_ReturnsOptional()
        {
            // Arrange
            var entityType = new EdmEntityType("NS", "Entity");
            entityType.AddKeys(entityType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32, false));

            // Act - Update operation excludes key properties
            var result = entityType.ShouldRequestBodyBeRequired(
                isUpdateOperation: true,
                model: null);

            // Assert
            Assert.False(result); // No non-key properties, so body is optional
        }

        [Fact]
        public void EntityTypeWithRequiredNonKeyProperty_UpdateOperation_ReturnsRequired()
        {
            // Arrange
            var entityType = new EdmEntityType("NS", "Entity");
            entityType.AddKeys(entityType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32, false));
            entityType.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String, false); // Required

            // Act
            var result = entityType.ShouldRequestBodyBeRequired(
                isUpdateOperation: true,
                model: null);

            // Assert
            Assert.True(result); // Has required non-key property
        }

        [Fact]
        public void EntityTypeWithNullableNonKeyProperties_UpdateOperation_ReturnsOptional()
        {
            // Arrange
            var entityType = new EdmEntityType("NS", "Entity");
            entityType.AddKeys(entityType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32, false));
            entityType.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String, true); // Nullable
            entityType.AddStructuralProperty("Email", EdmPrimitiveTypeKind.String, true); // Nullable

            // Act
            var result = entityType.ShouldRequestBodyBeRequired(
                isUpdateOperation: true,
                model: null);

            // Assert
            Assert.False(result); // All non-key properties are nullable
        }

        #endregion

        #region Inheritance Tests

        [Fact]
        public void EntityTypeWithRequiredInheritedProperty_ReturnsRequired()
        {
            // Arrange
            var baseType = new EdmEntityType("NS", "BaseEntity");
            baseType.AddKeys(baseType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32, false));
            baseType.AddStructuralProperty("BaseProperty", EdmPrimitiveTypeKind.String, false); // Required

            var derivedType = new EdmEntityType("NS", "DerivedEntity", baseType);
            derivedType.AddStructuralProperty("DerivedProperty", EdmPrimitiveTypeKind.String, true); // Nullable

            // Act - Update operation
            var result = derivedType.ShouldRequestBodyBeRequired(
                isUpdateOperation: true,
                model: null);

            // Assert
            Assert.True(result); // Base type has required property
        }

        [Fact]
        public void EntityTypeWithAllNullableInheritedProperties_ReturnsOptional()
        {
            // Arrange
            var baseType = new EdmEntityType("NS", "BaseEntity");
            baseType.AddKeys(baseType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32, false));
            baseType.AddStructuralProperty("BaseProperty", EdmPrimitiveTypeKind.String, true); // Nullable

            var derivedType = new EdmEntityType("NS", "DerivedEntity", baseType);
            derivedType.AddStructuralProperty("DerivedProperty", EdmPrimitiveTypeKind.String, true); // Nullable

            // Act - Update operation
            var result = derivedType.ShouldRequestBodyBeRequired(
                isUpdateOperation: true,
                model: null);

            // Assert
            Assert.False(result); // All non-key properties are nullable
        }

        #endregion

        #region Property with Default Value Tests

        [Fact]
        public void PropertyWithDefaultValue_TreatedAsOptional()
        {
            // Arrange
            var entityType = new EdmEntityType("NS", "Entity");
            entityType.AddKeys(entityType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32, false));

            // Create property with default value
            var nameProperty = new EdmStructuralProperty(
                entityType,
                "Name",
                EdmCoreModel.Instance.GetString(false),
                "DefaultName"); // Default value
            entityType.AddProperty(nameProperty);

            // Act - Update operation
            var result = entityType.ShouldRequestBodyBeRequired(
                isUpdateOperation: true,
                model: null);

            // Assert
            Assert.False(result); // Has default value, so optional
        }

        #endregion

        #region Computed Property Tests

        [Fact]
        public void ComputedProperty_ExcludedFromAnalysis()
        {
            // Arrange
            var model = new EdmModel();
            var entityType = new EdmEntityType("NS", "Entity");
            entityType.AddKeys(entityType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32, false));
            var computedProp = entityType.AddStructuralProperty("ComputedProp", EdmPrimitiveTypeKind.String, false);
            model.AddElement(entityType);

            // Add Computed annotation
            var term = CoreVocabularyModel.ComputedTerm;
            var annotation = new EdmVocabularyAnnotation(computedProp, term, new EdmBooleanConstant(true));
            model.SetVocabularyAnnotation(annotation);

            // Act - Update operation
            var result = entityType.ShouldRequestBodyBeRequired(
                isUpdateOperation: true,
                model: model);

            // Assert
            Assert.False(result); // Computed property excluded, no other properties
        }

        #endregion

        #region Complex Type Tests

        [Fact]
        public void ComplexTypeWithAllNullableProperties_ReturnsOptional()
        {
            // Arrange
            var complexType = new EdmComplexType("NS", "Address");
            complexType.AddStructuralProperty("Street", EdmPrimitiveTypeKind.String, true);
            complexType.AddStructuralProperty("City", EdmPrimitiveTypeKind.String, true);

            // Act
            var result = complexType.ShouldRequestBodyBeRequired(
                isUpdateOperation: false,
                model: null);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ComplexTypeWithRequiredProperty_ReturnsRequired()
        {
            // Arrange
            var complexType = new EdmComplexType("NS", "Address");
            complexType.AddStructuralProperty("Street", EdmPrimitiveTypeKind.String, false); // Required
            complexType.AddStructuralProperty("City", EdmPrimitiveTypeKind.String, true);

            // Act
            var result = complexType.ShouldRequestBodyBeRequired(
                isUpdateOperation: false,
                model: null);

            // Assert
            Assert.True(result);
        }

        #endregion

        #region Navigation Property Tests

        [Fact]
        public void EntityTypeWithNullableNavigationProperty_ReturnsOptional()
        {
            // Arrange
            var model = new EdmModel();
            var entityType = new EdmEntityType("NS", "Order");
            entityType.AddKeys(entityType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32, false));

            var customerType = new EdmEntityType("NS", "Customer");
            customerType.AddKeys(customerType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32, false));

            var navProperty = entityType.AddUnidirectionalNavigation(
                new EdmNavigationPropertyInfo
                {
                    Name = "Customer",
                    Target = customerType,
                    TargetMultiplicity = EdmMultiplicity.ZeroOrOne
                });

            model.AddElement(entityType);
            model.AddElement(customerType);

            // Act - Update operation (excludes key)
            var result = entityType.ShouldRequestBodyBeRequired(
                isUpdateOperation: true,
                model: model);

            // Assert
            Assert.False(result); // Navigation property is nullable
        }

        [Fact]
        public void EntityTypeWithRequiredNavigationProperty_ReturnsRequired()
        {
            // Arrange
            var model = new EdmModel();
            var entityType = new EdmEntityType("NS", "Order");
            entityType.AddKeys(entityType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32, false));

            var customerType = new EdmEntityType("NS", "Customer");
            customerType.AddKeys(customerType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32, false));

            var navProperty = entityType.AddUnidirectionalNavigation(
                new EdmNavigationPropertyInfo
                {
                    Name = "Customer",
                    Target = customerType,
                    TargetMultiplicity = EdmMultiplicity.One // Required
                });

            model.AddElement(entityType);
            model.AddElement(customerType);

            // Act - Update operation (excludes key)
            var result = entityType.ShouldRequestBodyBeRequired(
                isUpdateOperation: true,
                model: model);

            // Assert
            Assert.True(result); // Navigation property is required
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void EmptyEntityType_ReturnsOptional()
        {
            // Arrange - Entity with only key, update operation excludes key
            var entityType = new EdmEntityType("NS", "Empty");
            entityType.AddKeys(entityType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32, false));

            // Act
            var result = entityType.ShouldRequestBodyBeRequired(
                isUpdateOperation: true,
                model: null);

            // Assert
            Assert.False(result); // No properties after excluding key
        }

        [Fact]
        public void NullStructuredType_ReturnsRequired()
        {
            // Act
            var result = ((IEdmStructuredType)null).ShouldRequestBodyBeRequired(
                isUpdateOperation: false,
                model: null);

            // Assert
            Assert.True(result); // Safe default
        }

        #endregion

        #region Helper Methods

        private IEdmAction CreateAction(string name, bool isNullable)
        {
            var model = new EdmModel();
            var action = new EdmAction("NS", name, null);
            action.AddParameter("param", EdmCoreModel.Instance.GetString(isNullable));
            model.AddElement(action);
            return action;
        }

        private IEdmEntityType CreateEntityType(string name, bool hasRequiredProperties)
        {
            var entityType = new EdmEntityType("NS", name);
            // Key is always nullable for this helper (to allow testing all-nullable scenarios)
            entityType.AddKeys(entityType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32, true));
            entityType.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String, !hasRequiredProperties);
            return entityType;
        }

        #endregion
    }
}
