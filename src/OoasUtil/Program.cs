//---------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using System.Reflection;
using Microsoft.OpenApi.OData;

namespace OoasUtil
{
    class Program
    {
        static async System.Threading.Tasks.Task<int> Main(string[] args)
        {
            // args = new[] { "--json", "--input", @"E:\work\OneApiDesign\test\TripService.OData.xml", "-o", @"E:\work\OneApiDesign\test1\Trip.json" };
            // args = new[] { "--yaml", "-i", @"E:\work\OneApiDesign\test\TripService.OData.xml", "-o", @"E:\work\OneApiDesign\test1\Trip.yaml" };
            // args = new[] { "--yaml", "--input", @"http://services.odata.org/TrippinRESTierService", "-o", @"E:\work\OneApiDesign\test1\TripUrl.yaml" };
            // args = new[] { "--json", "-i", @"http://services.odata.org/TrippinRESTierService", "-o", @"E:\work\OneApiDesign\test1\TripUrl.json" };

            ComLineProcessor processor = new ComLineProcessor(args);
            if (!processor.Process())
            {
                return 0; 
            }

            if (!processor.CanContinue)
            {
                return 1;
            }

            OpenApiGenerator generator;

            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableKeyAsSegment = processor.KeyAsSegment,
                EnableDerivedTypesReferencesForResponses = processor.DerivedTypesReferencesForResponses.Value,
                EnableDerivedTypesReferencesForRequestBody = processor.DerivedTypesReferencesForRequestBody.Value,
                RequireDerivedTypesConstraintForBoundOperations = processor.RequireDerivedTypesConstraint.Value,
                EnablePagination = processor.EnablePagination.Value,
                EnableUnqualifiedCall = processor.EnableUnqualifiedCall.Value,
                ShowSchemaExamples = !processor.DisableSchemaExamples.Value,
                OpenApiSpecVersion = processor.Version.Value,
                UseHttpPutForUpdate = processor.UseHttpPutForUpdate.Value,
            };

            if (processor.IsLocalFile)
            {
                generator = new FileOpenApiGenerator(processor.Input, processor.Output, processor.Format, settings);
            }
            else
            {
                generator = new UrlOpenApiGenerator(new Uri(processor.Input), processor.Output, processor.Format, settings);
            }

            if (await generator.GenerateAsync())
            {
                Console.WriteLine("Succeeded!");
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
