// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Generator;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;

namespace Microsoft.OpenApi.OData.Operation
{
    /// <summary>
    /// Retrieve a $count get
    /// </summary>
    internal class DollarCountGetOperationHandler : OperationHandler
    {
        /// <inheritdoc/>
        public override OperationType OperationType => OperationType.Get;

        /// <summary>
        /// Gets/sets the segment before $count.
        /// this segment could be "entity set", "Collection property", "Composable function whose return is collection",etc.
        /// </summary>
        internal ODataSegment SecondLastSegment { get; set; }
        private ODataSegment firstSegment;
        private int pathCount;        
        private const int SecondLastSegmentIndex = 2;
        private IEdmVocabularyAnnotatable annotatable;

        /// <inheritdoc/>
        protected override void Initialize(ODataContext context, ODataPath path)
        {
            base.Initialize(context, path);

            // Get the first segment
            firstSegment = path.Segments.First();

            // get the last second segment
            pathCount = path.Segments.Count;
            if(pathCount >= SecondLastSegmentIndex)
                SecondLastSegment = path.Segments.ElementAt(pathCount - SecondLastSegmentIndex);

            if (SecondLastSegment is ODataNavigationSourceSegment sourceSegment)
            {
                annotatable = sourceSegment.NavigationSource as IEdmEntitySet;
            }
            else if (SecondLastSegment is ODataNavigationPropertySegment navigationPropertySegment)
            {
                annotatable = navigationPropertySegment.NavigationProperty;
            }
        }

        /// <inheritdoc/>
        protected override void SetTags(OpenApiOperation operation)
        {
            string tagName = null;
            if (SecondLastSegment is ODataNavigationSourceSegment sourceSegment)
            {
                tagName = TagNameFromNavigationSourceSegment(sourceSegment);
            }
            else if (SecondLastSegment is ODataNavigationPropertySegment)
            {
                tagName = TagNameFromNavigationPropertySegment();
            }
            else if (SecondLastSegment is ODataTypeCastSegment)
            {
                ODataSegment lastThirdSegment = Path.Segments.ElementAt(pathCount - 3);
                if (lastThirdSegment is ODataNavigationSourceSegment sourceSegment2)
                {
                    tagName = TagNameFromNavigationSourceSegment(sourceSegment2);
                }
                else if (lastThirdSegment is ODataNavigationPropertySegment)
                {
                    tagName = TagNameFromNavigationPropertySegment();
                }
            }
            else if (SecondLastSegment is ODataComplexPropertySegment)
            {
                tagName = EdmModelHelper.GenerateComplexPropertyPathTagName(Path, Context);
            }

            if (tagName != null)
            {
                OpenApiTag tag = new()
                {
                    Name = tagName
                };

                // Use an extension for TOC (Table of Content)
                tag.Extensions.Add(Constants.xMsTocType, new OpenApiString("page"));

                operation.Tags.Add(tag);

                Context.AppendTag(tag);
            }

            string TagNameFromNavigationSourceSegment(ODataNavigationSourceSegment sourceSegment)
            {
                return $"{sourceSegment.NavigationSource.Name}.{sourceSegment.NavigationSource.EntityType.Name}";
            }

            string TagNameFromNavigationPropertySegment()
            {
                return EdmModelHelper.GenerateNavigationPropertyPathTagName(Path, Context);
            }

            base.SetTags(operation);
        }

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            // Summary
            operation.Summary = $"Get the number of the resource";

            // OperationId
            if (Context.Settings.EnableOperationId)
            {
                if (SecondLastSegment is ODataNavigationSourceSegment)
                {
                    operation.OperationId = $"{firstSegment.Identifier}.GetCount-{Path.GetPathHash(Context.Settings)}";
                }
                else if (SecondLastSegment is ODataNavigationPropertySegment)
                {
                    var navPropOpId = string.Join(".", EdmModelHelper.RetrieveNavigationPropertyPathsOperationIdSegments(Path, Context));
                    operation.OperationId = $"{navPropOpId}.GetCount-{Path.GetPathHash(Context.Settings)}";
                }
                else if (SecondLastSegment is ODataTypeCastSegment odataTypeCastSegment)
                {
                    IEdmNamedElement targetStructuredType = odataTypeCastSegment.StructuredType as IEdmNamedElement;
                    operation.OperationId = $"{EdmModelHelper.GenerateODataTypeCastPathOperationIdPrefix(Path, Context, false)}.GetCount.As{Utils.UpperFirstChar(targetStructuredType.Name)}-{Path.GetPathHash(Context.Settings)}";
                }
                else if (SecondLastSegment is ODataComplexPropertySegment)
                {
                    operation.OperationId = $"{EdmModelHelper.GenerateComplexPropertyPathOperationId(Path, Context)}.GetCount-{Path.GetPathHash(Context.Settings)}";
                }
            }

            base.SetBasicInfo(operation);
        }

        /// <inheritdoc/>
        protected override void SetResponses(OpenApiOperation operation)
        {
            operation.Responses = new OpenApiResponses
            {
                {
                    Context.Settings.UseSuccessStatusCodeRange ? Constants.StatusCodeClass2XX : Constants.StatusCode200,
                    new OpenApiResponse
                    {
                        UnresolvedReference = true,
                        Reference = new OpenApiReference() {
                            Type = ReferenceType.Response,
                            Id = Constants.DollarCountSchemaName
                        }
                    }
                }
            };
            operation.AddErrorResponses(Context.Settings, false);

            base.SetResponses(operation);
        }

        /// <inheritdoc/>
        protected override void SetParameters(OpenApiOperation operation)
        {
            base.SetParameters(operation);

            if (annotatable == null)
            {
                return;
            }

            OpenApiParameter parameter;

            parameter = Context.CreateSearch(TargetPath) ?? Context.CreateSearch(annotatable);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }

            parameter = Context.CreateFilter(TargetPath) ?? Context.CreateFilter(annotatable);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }
        }

        protected override void AppendCustomParameters(OpenApiOperation operation)
        {
            if (annotatable == null)
            {
                return;
            }

            ReadRestrictionsType readRestrictions = Context.Model.GetRecord<ReadRestrictionsType>(TargetPath, CapabilitiesConstants.ReadRestrictions);
            ReadRestrictionsType annotatableReadRestrictions = Context.Model.GetRecord<ReadRestrictionsType>(annotatable, CapabilitiesConstants.ReadRestrictions);
            readRestrictions?.MergePropertiesIfNull(annotatableReadRestrictions);
            readRestrictions ??= annotatableReadRestrictions;
            
            if (readRestrictions == null)
            {
                return;
            }

            if (readRestrictions.CustomHeaders != null)
            {
                AppendCustomParameters(operation, readRestrictions.CustomHeaders, ParameterLocation.Header);
            }

            if (readRestrictions.CustomQueryOptions != null)
            {
                AppendCustomParameters(operation, readRestrictions.CustomQueryOptions, ParameterLocation.Query);
            }
        }
    }
}
