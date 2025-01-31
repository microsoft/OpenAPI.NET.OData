//---------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using System.IO;
using System.Xml.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OpenApi;
using Microsoft.OpenApi.OData;
using Microsoft.OpenApi.Extensions;
using System.Threading.Tasks;

namespace UpdateDocs
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            // we assume the path are existed for simplicity.
            string path = Directory.GetCurrentDirectory();
            string parentPath = Path.Combine(path, "..", "..", "..", "..", "..");
            string csdl = Path.Combine(parentPath, "docs", "csdl");
            string oas20 = Path.Combine(parentPath, "docs", "oas_2_0");
            string oas30 = Path.Combine(parentPath, "docs", "oas3_0_0");
            string oas31 = Path.Combine(parentPath, "docs", "oas3_1_0");

            foreach (var filePath in Directory.GetFiles(csdl, "*.xml"))
            {
                Console.WriteLine(filePath);

                IEdmModel model = await LoadEdmModelAsync(filePath);
                if (model == null)
                {
                    continue;
                }

                FileInfo fileInfo = new FileInfo(filePath);
                string fileName = fileInfo.Name.Substring(0, fileInfo.Name.Length - 4);

                OpenApiConvertSettings settings = new OpenApiConvertSettings();
                if (fileName.Contains("graph.beta"))
                {
                    settings.PrefixEntityTypeNameBeforeKey = true;
                    settings.ServiceRoot = new Uri("https://graph.microsoft.com/beta");
                }
                else if (fileName.Contains("graph1.0"))
                {
                    settings.PrefixEntityTypeNameBeforeKey = true;
                    settings.ServiceRoot = new Uri("https://graph.microsoft.com/v1.0");
                }

                settings.EnableKeyAsSegment = true;
                settings.EnableUnqualifiedCall = true;
                var output = Path.Combine(oas31, fileName + ".json");
                var document = model.ConvertToOpenApi(settings);
                await File.WriteAllTextAsync(output, await document.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_1));

                output = Path.Combine(oas30, fileName + ".json");
                await File.WriteAllTextAsync(output, await document.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0));

                output = Path.Combine(oas20, fileName + ".json");
                await File.WriteAllTextAsync(output, await document.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi2_0));

                Console.WriteLine("Output [ " + fileName + " ] Successful!");
            }

            Console.WriteLine("\n==> All Done!");
            return 0;
        }

        private static async Task<IEdmModel> LoadEdmModelAsync(string file)
        {
            try
            {
                string csdl = await File.ReadAllTextAsync(file);
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
