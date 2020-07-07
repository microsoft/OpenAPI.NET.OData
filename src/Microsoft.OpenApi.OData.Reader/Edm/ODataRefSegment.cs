// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.OpenApi.OData.Edm
{
    /// <summary>
    /// Type cast segment.
    /// </summary>
    public class ODataRefSegment : ODataSegment
    {
        /// <summary>
        /// Get the static instance of $ref segment.
        /// </summary>
        public static ODataRefSegment Instance = new ODataRefSegment();

        /// <summary>
        /// Initializes a new instance of <see cref="ODataRefSegment"/> class.
        /// </summary>
        private ODataRefSegment()
        {
        }

        /// <inheritdoc />
        public override ODataSegmentKind Kind => ODataSegmentKind.Ref;

        /// <inheritdoc />
        public override string Identifier => "$ref";

        /// <inheritdoc />
        public override string GetPathItemName(OpenApiConvertSettings settings, HashSet<string> parameters) => "$ref";
    }
}