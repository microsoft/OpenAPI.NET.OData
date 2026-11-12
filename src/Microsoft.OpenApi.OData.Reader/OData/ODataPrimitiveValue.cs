// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Globalization;

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
            return Value switch
            {
                // Match the OData literal formats of Microsoft.OData.Edm 8 Date and TimeOfDay, which Edm 9 replaced.
                DateOnly date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                TimeOnly time => time.ToString("HH:mm:ss.fffffff", CultureInfo.InvariantCulture),
                _ => Value.ToString(),
            };
        }
    }
}
