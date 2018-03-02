// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Validation;
using Microsoft.OpenApi.Exceptions;

namespace Microsoft.OpenApi.OData.Annotations
{
    internal static class AnnotationEdmModelExtensions
    {
        public static IEdmModel AppendAnnotations(this IEdmModel model)
        {
            IEnumerable<EdmError> errors;
            StringWriter sw = new StringWriter();
            XmlWriter xw = XmlWriter.Create(sw);
            if (!CsdlWriter.TryWriteCsdl(model, xw, CsdlTarget.OData, out errors))
            {
                throw new OpenApiException("");
            }

            xw.Flush();
            xw.Close();
            string outputCsdl = sw.ToString();

            int last = outputCsdl.LastIndexOf("</Schema>");
            if (last == -1)
            {
                return model;
            }

            StringBuilder sb = new StringBuilder(outputCsdl.Substring(0, last + 9));

            foreach (var def in GetDefs())
            {
                using (Stream stream = typeof(Resources).Assembly.GetManifestResourceStream(def))
                using (TextReader reader = new StreamReader(stream))
                {
                    string annotationDef = reader.ReadToEnd();
                    sb.Append(annotationDef);
                }
            }

            sb.Append("</edmx:DataServices></edmx:Edmx>");
            outputCsdl = sb.ToString();

            return CsdlReader.Parse(XElement.Parse(outputCsdl).CreateReader());
        }

        private static IEnumerable<string> GetDefs()
        {
            Assembly assmebly = typeof(Resources).Assembly;
            var resources = assmebly.GetManifestResourceNames();
            return resources.Where(r => r.Contains(".Annotations.") && r.EndsWith(".def"));
        }
    }
}
