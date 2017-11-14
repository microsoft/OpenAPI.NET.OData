// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using Microsoft.OpenApi.OData.Commons;

namespace Microsoft.OpenApi.OData
{
    /// <summary>
    /// Represents the Open Api Data type metadata attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    internal class MetadataAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataAttribute"/> class.
        /// </summary>
        /// <param name="name">The display name.</param>
        public MetadataAttribute(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw Error.ArgumentNullOrEmpty(nameof(name));
            }

            Name = name;
        }

        /// <summary>
        /// The display Name.
        /// </summary>
        public string Name { get; }
    }
}
