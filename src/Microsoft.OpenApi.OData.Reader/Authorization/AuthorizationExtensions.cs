// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Validation;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.Exceptions;
using Microsoft.OpenApi.OData.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Microsoft.OpenApi.OData.Authorization
{
    internal static class AuthorizationExtensions
    {
        public static IList<Authorization> GetAuthorizationsVocabularyAnnotation(this IEdmModel model)
        {
            Utils.CheckArgumentNull(model, nameof(model));

            if (model.EntityContainer == null)
            {
                return null;
            }

            IEdmVocabularyAnnotation annotation = model.GetVocabularyAnnotation(model.EntityContainer, AuthorizationConstants.Authorizations);
            if (annotation == null ||
                annotation.Value == null ||
                annotation.Value.ExpressionKind != EdmExpressionKind.Collection)
            {
                return null;
            }


            return null;
        }

        /*
        public static IEdmModel GetAuthorizationEdmModel(this IEdmModel model)
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

            using (Stream stream = GetStream())
            using (TextReader reader = new StreamReader(stream))
            {
                string def = reader.ReadToEnd();

                int last = outputCsdl.LastIndexOf("</Schema>");
                StringBuilder sb = new StringBuilder(outputCsdl.Substring(0, last + 9));
                sb.Append(def);

                sb.Append("</edmx:DataServices></edmx:Edmx>");
                outputCsdl = sb.ToString();
            }

            return CsdlReader.Parse(XElement.Parse(outputCsdl).CreateReader());
        }

        public static Stream GetStream()
        {
            Assembly assmebly = typeof(Resources).Assembly;
            var resources = assmebly.GetManifestResourceNames();
            var annotation = resources.FirstOrDefault(r => r.EndsWith("Authorization.def"));
            if (annotation == null)
            {
                return null;
            }

            return assmebly.GetManifestResourceStream(annotation);
        }*/
    }
}
