// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.Interfaces;
using Microsoft.OpenApi.Models.References;
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
        /// <summary>
        /// Initializes a new instance of <see cref="DollarCountGetOperationHandler"/> class.
        /// </summary>
        /// <param name="document">The document to use to lookup references.</param>
        public DollarCountGetOperationHandler(OpenApiDocument document) : base(document)
        {
            
        }
        /// <inheritdoc/>
        public override HttpMethod OperationType => HttpMethod.Get;

        /// <summary>
        /// Gets/sets the segment before $count.
        /// this segment could be "entity set", "Collection property", "Composable function whose return is collection",etc.
        /// </summary>
        internal ODataSegment SecondLastSegment { get; set; }
        private ODataSegment firstSegment;
        private int pathCount;        
        private const int SecondLastSegmentIndex = 2;
        private readonly List<IEdmVocabularyAnnotatable> annotatables = [];

        /// <inheritdoc/>
        protected override void Initialize(ODataContext context, ODataPath path)
        {
            base.Initialize(context, path);

            // Get the first segment
            firstSegment = path.Segments[0];

            // get the last second segment
            pathCount = path.Segments.Count;
            if(pathCount >= SecondLastSegmentIndex)
                SecondLastSegment = path.Segments[pathCount - SecondLastSegmentIndex];

            AddODataSegmentToAnnotables(SecondLastSegment, path.Segments.Count > SecondLastSegmentIndex ? path.Segments.SkipLast(SecondLastSegmentIndex).ToArray() : []);
        }
        private void AddODataSegmentToAnnotables(ODataSegment oDataSegment, ODataSegment[] oDataSegments)
        {
            if (oDataSegment is ODataNavigationSourceSegment sourceSegment)
            {
                annotatables.Add(sourceSegment.NavigationSource as IEdmEntitySet);
            }
            else if (oDataSegment is ODataNavigationPropertySegment navigationPropertySegment)
            {
                annotatables.Add(navigationPropertySegment.NavigationProperty);
            }
            else if (oDataSegment is ODataTypeCastSegment odataTypeCastSegment)
            {
                annotatables.Add(odataTypeCastSegment.StructuredType as IEdmVocabularyAnnotatable);
                if (annotatables.Count == 1 && oDataSegments.Length > 0)
                {// we want to look at the parent navigation property or entity set
                    AddODataSegmentToAnnotables(oDataSegments[oDataSegments.Length - 1], oDataSegments.SkipLast(1).ToArray());
                }
            }
        }

        /// <inheritdoc/>
        protected override void SetTags(OpenApiOperation operation)
        {
            string tagName = null;
            operation.Tags ??= new HashSet<OpenApiTagReference>();

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
                ODataSegment lastThirdSegment = Path.Segments[pathCount - 3];
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
                Context.AddExtensionToTag(tagName, Constants.xMsTocType, new OpenApiAny("page"), () => new OpenApiTag()
                {
                    Name = tagName
                });

                operation.Tags.Add(new OpenApiTagReference(tagName, _document));
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
                    new OpenApiResponseReference(Constants.DollarCountSchemaName, _document)
                }
            };
            operation.AddErrorResponses(Context.Settings, _document, false);

            base.SetResponses(operation);
        }

        /// <inheritdoc/>
        protected override void SetParameters(OpenApiOperation operation)
        {
            base.SetParameters(operation);


            var parameter = (string.IsNullOrEmpty(TargetPath) ? null : Context?.CreateSearch(TargetPath, _document)) ??
                        (annotatables.Count == 0 ? null : annotatables.Select(x => Context?.CreateSearch(x, _document)).FirstOrDefault(static x => x is not null));
            if (parameter != null)
            {
                operation.Parameters ??= [];
                operation.Parameters.Add(parameter);
            }

            parameter = (string.IsNullOrEmpty(TargetPath) ? null : Context?.CreateFilter(TargetPath, _document)) ??
                        (annotatables.Count == 0 ? null : annotatables.Select(x => Context?.CreateFilter(x, _document)).FirstOrDefault(static x => x is not null));
            if (parameter != null)
            {
                operation.Parameters ??= [];
                operation.Parameters.Add(parameter);
            }
        }

        protected override void AppendCustomParameters(OpenApiOperation operation)
        {
            ReadRestrictionsType readRestrictions = Context.Model.GetRecord<ReadRestrictionsType>(TargetPath, CapabilitiesConstants.ReadRestrictions);
            if (annotatables.Count > 0)
            {
                ReadRestrictionsType annotatableReadRestrictions = annotatables.Select(x => Context.Model.GetRecord<ReadRestrictionsType>(x, CapabilitiesConstants.ReadRestrictions)).FirstOrDefault(static x => x is not null);
                readRestrictions?.MergePropertiesIfNull(annotatableReadRestrictions);
                readRestrictions ??= annotatableReadRestrictions;
            }
            
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
