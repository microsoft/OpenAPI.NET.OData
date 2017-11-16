//---------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;

namespace OoasUtil
{
    class Program
    {
        static int Main(string[] args)
        {
            // args = new[] { "--json", "--file", @"E:\work\OneApi Design\test\TripService.OData.xml", "-o", @"E:\work\OneApi Design\test\Trip.json" };
            // args = new[] { "--yaml", "--file", @"E:\work\OneApi Design\test\TripService.OData.xml", "-o", @"E:\work\OneApi Design\test\Trip.yaml" };
            // args = new[] { "--yaml", "--url", @"http://services.odata.org/TrippinRESTierService", "-o", @"E:\work\OneApi Design\test\TripUrl.yaml" };
            // args = new[] { "--json", "--url", @"http://services.odata.org/TrippinRESTierService", "-o", @"E:\work\OneApi Design\test\TripUrl.json" };

            ComLineProcesser processer = new ComLineProcesser(args);
            if (!processer.Process())
            {
                return 0; 
            }

            if (!processer.CanContinue)
            {
                return 1;
            }

            OpenApiGenerator generator;
            if (processer.IsUrl)
            {
                generator = new UrlOpenApiGenerator(new Uri(processer.InputUri), processer.Output, processer.Format.Value);
            }
            else
            {
                generator = new FileOpenApiGenerator(processer.InputFile, processer.Output, processer.Format.Value);
            }

            if (generator.Generate())
            {
                Console.WriteLine("Successed!");
                return 1;
            }
            else
            {
                Console.WriteLine("Failed!");
                return 0;
            }
        }
    }
}
