namespace OoasGui
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.csdlRichTextBox = new System.Windows.Forms.RichTextBox();
            this.oasRichTextBox = new System.Windows.Forms.RichTextBox();
            this.saveBtn = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.yamlRadioBtn = new System.Windows.Forms.RadioButton();
            this.jsonRadioBtn = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.loadBtn = new System.Windows.Forms.Button();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.urlTextBox = new System.Windows.Forms.TextBox();
            this.fileTextBox = new System.Windows.Forms.TextBox();
            this.fromUrlRadioBtn = new System.Windows.Forms.RadioButton();
            this.fromFileRadioBtn = new System.Windows.Forms.RadioButton();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // csdlRichTextBox
            // 
            this.csdlRichTextBox.Location = new System.Drawing.Point(12, 23);
            this.csdlRichTextBox.Name = "csdlRichTextBox";
            this.csdlRichTextBox.Size = new System.Drawing.Size(435, 529);
            this.csdlRichTextBox.TabIndex = 1;
            this.csdlRichTextBox.Text = "";
            // 
            // oasRichTextBox
            // 
            this.oasRichTextBox.Location = new System.Drawing.Point(461, 23);
            this.oasRichTextBox.Name = "oasRichTextBox";
            this.oasRichTextBox.Size = new System.Drawing.Size(435, 529);
            this.oasRichTextBox.TabIndex = 1;
            this.oasRichTextBox.Text = "";
            // 
            // saveBtn
            // 
            this.saveBtn.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.saveBtn.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.saveBtn.Location = new System.Drawing.Point(17, 51);
            this.saveBtn.Name = "saveBtn";
            this.saveBtn.Size = new System.Drawing.Size(125, 27);
            this.saveBtn.TabIndex = 5;
            this.saveBtn.Text = "Save...";
            this.saveBtn.UseVisualStyleBackColor = false;
            this.saveBtn.Click += new System.EventHandler(this.saveBtn_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(639, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Open API v3.0";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(162, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "CSDL";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.saveBtn);
            this.groupBox2.Controls.Add(this.yamlRadioBtn);
            this.groupBox2.Controls.Add(this.jsonRadioBtn);
            this.groupBox2.Location = new System.Drawing.Point(744, 558);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(152, 93);
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            // 
            // yamlRadioBtn
            // 
            this.yamlRadioBtn.AutoSize = true;
            this.yamlRadioBtn.Location = new System.Drawing.Point(80, 23);
            this.yamlRadioBtn.Name = "yamlRadioBtn";
            this.yamlRadioBtn.Size = new System.Drawing.Size(54, 17);
            this.yamlRadioBtn.TabIndex = 0;
            this.yamlRadioBtn.TabStop = true;
            this.yamlRadioBtn.Text = "YAML";
            this.yamlRadioBtn.UseVisualStyleBackColor = true;
            this.yamlRadioBtn.CheckedChanged += new System.EventHandler(this.yamlRadioBtn_CheckedChanged);
            // 
            // jsonRadioBtn
            // 
            this.jsonRadioBtn.AutoSize = true;
            this.jsonRadioBtn.Location = new System.Drawing.Point(21, 23);
            this.jsonRadioBtn.Name = "jsonRadioBtn";
            this.jsonRadioBtn.Size = new System.Drawing.Size(53, 17);
            this.jsonRadioBtn.TabIndex = 0;
            this.jsonRadioBtn.TabStop = true;
            this.jsonRadioBtn.Text = "JSON";
            this.jsonRadioBtn.UseVisualStyleBackColor = true;
            this.jsonRadioBtn.CheckedChanged += new System.EventHandler(this.jsonRadioBtn_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.loadBtn);
            this.groupBox1.Controls.Add(this.btnBrowse);
            this.groupBox1.Controls.Add(this.urlTextBox);
            this.groupBox1.Controls.Add(this.fileTextBox);
            this.groupBox1.Controls.Add(this.fromUrlRadioBtn);
            this.groupBox1.Controls.Add(this.fromFileRadioBtn);
            this.groupBox1.Location = new System.Drawing.Point(12, 558);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(716, 93);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            // 
            // loadBtn
            // 
            this.loadBtn.Location = new System.Drawing.Point(635, 61);
            this.loadBtn.Name = "loadBtn";
            this.loadBtn.Size = new System.Drawing.Size(75, 23);
            this.loadBtn.TabIndex = 3;
            this.loadBtn.Text = "Load...";
            this.loadBtn.UseVisualStyleBackColor = true;
            this.loadBtn.Click += new System.EventHandler(this.loadBtn_Click);
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(635, 25);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnBrowse.TabIndex = 3;
            this.btnBrowse.Text = "Browse...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // urlTextBox
            // 
            this.urlTextBox.Location = new System.Drawing.Point(88, 63);
            this.urlTextBox.Name = "urlTextBox";
            this.urlTextBox.Size = new System.Drawing.Size(541, 20);
            this.urlTextBox.TabIndex = 2;
            // 
            // fileTextBox
            // 
            this.fileTextBox.Location = new System.Drawing.Point(88, 26);
            this.fileTextBox.Name = "fileTextBox";
            this.fileTextBox.Size = new System.Drawing.Size(541, 20);
            this.fileTextBox.TabIndex = 2;
            // 
            // fromUrlRadioBtn
            // 
            this.fromUrlRadioBtn.AutoSize = true;
            this.fromUrlRadioBtn.Location = new System.Drawing.Point(15, 62);
            this.fromUrlRadioBtn.Name = "fromUrlRadioBtn";
            this.fromUrlRadioBtn.Size = new System.Drawing.Size(64, 17);
            this.fromUrlRadioBtn.TabIndex = 1;
            this.fromUrlRadioBtn.TabStop = true;
            this.fromUrlRadioBtn.Text = "From Url";
            this.fromUrlRadioBtn.UseVisualStyleBackColor = true;
            this.fromUrlRadioBtn.CheckedChanged += new System.EventHandler(this.fromUrlRadioBtn_CheckedChanged);
            // 
            // fromFileRadioBtn
            // 
            this.fromFileRadioBtn.AutoSize = true;
            this.fromFileRadioBtn.Location = new System.Drawing.Point(15, 27);
            this.fromFileRadioBtn.Name = "fromFileRadioBtn";
            this.fromFileRadioBtn.Size = new System.Drawing.Size(67, 17);
            this.fromFileRadioBtn.TabIndex = 1;
            this.fromFileRadioBtn.TabStop = true;
            this.fromFileRadioBtn.Text = "From File";
            this.fromFileRadioBtn.UseVisualStyleBackColor = true;
            this.fromFileRadioBtn.CheckedChanged += new System.EventHandler(this.fromFileRadioBtn_CheckedChanged);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(910, 663);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.oasRichTextBox);
            this.Controls.Add(this.csdlRichTextBox);
            this.Name = "MainForm";
            this.Text = "OData CSDL to Open API v3.0";
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.RichTextBox csdlRichTextBox;
        private System.Windows.Forms.RichTextBox oasRichTextBox;
        private System.Windows.Forms.Button saveBtn;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton yamlRadioBtn;
        private System.Windows.Forms.RadioButton jsonRadioBtn;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button loadBtn;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.TextBox urlTextBox;
        private System.Windows.Forms.TextBox fileTextBox;
        private System.Windows.Forms.RadioButton fromUrlRadioBtn;
        private System.Windows.Forms.RadioButton fromFileRadioBtn;
    }
}

