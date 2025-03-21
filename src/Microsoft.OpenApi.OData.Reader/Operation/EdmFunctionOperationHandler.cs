﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Net.Http;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;

namespace Microsoft.OpenApi.OData.Operation
{
    /// <summary>
    /// The Open Api operation for <see cref="IEdmFunction"/>.
    /// </summary>
    internal class EdmFunctionOperationHandler : EdmOperationOperationHandler
    {
        /// <summary>
        /// Initializes a new instance of <see cref="EdmFunctionOperationHandler"/> class.
        /// </summary>
        /// <param name="document">The document to use to lookup references.</param>
        public EdmFunctionOperationHandler(OpenApiDocument document):base(document)
        {
            
        }
        /// <inheritdoc/>
        public override HttpMethod OperationType => HttpMethod.Get;

        /// <summary>
        /// Gets the Edm Function.
        /// </summary>
        public IEdmFunction Function => EdmOperation as IEdmFunction;

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            base.SetBasicInfo(operation);

            ReadRestrictionsType readRestrictions = Context.Model.GetRecord<ReadRestrictionsType>(TargetPath, CapabilitiesConstants.ReadRestrictions);
            ReadRestrictionsType operationReadRestrictions = Context.Model.GetRecord<ReadRestrictionsType>(EdmOperation, CapabilitiesConstants.ReadRestrictions);
            readRestrictions?.MergePropertiesIfNull(operationReadRestrictions);
            readRestrictions ??= operationReadRestrictions;

            // Description
            if (!string.IsNullOrWhiteSpace(readRestrictions?.LongDescription))
            {
                operation.Description = readRestrictions.LongDescription;
            }
        }

        /// <inheritdoc/>
        protected override void SetExtensions(OpenApiOperation operation)
        {
            operation.Extensions.Add(Constants.xMsDosOperationType, new OpenApiAny("function"));
            base.SetExtensions(operation);
        }
    }
}
