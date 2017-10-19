//---------------------------------------------------------------------
// <copyright file="Resources.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using System.IO;

namespace Microsoft.OData.OpenAPI
{
    internal static class Resources
    {
        public const string SectionFormat = "##[{0}]";

        public static string GetString(string fileName)
        {
            return GetString(fileName, null);
        }

        public static string GetString(string fileName, string section)
        {
            using (Stream stream = GetStream(fileName))
            using (TextReader reader = new StreamReader(stream))
            {
                string fileString = reader.ReadToEnd();

                if (section == null)
                {
                    return fileString;
                }

                return GetStringInSection(fileString, section);
            }
        }

        private static Stream GetStream(string fileName)
        {
            string path = GetPath(fileName);
            Stream stream = typeof(Resources).Assembly.GetManifestResourceStream(path);

            if (stream == null)
            {
                string message = Error.Format("The embedded resource '{0}' was not found.", path);
                throw new FileNotFoundException(message, path);
            }

            return stream;
        }

        private static string GetPath(string fileName)
        {
            const string projectDefaultNamespace = "Microsoft.OData.OpenAPI.Tests";
            const string resourcesFolderName = "Resources";
            const string pathSeparator = ".";
            return projectDefaultNamespace + pathSeparator + resourcesFolderName + pathSeparator + fileName;
        }

        private static string GetStringInSection(string value, string section)
        {
            string sectionName = String.Format(SectionFormat, section);
            int location = value.IndexOf(sectionName);
            if (location < 0)
            {
                return null;
            }

            string subString = value.Substring(location + sectionName.Length + 1);
            location = subString.IndexOf("##[");
            if (location < 0)
            {
                return null;
            }

            return subString.Substring(0, location).Trim('\n', ' ');
        }
    }
}
