// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.OpenApi.OData.Edm
{
    /// <summary>
    /// Stream segment.
    /// </summary>
    public class ODataStreamContentSegment : ODataSegment
    {
        /// <inheritdoc />
        public override ODataSegmentKind Kind => ODataSegmentKind.StreamContent;

        /// <inheritdoc />
        public override string Identifier => "$value";

        /// <inheritdoc />
        public override string GetPathItemName(OpenApiConvertSettings settings, HashSet<string> parameters) => "$value";
    }
}