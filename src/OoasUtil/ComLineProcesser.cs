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
        /// Input file with the CSDL
        /// </summary>
        public string InputFile { get; private set; }

        /// <summary>
        /// Input Url with the CSDL
        /// </summary>
        public string InputUri { get; private set; }

        /// <summary>
        /// Output format.
        /// </summary>
        public OpenApiFormat? Format { get; private set; }

        /// <summary>
        /// Output file.
        /// </summary>
        public string Output { get; private set; }

        /// <summary>
        /// Is input Url
        /// </summary>
        public bool IsUrl
        {
            get
            {
                return InputUri != null;
            }
        }

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

                        case "--url":
                        case "-u":
                            if (!ProcessUrl(_args[i+1]))
                            {
                                return false;
                            }
                            i++;
                            break;

                        case "--file":
                        case "-f":
                            if (!ProcessFile(_args[i + 1]))
                            {
                                return false;
                            }
                            i++;
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

                        case "--output":
                        case "-o":
                            if (!ProcessOutput(_args[i + 1]))
                            {
                                return false;
                            }
                            i++;
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

            ValidateArguments();
            _continue = true;
            return true;
        }

        private bool ProcessUrl(string uri)
        {
            if (InputUri != null)
            {
                Console.WriteLine("[Error:] Multiple [--url|-u] are not allowed.\n");
                PrintUsage();
                return false;
            }

            if (InputFile != null)
            {
                Console.WriteLine("[Error:] Already [--file|-f] is used.\n");
                PrintUsage();
                return false;
            }

            InputUri = uri;
            return true;
        }

        private bool ProcessFile(string file)
        {
            if (InputFile != null)
            {
                Console.WriteLine("[Error:] Multiple [--file|-f] are not allowed.\n");
                PrintUsage();
                return false;
            }

            if (InputUri != null)
            {
                Console.WriteLine("[Error:] Already [--url|-u] is used.\n");
                PrintUsage();
                return false;
            }

            InputFile = file;
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

        private bool ValidateArguments()
        {
            if (String.IsNullOrEmpty(InputUri) && String.IsNullOrEmpty(InputFile))
            {
                Console.WriteLine("[Error:] At least one of [--url|-u|--file|-f] is required.\n");
                PrintUsage();
                return false;
            }

            if (!String.IsNullOrEmpty(InputFile))
            {
                if (!File.Exists(InputFile))
                {
                    Console.WriteLine("[Error]: File (" + InputFile + ") is not existed.\n");
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
            sb.Append("  --url|-u ServiceUrl\t\tSet the OData Service Url.\n");
            sb.Append("  --file|-f CsdlFile\t\tSet the CSDL file name.\n");
            sb.Append("  --output|-o CsdlFile\t\tSet the output file name.\n");
            sb.Append("  --json|-j\t\t\tSet the output format as JSON.\n");
            sb.Append("  --yaml|-y\t\t\tSet the output format as YAML.\n");

            sb.Append("\nExamples:\n");
            sb.Append("    OoasUtil.exe -y -u http://services.odata.org/TrippinRESTierService -o trip.yaml\n");
            sb.Append("    OoasUtil.exe -j -u c:\\csdl.xml -o trip.json\n");

            Console.WriteLine(sb.ToString());
        } 

        public static string Copyright()
        {
            return
                "Microsoft (R) OData To Open API Utitlity, Version " + version.ToString() + " \n" +
                "Copyright(C) Microsoft Corporation.  All rights reserved.\n\n";
        }

        public static void PrintVersion()
        {
            Console.WriteLine(version.ToString());
        }
    }
}
