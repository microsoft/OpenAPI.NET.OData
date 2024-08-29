// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Windows.Forms;
using Microsoft.OpenApi;
using System.IO;
using Microsoft.OData.Edm.Csdl;
using System.Xml.Linq;
using Microsoft.OData.Edm;
using System.Collections.Generic;
using Microsoft.OData.Edm.Validation;
using System.Text;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData;
using System.Xml;
using System.Threading.Tasks;
using System.Net.Http;

namespace OoasGui
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public partial class MainForm : Form
    {
        private OpenApiFormat Format { get; set; } = OpenApiFormat.Json;

        private OpenApiSpecVersion Version { get; set; } = OpenApiSpecVersion.OpenApi3_0;

        private OpenApiConvertSettings Settings = new OpenApiConvertSettings();

        private IEdmModel EdmModel { get; set; }

        private OpenApiDocument _document;

        public MainForm()
        {
            InitializeComponent();

            saveBtn.Enabled = false;
            jsonRadioBtn.Checked = true;
            v3RadioButton.Checked = true;
            fromFileRadioBtn.Checked = true;
            urlTextBox.Text = "http://services.odata.org/TrippinRESTierService";
            loadBtn.Enabled = false;
            operationIdcheckBox.Checked = true;
            Settings.EnableOperationId = true;

            verifyEdmModelcheckBox.Checked = true;
            Settings.VerifyEdmModel = true;

            csdlRichTextBox.WordWrap = false;
            oasRichTextBox.WordWrap = false;
        }
#pragma warning disable VSTHRD100
        private async void jsonRadioBtn_CheckedChanged(object sender, EventArgs e)
#pragma warning restore VSTHRD100
        {
            Format = OpenApiFormat.Json;
            await ConvertAsync();
        }

#pragma warning disable VSTHRD100
        private async void yamlRadioBtn_CheckedChanged(object sender, EventArgs e)
#pragma warning restore VSTHRD100
        {
            Format = OpenApiFormat.Yaml;
            await ConvertAsync();
        }

#pragma warning disable VSTHRD100
        private async void v2RadioBtn_CheckedChanged(object sender, EventArgs e)
#pragma warning restore VSTHRD100
        {
            Settings.OpenApiSpecVersion = Version = OpenApiSpecVersion.OpenApi2_0;
            await ConvertAsync();
        }

#pragma warning disable VSTHRD100
        private async void v3RadioBtn_CheckedChanged(object sender, EventArgs e)
#pragma warning restore VSTHRD100
        {
            Settings.OpenApiSpecVersion = Version = OpenApiSpecVersion.OpenApi3_0;
            await ConvertAsync();
        }

        private void fromFileRadioBtn_CheckedChanged(object sender, EventArgs e)
        {
            fileTextBox.Enabled = true;
            btnBrowse.Enabled = true;
            urlTextBox.Enabled = false;
            loadBtn.Enabled = false;
        }

        private void fromUrlRadioBtn_CheckedChanged(object sender, EventArgs e)
        {
            fileTextBox.Enabled = false;
            btnBrowse.Enabled = false;

            urlTextBox.Enabled = true;
            loadBtn.Enabled = true;
        }

#pragma warning disable VSTHRD100
        private async void btnBrowse_Click(object sender, EventArgs e)
#pragma warning restore VSTHRD100
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CSDL files (*.xml)|*.xml|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string text = File.ReadAllText(openFileDialog.FileName);

                    LoadEdm(openFileDialog.FileName, text);

                    fileTextBox.Text = openFileDialog.FileName;
                    csdlRichTextBox.Text = text;
                    Settings.ServiceRoot = new Uri(openFileDialog.FileName);
                    await ConvertAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not read file. Error: " + ex.Message);

                    csdlRichTextBox.Text = "";
                    EdmModel = null;
                    oasRichTextBox.Text = "";
                }
            }
        }

        private static HttpClient client = new();

#pragma warning disable VSTHRD100
        private async void loadBtn_Click(object sender, EventArgs e)
