// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Microsoft.OpenApi.OData.Annotations
{
    internal static class Resources
    {
        public static IEnumerable<Stream> GetStreams()
        {
            Assembly assmebly = typeof(Resources).Assembly;
            var resources = assmebly.GetManifestResourceNames();
            var annotations = resources.Where(r => r.Contains(".Annotations.") && r.EndsWith(".xml"));
            foreach(var annotation in annotations)
            {
                yield return assmebly.GetManifestResourceStream(annotation);
            }
        }
    }
}
