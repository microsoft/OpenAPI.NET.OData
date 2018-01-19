//---------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData;
using Microsoft.OpenApi.Extensions;
using System;
using System.IO;
using System.Xml.Linq;

namespace UpdateDocs
{
    class Program
    {
        static int Main(string[] args)
        {
            // we assume the path are existed for simplicity.
            string path = Directory.GetCurrentDirectory();
            string csdl = path + "../../../../../../docs/csdl";
            string oas20 = path + "../../../../../../docs/oas_2_0";
            string oas30 = path + "../../../../../../docs/oas3_0_0";


            foreach (var filePath in Directory.GetFiles(csdl, "*.xml"))
            {
                IEdmModel model = LoadEdmModel(filePath);
                if (model == null)
                {
                    continue;
                }

                FileInfo fileInfo = new FileInfo(filePath);
                string fileName = fileInfo.Name.Substring(0, fileInfo.Name.Length - 4);

                OpenApiConvertSettings settings = new OpenApiConvertSettings();
                if (fileName.Contains("graph.beta"))
                {
                    settings.ServiceRoot = new Uri("https://graph.microsoft.com/beta");
                }
                else if (fileName.Contains("graph1.0"))
                {
                    settings.ServiceRoot = new Uri("https://graph.microsoft.com/1.0");
                }

                OpenApiDocument document = model.ConvertToOpenApi(settings);

                string output = oas20 + "/" + fileName + ".yaml";
                File.WriteAllText(output, document.SerializeAsYaml(OpenApiSpecVersion.OpenApi2_0));

                output = oas20 + "/" + fileName + ".json";
                File.WriteAllText(output, document.SerializeAsJson(OpenApiSpecVersion.OpenApi2_0));

                output = oas30 + "/" + fileName + ".yaml";
                File.WriteAllText(output, document.SerializeAsYaml(OpenApiSpecVersion.OpenApi3_0));

                output = oas30 + "/" + fileName + ".json";
                File.WriteAllText(output, document.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0));

                Console.WriteLine("Output [ " + fileName + " ] Succeessful!");
            }

            Console.WriteLine("\n==> All Done!");
            return 0;
        }

        public static IEdmModel LoadEdmModel(string file)
        {
            try
            {
                string csdl = File.ReadAllText(file);
                return CsdlReader.Parse(XElement.Parse(csdl).CreateReader());
            }
            catch
            {
                Console.WriteLine("Cannot load EDM from file: " + file);
                return null;
            }
        }
    }
}
