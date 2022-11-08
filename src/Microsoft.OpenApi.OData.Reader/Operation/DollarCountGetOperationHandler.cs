// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
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
        internal ODataSegment LastSecondSegment { get; set; }
        private const int SecondLastSegmentIndex = 2;
        /// <inheritdoc/>
        protected override void Initialize(ODataContext context, ODataPath path)
        {
            base.Initialize(context, path);

            // get the last second segment
            int count = path.Segments.Count;
            if(count >= SecondLastSegmentIndex)
                LastSecondSegment = path.Segments.ElementAt(count - SecondLastSegmentIndex);
        }

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            // Summary
            operation.Summary = $"Get the number of the resource";

            // OperationId
            if (Context.Settings.EnableOperationId)
            {
                operation.OperationId = $"Get.Count.{LastSecondSegment.Identifier}-{Path.GetPathHash(Context.Settings)}";
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

        protected override void AppendCustomParameters(OpenApiOperation operation)
        {
            IEdmVocabularyAnnotatable annotatable = null;            
            if (LastSecondSegment is ODataNavigationSourceSegment sourceSegment)
            {
                annotatable = sourceSegment.NavigationSource as IEdmEntitySet;
                annotatable ??= sourceSegment.NavigationSource as IEdmSingleton;
            }
            else if (LastSecondSegment is ODataNavigationPropertySegment navigationPropertySegment)
            {
                annotatable = navigationPropertySegment.NavigationProperty;
            }

            if (annotatable == null)
            {
                return;
            }

            ReadRestrictionsType readRestrictions = Context.Model.GetRecord<ReadRestrictionsType>(annotatable, CapabilitiesConstants.ReadRestrictions);

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
