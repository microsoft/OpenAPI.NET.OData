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
using Microsoft.OpenApi.OData.Edm;

namespace UpdateDocs
{
    class Program
    {
        static int Main(string[] args)
        {
            if (!Test())
            {
                return 0;
            }
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
                    settings.ServiceRoot = new Uri("https://graph.microsoft.com/v1.0");
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

                settings.EnableKeyAsSegment = true;
                settings.EnableNavigationPropertyPath = true;
                output = oas30 + "/" + fileName + "_content.json";
                document = model.ConvertToOpenApi(settings);
                File.WriteAllText(output, document.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0));

                Console.WriteLine("Output [ " + fileName + " ] Succeessful!");
            }

            Console.WriteLine("\n==> All Done!");
            return 0;
        }

        public static bool Test()
        {
            var model = LoadEdmModel(@"E:\github\microsoft\OpenAPI.NET.OData\test\Microsoft.OpenAPI.OData.Reader.Tests\Resources\Graph.Beta.OData2.xml");

            //var model = LoadEdmModel(@"E:\work\OpenApi\metadata_withRequestTerms.xml");

            /*
            OpenApiDocument document = model.ConvertToOpenApi();
            string output = @"E:\work\OpenApi\metadata_withRequestTerms.json";
            File.WriteAllText(output, document.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0));
            return false;*/
            ODataPathProvider provider = new ODataPathProvider(model);
            var paths = provider.CreatePaths();

            using (StreamWriter file = new StreamWriter(@"e:\work\openapi\Graph.Beta.OData.AllPath5.txt"))
            {
                foreach (var path in paths)
                {
                    string pathItem = path.GetPathItemName();

                    file.WriteLine(pathItem);
                }

                file.Flush();
            }

            return false;
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
