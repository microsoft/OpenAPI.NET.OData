// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.OpenApi.OData.Edm
{
    /// <summary>
    /// The $metadata segment.
    /// </summary>
    public class ODataMetadataSegment : ODataSegment
    {
        /// <inheritdoc />
        public override ODataSegmentKind Kind => ODataSegmentKind.Metadata;

        /// <inheritdoc />
        public override string Identifier => "$metadata";

        /// <inheritdoc />
        public override string GetPathItemName(OpenApiConvertSettings settings, HashSet<string> parameters) => "$metadata";
    }
}