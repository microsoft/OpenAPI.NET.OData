// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;

namespace Microsoft.OpenApi.OData.Vocabulary
{
    /// <summary>
    /// Term information attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    internal class TermAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ODataPath"/> class.
        /// </summary>
        /// <param name="qualifiedName">The qualified name of this term.</param>
        public TermAttribute(string qualifiedName)
        {
            QualifiedName = qualifiedName ?? throw new ArgumentNullException(nameof(qualifiedName));
        }

        /// <summary>
        /// Gets the qualified name of this term.
        /// </summary>
        public string QualifiedName { get; }
    }
}
