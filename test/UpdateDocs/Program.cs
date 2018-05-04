//---------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using System.IO;
using System.Xml.Linq;
using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Validation;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData;
using Microsoft.OpenApi.Extensions;
using System.Linq;
using System.Text;

namespace UpdateDocs
{
    class Program
    {
        private static bool ProcessAnnotation(string path)
        {
            string csdlpath = path + "../../../../../../docs/annotations";
            foreach (var filePath in Directory.GetFiles(csdlpath, "*.xml"))
            {
                string csdl = File.ReadAllText(filePath);
                if (CsdlReader.TryParse(XElement.Parse(csdl).CreateReader(), out IEdmModel model, out IEnumerable<EdmError> errors))
                {
                    Console.WriteLine("Read [" + filePath + "] OK!");
                }
                else
                {
                    Console.WriteLine("Read [" + filePath + "] Failed!");
                    foreach (var error in errors)
                    {
                        Console.WriteLine(error.ErrorMessage + ":" + error.ErrorLocation);
                    }

                    continue;
                }

                foreach (var a in model.SchemaElements)
                {
                    Console.WriteLine(a.Name + ":" + a.SchemaElementKind);
                }

                foreach(var a in model.VocabularyAnnotations)
                {
                    Console.WriteLine(a.Qualifier + a.Term + a.Target);
                }
            }

            return true;
        }

        static int Main(string[] args)
        {
            // we assume the path are existed for simplicity.
            string path = Directory.GetCurrentDirectory();
            string csdl = path + "../../../../../../docs/csdl";
            string oas20 = path + "../../../../../../docs/oas_2_0";
            string oas30 = path + "../../../../../../docs/oas3_0_0";
            string annotationPath = path + "../../../../../../docs/annotations";
            /*
            if (ProcessAnnotation(path))
            {
                return 0;
            }*/

            foreach (var filePath in Directory.GetFiles(csdl, "*.xml"))
            {
                /*
                IEdmModel model = LoadEdmModel(filePath, annotationPath);
                if (model == null)
                {
                    continue;
                }*/
                string edmCsdl = LoadCsdl(filePath, annotationPath);
                if (edmCsdl == null)
                {
                    continue;
                }

                File.WriteAllText("c:\\a.xml", edmCsdl);

                FileInfo fileInfo = new FileInfo(filePath);
                string fileName = fileInfo.Name.Substring(0, fileInfo.Name.Length - 4);

                OpenApiConvertSettings settings = new OpenApiConvertSettings();
                if (fileName.Contains("graph.beta"))
                {
                    settings.ServiceRoot = new Uri("https://graph.microsoft.com/beta");
                }
                else if (fileName.Contains("graph1.0"))
                {
                    settings.ServiceRoot = new Uri("https://graph.microsoft.com/v1.0");
                }
                else
                {
                    continue;
                }

                OpenApiDocument document = edmCsdl.ConvertToOpenApi(settings);

                string output = oas20 + "/" + fileName + ".yaml";
                File.WriteAllText(output, document.SerializeAsYaml(OpenApiSpecVersion.OpenApi2_0));

                output = oas20 + "/" + fileName + ".json";
                File.WriteAllText(output, document.SerializeAsJson(OpenApiSpecVersion.OpenApi2_0));

                output = oas30 + "/" + fileName + ".yaml";
                File.WriteAllText(output, document.SerializeAsYaml(OpenApiSpecVersion.OpenApi3_0));

                output = oas30 + "/" + fileName + ".json";
                File.WriteAllText(output, document.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0));

                settings.KeyAsSegment = true;
                settings.EnableNavigationPropertyPath = true;
                settings.NavigationPropertyDepth = 5;
                output = oas30 + "/" + fileName + "_content.json";
                document = edmCsdl.ConvertToOpenApi(settings);
                File.WriteAllText(output, document.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0));

                settings.NavigationPropertyDepth = 3;
                settings.CountKeySegmentAsDepth = false;
                output = oas30 + "/" + fileName + "_content_withoutKeySegment.json";
                document = edmCsdl.ConvertToOpenApi(settings);
                File.WriteAllText(output, document.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0));

                Console.WriteLine("Output [ " + fileName + " ] Succeessful!");
            }

            Console.WriteLine("\n==> All Done!");
            return 0;
        }

        public static string LoadCsdl(string file, string annotationPath)
        {
            string csdl = File.ReadAllText(file);

            if (file.Contains("graph.beta.xml"))
            {
                csdl = MergeWithAnnotation(csdl, annotationPath);
            }

            return csdl;
        }

        public static IEdmModel LoadEdmModel(string file, string annotationPath)
        {
            try
            {
                string csdl = File.ReadAllText(file);

                if (file.Contains("graph.beta.xml"))
                {
                    csdl = MergeWithAnnotation(csdl, annotationPath);
                }

                return CsdlReader.Parse(XElement.Parse(csdl).CreateReader());
            }
            catch
            {
                Console.WriteLine("Cannot load EDM from file: " + file);
                return null;
            }
        }

        protected static IEnumerable<string> GetAnnotations(string annotationPath)
        {
            foreach (var filePath in Directory.GetFiles(annotationPath, "*.xml"))
            {
                string csdl = File.ReadAllText(filePath);
                yield return csdl;
            }
        }

        protected static string MergeWithAnnotation(string csdl, string annotationPath)
        {
            IEnumerable<string> annotations = GetAnnotations(annotationPath);
            if (!annotations.Any())
            {
                return csdl;
            }

            int last = csdl.LastIndexOf("</Schema>");
            if (last == -1)
            {
                return csdl;
            }

            StringBuilder sb = new StringBuilder(csdl.Substring(0, last));
            string lastString = csdl.Substring(last + 9);
            foreach (string annotation in annotations)
            {
                sb.Append(annotation);
            }

            sb.Append("\n    </Schema>");
            sb.Append("\n  </edmx:DataServices>\n</edmx:Edmx>");
            return sb.ToString();
        }
    }
}