#pragma warning restore VSTHRD100
        {
            string url = urlTextBox.Text;

            try
            {
                Uri requestUri;
                if (url.EndsWith("/$metadata"))
                {
                    requestUri = new Uri(url);
                }
                else
                {
                    requestUri = new Uri(url + "/$metadata");
                }

                HttpResponseMessage response = await client.GetAsync(requestUri);

                string csdl = await response.Content.ReadAsStringAsync();

                LoadEdm(url, csdl);
                csdlRichTextBox.Text = FormatXml(csdl);
                Settings.ServiceRoot = requestUri;
                await ConvertAsync();
            }
            catch(Exception ex)
            {
                MessageBox.Show("Cannot load the metadata from " + url + "\nError: " + ex.Message);

                csdlRichTextBox.Text = "";
                EdmModel = null;
                oasRichTextBox.Text = "";
            }
        }

        private void LoadEdm(string resource, string text)
        {
            IEdmModel model;
            IEnumerable<EdmError> errors;

            if (!CsdlReader.TryParse(XElement.Parse(text).CreateReader(), out model, out errors))
            {
                StringBuilder sb = new StringBuilder();
                foreach (EdmError error in errors)
                {
                    sb.Append(error.ErrorMessage).Append("\n");
                }

                MessageBox.Show("Parse CSDL from " + resource + " failed. Error: " + sb.ToString());
                return;
            }

            EdmModel = model;
        }

        private async Task ConvertAsync()
        {
            saveBtn.Enabled = false;

            if (EdmModel == null)
            {
                return;
            }

            string openApi = null;
            await Task.Run(() =>
            {
                _document = EdmModel.ConvertToOpenApi(Settings);
                MemoryStream stream = new MemoryStream();
                _document.Serialize(stream, Version, Format);
                stream.Flush();
                stream.Position = 0;
                openApi = new StreamReader(stream).ReadToEnd();
            });

            oasRichTextBox.Text = openApi;
            saveBtn.Enabled = true;
        }

        private string FormatXml(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            StringWriter sw = new StringWriter();
            using (XmlTextWriter writer = new XmlTextWriter(sw))
            {
                writer.Indentation = 2;  // the Indentation
                writer.Formatting = Formatting.Indented;
                doc.WriteContentTo(writer);
                writer.Close();
            }

            return sw.ToString();
        }

#pragma warning disable VSTHRD100
        private async void saveBtn_Click(object sender, EventArgs e)
#pragma warning restore VSTHRD100
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (Format == OpenApiFormat.Json)
            {
                saveFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
            }
            else
            {
                saveFileDialog.Filter = "YAML files (*.yaml)|*.yaml|All files (*.*)|*.*";
            }

            saveFileDialog.RestoreDirectory = true;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string output = saveFileDialog.FileName;
                using (FileStream fs = File.Create(output))
                {
                    await Task.Run(() =>
                    {
                        _document?.Serialize(fs, Version, Format);
                        fs.Flush();
                    });
                }

                MessageBox.Show("Saved successfully!");
            }
        }

#pragma warning disable VSTHRD100
        private async void operationIdcheckBox_CheckedChanged(object sender, EventArgs e)
#pragma warning restore VSTHRD100
        {
            Settings.EnableOperationId = !Settings.EnableOperationId;
            await ConvertAsync ();
        }

#pragma warning disable VSTHRD100
        private async void VerifyEdmModelcheckBox_CheckedChanged(object sender, EventArgs e)
#pragma warning restore VSTHRD100
        {
            Settings.VerifyEdmModel = !Settings.VerifyEdmModel;
            await ConvertAsync();
        }

#pragma warning disable VSTHRD100
        private async void NavPathcheckBox_CheckedChanged(object sender, EventArgs e)
#pragma warning restore VSTHRD100
        {
            Settings.EnableNavigationPropertyPath = !Settings.EnableNavigationPropertyPath;
            await ConvertAsync();
        }
    }
}
