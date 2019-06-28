// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.OpenApi.OData.Vocabulary
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    internal class TermAttribute : Attribute
    {
        public TermAttribute(string qualifiedName)
        {
            QualifiedName = qualifiedName ?? throw new ArgumentNullException(nameof(qualifiedName));
        }

        public string QualifiedName { get; }
    }

    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    internal class SubTypeAttribute : Attribute
    {
        public SubTypeAttribute(params KeyValuePair<string, Type>[] typeInfos)
        {
            SubTypes = new Dictionary<string, Type>();
            foreach (KeyValuePair<string, Type> item in typeInfos)
            {
                SubTypes.Add(item);
            }
        }

        public SubTypeAttribute(string fullTypeName, Type type)
        {
            FullName = fullTypeName ?? throw new ArgumentNullException(nameof(fullTypeName));
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        public string FullName { get; }

        public Type Type { get; }

        public IDictionary<string, Type> SubTypes { get; }
    }
}
