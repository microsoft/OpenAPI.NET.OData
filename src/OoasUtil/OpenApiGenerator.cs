//---------------------------------------------------------------------
// <copyright file="OpenApiGenerator.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Validation;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData;

namespace OoasUtil
{
    /// <summary>
    /// Open Api generator.
    /// </summary>
    internal abstract class OpenApiGenerator
    {
        /// <summary>
        /// Output format.
        /// </summary>
        public OpenApiFormat Format { get; }

        /// <summary>
        /// Output file.
        /// </summary>
        public string Output { get; }

        /// <summary>
        /// Output file.
        /// </summary>
        public ComLineProcesser CommandLine { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenApiGenerator"/> class.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="output">The output.</param>
        /// <param name="target">The output target.</param>
        public OpenApiGenerator(string output, OpenApiFormat format)
        {
            Output = output;
            Format = format;
        }

        public OpenApiGenerator(ComLineProcesser cmmLine)
        {
            CommandLine = cmmLine;
            Output = cmmLine.Output;
            Format = cmmLine.Format.Value;
        }

        protected IEnumerable<string> GetAnnotations()
        {
            if (CommandLine.AnnotationPath != null)
            {
                foreach (var filePath in Directory.GetFiles(CommandLine.AnnotationPath, "*.xml"))
                {
                    string csdl = File.ReadAllText(filePath);
                    /*
                    if (!CsdlReader.TryParse(XElement.Parse(csdl).CreateReader(), out IEdmModel model, out IEnumerable<EdmError> errors))
                    {
                        Console.WriteLine("Skip: " + filePath + " Is not a valid CSDL files");
                        continue;
                    }
                    */
                    yield return csdl;
                }
            }
        }

        protected string GetAnnotationDefinition()
        {
            if (CommandLine.AnnotationPath != null)
            {
                var file = Directory.GetFiles(CommandLine.AnnotationPath, "AnnotationType.def").FirstOrDefault();
                if (file != null)
                {
                    return File.ReadAllText(file);
                }
            }

            return null;
        }

        protected string MergeWithAnnotation(string csdl)
        {
            IEnumerable<string> annotations = GetAnnotations();
            if (!annotations.Any())
            {
                return csdl;
            }

            int last = csdl.LastIndexOf("</Schema>");
            StringBuilder sb = new StringBuilder(csdl.Substring(0, last));
            string lastString = csdl.Substring(last + 9);
            foreach (string annotation in annotations)
            {
                sb.Append(annotation);
            }

            sb.Append("</Schema>");
//            sb.Append(GetAnnotationDefinition());
            sb.Append("</edmx:DataServices></edmx:Edmx>");
            return sb.ToString();
        }

        /// <summary>
        /// Generate the Open Api.
        /// </summary>
        public bool Generate()
        {
            try
            {
                IEdmModel edmModel = GetEdmModel();

                OpenApiConvertSettings settings = GetSettings();

                using (FileStream fs = File.Create(Output))
                {
                    OpenApiDocument document = edmModel.ConvertToOpenApi(settings);
                    document.Serialize(fs, OpenApiSpecVersion.OpenApi3_0, Format);
                    fs.Flush();
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        protected virtual OpenApiConvertSettings GetSettings()
        {
            return new OpenApiConvertSettings();
        }

        protected abstract IEdmModel GetEdmModel();
    }
}
