//---------------------------------------------------------------------
// <copyright file="ComLineProcesser.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.OpenApi;

namespace OoasUtil
{
    /// <summary>
    /// Command line arguments processer.
    /// </summary>
    internal class ComLineProcesser
    {
        private IList<string> _args;
        private bool _continue;
        public static Version version = new Version(1, 0, 0, 0);

        /// <summary>
        /// Initializes a new instance of the <see cref="ComLineProcesser"/> class.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        public ComLineProcesser(string[] args)
        {
            _args = args;
        }

        /// <summary>
        /// Can continue with the arguments
        /// </summary>
        public bool CanContinue { get { return _continue; } }

        /// <summary>
        /// Input FilePath or Url with the CSDL.
        /// </summary>
        public string Input { get; private set; }

        /// <summary>
        /// Output format.
        /// </summary>
        public OpenApiFormat? Format { get; private set; }

        /// <summary>
        /// Whether KeyAsSegment is used.
        /// </summary>
        public bool? KeyAsSegment { get; private set; }

        /// <summary>
        /// Output OpenApi Specification Version.
        /// </summary>
        public OpenApiSpecVersion? Version { get; private set; }

        /// <summary>
        /// Output file.
        /// </summary>
        public string Output { get; private set; }

        /// <summary>
        /// Is input Url
        /// </summary>
        public bool IsLocalFile { get; private set; }

        /// <summary>
        /// Set the output to produce all derived types in responses.
        /// </summary>
        public bool? DerivedTypesReferencesForResponses { get; private set; }

        /// <summary>
        /// Set the output to expect all derived types in request bodies.
        /// </summary>
        public bool? DerivedTypesReferencesForRequestBody { get; private set; }
        
        /// <summary>
        /// Set the output to expose pagination for collections.
        /// </summary>
        public bool? EnablePagination { get; private set; }

        /// <summary>
        /// Set the output to use unqualified calls for bound operations.
        /// </summary>
        public bool? EnableUnqualifiedCall { get; private set; }

        /// <summary>
        /// Disable examples in the schema.
        /// </summary>
        public bool? DisableSchemaExamples { get; private set; }

        /// <summary>
        /// Gets/Sets a value indicating whether or not to require the
        /// Validation.DerivedTypeConstraint to be applied to NavigationSources
        /// to bind operations of derived types to them.
        /// </summary>
        public bool? RequireDerivedTypesConstraint { get; private set; }

        /// <summary>
        /// Process the arguments.
        /// </summary>
        public bool Process()
        {
            _continue = false;
            if (_args.Count == 0)
            {
                PrintUsage();
                return true;
            }

            try
            {
                for (int i = 0; i < _args.Count; i++)
                {
                    string arg = _args[i];
                    switch (arg)
                    {
                        case "--version":
                        case "-v":
                            PrintVersion();
                            return true;

                        case "--help":
                        case "-h":
                            PrintUsage();
                            return true;

                        case "--input":
                        case "-i":
                            if (!ProcessInput(_args[i + 1]))
                            {
                                return false;
                            }
                            i++;
                            break;

                        case "--keyassegment":
                        case "-k":
                            if (!ProcessKeyAsSegment(true))
                            {
                                return false;
                            }
                            break;

                        case "--yaml":
                        case "-y":
                            if (!ProcessTarget(OpenApiFormat.Yaml))
                            {
                                return false;
                            }
                            break;

                        case "--json":
                        case "-j":
                            if (!ProcessTarget(OpenApiFormat.Json))
                            {
                                return false;
                            }
                            break;

                        case "--specversion":
                        case "-s":
                            if (!int.TryParse(_args[i + 1], out int version) || !ProcessTarget(version))
                            {
                                return false;
                            }
                            ++i;
                            break;

                        case "--output":
                        case "-o":
                            if (!ProcessOutput(_args[i + 1]))
                            {
                                return false;
                            }
                            i++;
                            break;

                        case "--derivedtypesreferencesforresponses":
                        case "-drs":
                            if (!ProcessDerivedTypesReferencesForResponses(true))
                            {
                                return false;
                            }
                            break;

                        case "--derivedtypesreferencesforrequestbody":
                        case "-drq":
                            if (!ProcessDerivedTypesReferencesForRequestBody(true))
                            {
                                return false;
                            }
                            break;

                        case "--requireDerivedTypesConstraint":
                        case "-rdt":
                            if (!ProcessRequireDerivedTypesConstraint(true))
                            {
                                return false;
                            }
                            break;

                        case "--enablepagination":
                        case "-p":
                            if (!ProcessEnablePagination(true))
                            {
                                return false;
                            }
                            break;

                        case "--enableunqualifiedcall":
                        case "-u":
                            if (!ProcessEnableUnqualifiedCall(true))
                            {
                                return false;
                            }
                            break;

                        case "--disableschemaexamples":
                        case "-x":
                            if (!ProcessDisableSchemaExamples(true))
                            {
                                return false;
                            }
                            break;

                        default:
                            PrintUsage();
                            return false;
                    }
                }
            }
            catch
            {
                PrintUsage();
                return false;
            }

            // by default.
            if (Format == null)
            {
                Format = OpenApiFormat.Json;
            }

            if (Version == null)
            {
                Version = OpenApiSpecVersion.OpenApi3_0;
            }

            if (KeyAsSegment == null)
            {
                KeyAsSegment = false;
            }

            if (DerivedTypesReferencesForResponses == null)
            {
                DerivedTypesReferencesForResponses = false;
            }

            if (DerivedTypesReferencesForRequestBody == null)
            {
                DerivedTypesReferencesForRequestBody = false;
            }

            if (RequireDerivedTypesConstraint == null)
            {
                RequireDerivedTypesConstraint = false;
            }

            if (EnablePagination == null)
            {
                EnablePagination = false;
            }

            if (EnableUnqualifiedCall == null)
            {
                EnableUnqualifiedCall = false;
            }

            if (DisableSchemaExamples == null)
            {
                DisableSchemaExamples = false;
            }

            _continue = ValidateArguments();
            return _continue;
        }

