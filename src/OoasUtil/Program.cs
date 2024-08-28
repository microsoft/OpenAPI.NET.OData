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

            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableKeyAsSegment = processer.KeyAsSegment,
                EnableDerivedTypesReferencesForResponses = processer.DerivedTypesReferencesForResponses.Value,
                EnableDerivedTypesReferencesForRequestBody = processer.DerivedTypesReferencesForRequestBody.Value,
                RequireDerivedTypesConstraintForBoundOperations = processer.RequireDerivedTypesConstraint.Value,
                EnablePagination = processer.EnablePagination.Value,
                EnableUnqualifiedCall = processer.EnableUnqualifiedCall.Value,
                ShowSchemaExamples = !processer.DisableSchemaExamples.Value,
                OpenApiSpecVersion = processer.Version.Value,
            };

            if (processer.IsLocalFile)
            {
                generator = new FileOpenApiGenerator(processer.Input, processer.Output, processer.Format.Value, settings);
            }
            else
            {
                generator = new UrlOpenApiGenerator(new Uri(processer.Input), processer.Output, processer.Format.Value, settings);
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
