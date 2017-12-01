// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;

namespace Microsoft.OpenApi.OData.Generator
{
    /// <summary>
    /// Edm model visitor
    /// </summary>
    internal class EdmModelVisitor
    {
        /// <summary>
        /// Gets the value indicating Edm spatial type using in the Edm model.
        /// </summary>
        public bool IsSpatialTypeUsed { get; private set; }

        /// <summary>
        /// Visit <see cref="IEdmModel"/> to gather some information.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        public void Visit(IEdmModel model)
        {
            if (model == null)
            {
                return;
            }

            IsSpatialTypeUsed = false;

            foreach (var element in model.SchemaElements)
            {
                switch (element.SchemaElementKind)
                {
                    case EdmSchemaElementKind.Action:
                    case EdmSchemaElementKind.Function:
                        VisitOperation((IEdmOperation)element);
                        break;

                    case EdmSchemaElementKind.TypeDefinition:
                        VisitSchemaType((IEdmType)element);
                        break;
                }
            }
        }

        private void VisitOperation(IEdmOperation operation)
        {
            if (operation.ReturnType != null)
            {
                this.VisitTypeReference(operation.ReturnType);
            }

            foreach (var parameter in operation.Parameters)
            {
                VisitTypeReference(parameter.Type);

                if (IsSpatialTypeUsed)
                {
                    break;
                }
            }
        }

        private void VisitSchemaType(IEdmType definition)
        {
            switch (definition.TypeKind)
            {
                case EdmTypeKind.Complex:
                case EdmTypeKind.Entity:
                    VisitStructuredType((IEdmStructuredType)definition);
                    break;
            }
        }

        private void VisitStructuredType(IEdmStructuredType structuredType)
        {
            foreach (var property in structuredType.DeclaredProperties)
            {
                VisitTypeReference(property.Type);

                if (IsSpatialTypeUsed)
                {
                    break;
                }
            }
        }

        private void VisitTypeReference(IEdmTypeReference reference)
        {
            switch (reference.TypeKind())
            {
                case EdmTypeKind.Collection:
                    IEdmCollectionTypeReference collectionTypeReferent = reference.AsCollection();
                    VisitTypeReference(collectionTypeReferent.ElementType());
                    break;

                case EdmTypeKind.Primitive:
                    VisitPrimitiveTypeReference(reference.AsPrimitive());
                    break;
            }
        }

        private void VisitPrimitiveTypeReference(IEdmPrimitiveTypeReference reference)
        {
            switch (reference.PrimitiveKind())
            {
                case EdmPrimitiveTypeKind.Geography:
                case EdmPrimitiveTypeKind.GeographyPoint:
                case EdmPrimitiveTypeKind.GeographyLineString:
                case EdmPrimitiveTypeKind.GeographyPolygon:
                case EdmPrimitiveTypeKind.GeographyCollection:
                case EdmPrimitiveTypeKind.GeographyMultiPolygon:
                case EdmPrimitiveTypeKind.GeographyMultiLineString:
                case EdmPrimitiveTypeKind.GeographyMultiPoint:
                case EdmPrimitiveTypeKind.Geometry:
                case EdmPrimitiveTypeKind.GeometryPoint:
                case EdmPrimitiveTypeKind.GeometryLineString:
                case EdmPrimitiveTypeKind.GeometryPolygon:
                case EdmPrimitiveTypeKind.GeometryCollection:
                case EdmPrimitiveTypeKind.GeometryMultiPolygon:
                case EdmPrimitiveTypeKind.GeometryMultiLineString:
                case EdmPrimitiveTypeKind.GeometryMultiPoint:
                    IsSpatialTypeUsed = true;
                    break;
            }
        }
    }
}
