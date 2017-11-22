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
using System.Net;
using System.Xml;

namespace OoasGui
{
    public partial class MainForm : Form
    {
        private OpenApiFormat Format { get; set; }
        private IEdmModel EdmModel { get; set; }

        public MainForm()
        {
            InitializeComponent();

            jsonRadioBtn.Checked = true;
            fromFileRadioBtn.Checked = true;
            urlTextBox.Text = "http://services.odata.org/TrippinRESTierService";
            loadBtn.Enabled = false;

            csdlRichTextBox.WordWrap = false;
            oasRichTextBox.WordWrap = false;
        }

        private void jsonRadioBtn_CheckedChanged(object sender, EventArgs e)
        {
            Format = OpenApiFormat.Json;
            Convert();
        }

        private void yamlRadioBtn_CheckedChanged(object sender, EventArgs e)
        {
            Format = OpenApiFormat.Yaml;
            Convert();
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

        private void btnBrowse_Click(object sender, EventArgs e)
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

                    Convert();
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

        private void loadBtn_Click(object sender, EventArgs e)
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

                WebRequest request = WebRequest.Create(requestUri);

                WebResponse response = request.GetResponse();

                Stream receivedStream = response.GetResponseStream();

                StreamReader reader = new StreamReader(receivedStream, Encoding.UTF8);

                string csdl = reader.ReadToEnd();
                LoadEdm(url, csdl);
                csdlRichTextBox.Text = FormatXml(csdl);
                Convert();
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

        private void Convert()
        {
            if (EdmModel == null)
            {
                return;
            }

            OpenApiDocument document = EdmModel.ConvertToOpenApi();
            MemoryStream stream = new MemoryStream();
            document.Serialize(stream, OpenApiSpecVersion.OpenApi3_0_0, Format);
            stream.Flush();
            stream.Position = 0;
            string openApi = new StreamReader(stream).ReadToEnd();
            oasRichTextBox.Text = openApi;
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

        private void saveBtn_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (Format == OpenApiFormat.Json)
            {
                saveFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
            }
            else
            {
                saveFileDialog.Filter = "YAML files (*.ymal)|*.json|All files (*.*)|*.*";
            }

            saveFileDialog.FilterIndex = 2;
            saveFileDialog.RestoreDirectory = true;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string output = saveFileDialog.FileName;
                using (FileStream fs = File.Create(output))
                {
                    OpenApiDocument document = EdmModel.ConvertToOpenApi();
                    document.Serialize(fs, OpenApiSpecVersion.OpenApi3_0_0, Format);
                    fs.Flush();
                }
            }

            MessageBox.Show("Saved successful!");
        }
    }
}