        private bool ProcessInput(string input)
        {
            if (Input != null)
            {
                Console.WriteLine("[Error:] Multiple [--input|-i] are not allowed.\n");
                PrintUsage();
                return false;
            }

            Input = input;
            return true;
        }

        private bool ProcessOutput(string file)
        {
            if (Output != null)
            {
                Console.WriteLine("[Error:] Multiple [--output|-o] are not allowed.\n");
                PrintUsage();
                return false;
            }

            Output = file;
            return true;
        }

        private bool ProcessTarget(OpenApiFormat format)
        {
            if (Format != null)
            {
                Console.WriteLine("[Error:] Multiple [--json|-j|--yaml|-y] are not allowed.\n");
                PrintUsage();
                return false;
            }

            Format = format;
            return true;
        }

        private bool ProcessKeyAsSegment(bool keyAsSegment)
        {
            if (KeyAsSegment != null)
            {
                Console.WriteLine("[Error:] Multiple [--keyassegment|-k] are not allowed.\n");
                PrintUsage();
                return false;
            }

            KeyAsSegment = keyAsSegment;
            return true;
        }

        private bool ProcessDerivedTypesReferencesForResponses(bool derivedTypesReferencesForResponses)
        {
            if (DerivedTypesReferencesForResponses != null)
            {
                Console.WriteLine("[Error:] Multiple [--derivedtypesreferencesforresponses|-drs] are not allowed.\n");
                PrintUsage();
                return false;
            }

            DerivedTypesReferencesForResponses = derivedTypesReferencesForResponses;
            return true;
        }

        private bool ProcessDerivedTypesReferencesForRequestBody(bool derivedTypesReferencesForRequestBody)
        {
            if (DerivedTypesReferencesForRequestBody != null)
            {
                Console.WriteLine("[Error:] Multiple [--derivedtypesreferencesforrequestbody|-drq] are not allowed.\n");
                PrintUsage();
                return false;
            }

            DerivedTypesReferencesForRequestBody = derivedTypesReferencesForRequestBody;
            return true;
        }

        private bool ProcessRequireDerivedTypesConstraint(bool requireDerivedTypesConstraint)
        {
            if (RequireDerivedTypesConstraint != null)
            {
                Console.WriteLine("[Error:] Multiple [--requireDerivedTypesConstraint|-rdt] are not allowed.\n");
                PrintUsage();
                return false;
            }

            RequireDerivedTypesConstraint = requireDerivedTypesConstraint;
            return true;
        }

        private bool ProcessEnablePagination(bool enablePagination)
        {
            if (EnablePagination != null)
            {
                Console.WriteLine("[Error:] Multiple [--enablepagination|-p] are not allowed.\n");
                PrintUsage();
                return false;
            }

            EnablePagination = enablePagination;
            return true;
        }

