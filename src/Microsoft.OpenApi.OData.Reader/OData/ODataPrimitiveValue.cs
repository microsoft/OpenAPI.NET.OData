// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.OpenApi.OData
{
    /// <summary>
    /// Represents an OData primitive value.
    /// </summary>
    internal class ODataPrimitiveValue : ODataValue
    {
        public ODataPrimitiveValue(object value)
        {
            Value = value;
        }

        /// <summary>
        /// Gets the underlying CLR object wrapped by this <see cref="ODataPrimitiveValue"/>.
        /// </summary>
        public object Value { get; private set; }

        public override string? ToString()
        {
            return Value.ToString();
        }
    }
}