        private bool ProcessEnableUnqualifiedCall(bool enableUnqualifiedCall)
        {
            if (EnableUnqualifiedCall != null)
            {
                Console.WriteLine("[Error:] Multiple [--enableunqualifiedcall|-u] are not allowed.\n");
                PrintUsage();
                return false;
            }

            EnableUnqualifiedCall = enableUnqualifiedCall;
            return true;
        }

        private bool ProcessDisableSchemaExamples(bool disableSchemaExamples)
        {
            if (DisableSchemaExamples != null)
            {
                Console.WriteLine("[Error:] Multiple [--disableschemaexamples|-x] are not allowed.\n");
                PrintUsage();
                return false;
            }

            DisableSchemaExamples = disableSchemaExamples;
            return true;
        }

        private bool ProcessTarget(int version)
        {
            if (Version != null)
            {
                Console.WriteLine("[Error:] Multiple [--specversion|-s] are not allowed.\n");
                PrintUsage();
                return false;
            }
            else if (version > 3 || version < 2)
            {
                Console.WriteLine("[Error:] Only OpenApi specification version 2 or 3 are supported.\n");
                PrintUsage();
                return false;
            }

            Version = version == 2 ? OpenApiSpecVersion.OpenApi2_0 : OpenApiSpecVersion.OpenApi3_0;
            return true;
        }

        private bool ValidateArguments()
        {
            if (String.IsNullOrEmpty(Input))
            {
                Console.WriteLine("[Error:] At least one of [--input|-i] is required.\n");
                PrintUsage();
                return false;
            }

            IsLocalFile = IsLocalPath(Input);
            if (IsLocalFile)
            {
                if (!File.Exists(Input))
                {
                    Console.WriteLine("[Error]: File (" + Input + ") is not existed.\n");
                    return false;
                }
            }

            if (String.IsNullOrEmpty(Output))
            {
                Console.WriteLine("[Error:] [--output|-o] is required.\n");
                PrintUsage();
                return false;
            }

            return true;
        }

        public static void PrintUsage()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Usage: OoasUtil.exe [options]\n");

            sb.Append("\nOptions:\n");
            sb.Append("  --help|-h\t\t\tDisplay help.\n");
            sb.Append("  --version|-v\t\t\tDisplay version.\n");
            sb.Append("  --input|-i CsdlFileOrUrl\tSet the CSDL file name or the OData Service Url.\n");
            sb.Append("  --output|-o OutputFile\tSet the output file name.\n");
            sb.Append("  --keyassegment|-k\t\t\tSet the output to use key-as-segment style URLs.\n");
            sb.Append("  --derivedtypesreferencesforresponses|-drs\t\t\tSet the output to produce all derived types in responses.\n");
            sb.Append("  --derivedtypesreferencesforrequestbody|-drq\t\t\tSet the output to expect all derived types in request bodies.\n");
            sb.Append("  --requireDerivedTypesConstraint|-rdt\t\t\tSet the output to require derived type constraint to bind Operations.\n");
            sb.Append("  --enablepagination|-p\t\t\tSet the output to expose pagination for collections.\n");
            sb.Append("  --enableunqualifiedcall|-u\t\t\tSet the output to use unqualified calls for bound operations.\n");
            sb.Append("  --disableschemaexamples|-x\t\t\tDisable examples in the schema.\n");
            sb.Append("  --json|-j\t\t\tSet the output format as JSON.\n");
            sb.Append("  --yaml|-y\t\t\tSet the output format as YAML.\n");
            sb.Append("  --specversion|-s IntVersion\tSet the OpenApi Specification version of the output. Only 2 or 3 are supported.\n");

            sb.Append("\nExamples:\n");
            sb.Append("    OoasUtil.exe -y -i http://services.odata.org/TrippinRESTierService -o trip.yaml\n");
            sb.Append("    OoasUtil.exe -j -s 2 -i c:\\csdl.xml -o trip.json\n");

            Console.WriteLine(sb.ToString());
        } 

        public static string Copyright()
        {
            return
                "Microsoft (R) OData To Open API Utilities, Version " + version.ToString() + " \n" +
                "Copyright(C) Microsoft Corporation.  All rights reserved.\n\n";
        }

        public static void PrintVersion()
        {
            Console.WriteLine(version.ToString());
        }

        private static bool IsLocalPath(string path)
        {
            bool ret = true;
            try
            {
                ret = new Uri(path).IsFile;
            }
            catch
            {
                if (path.StartsWith("http://") ||
                    path.StartsWith(@"http:\\") ||
                    path.StartsWith("https://") ||
                    path.StartsWith(@"https:\\"))
                {
                    return false;
                }
            }

            return ret;
        }
    }
}
