using System.Windows.Forms;
using Extension.Forms;

namespace EntryDateSetter {
    partial class EntryDateInformationPropertyPage {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.accessDateLabel = new System.Windows.Forms.Label();
            this.modificationDateLabel = new System.Windows.Forms.Label();
            this.creationDateLabel = new System.Windows.Forms.Label();
            this.restoreDateButton = new System.Windows.Forms.Button();
            this.exchangeDateButton = new System.Windows.Forms.Button();
            this.setToNowButton = new System.Windows.Forms.Button();
            this.accessDateCheckBox = new System.Windows.Forms.CheckBox();
            this.modificationDateCheckBox = new System.Windows.Forms.CheckBox();
            this.creationDateCheckBox = new System.Windows.Forms.CheckBox();
            this.accessDatePicker = new Extension.Forms.DateTimePickerExtended();
            this.modificationDatePicker = new Extension.Forms.DateTimePickerExtended();
            this.creationDatePicker = new Extension.Forms.DateTimePickerExtended();
            this.SuspendLayout();
            // 
            // accessDateLabel
            // 
            this.accessDateLabel.AutoSize = true;
            this.accessDateLabel.Location = new System.Drawing.Point(35, 35);
            this.accessDateLabel.Margin = new System.Windows.Forms.Padding(50, 50, 15, 15);
            this.accessDateLabel.Name = "accessDateLabel";
            this.accessDateLabel.Size = new System.Drawing.Size(134, 31);
            this.accessDateLabel.TabIndex = 9;
            // 
            // modificationDateLabel
            // 
            this.modificationDateLabel.AutoSize = true;
            this.modificationDateLabel.Location = new System.Drawing.Point(35, 90);
            this.modificationDateLabel.Margin = new System.Windows.Forms.Padding(50, 15, 15, 15);
            this.modificationDateLabel.Name = "modificationDateLabel";
            this.modificationDateLabel.Size = new System.Drawing.Size(129, 31);
            this.modificationDateLabel.TabIndex = 10;
            // 
            // creationDateLabel
            // 
            this.creationDateLabel.AutoSize = true;
            this.creationDateLabel.Location = new System.Drawing.Point(35, 145);
            this.creationDateLabel.Margin = new System.Windows.Forms.Padding(50, 15, 15, 15);
            this.creationDateLabel.Name = "creationDateLabel";
            this.creationDateLabel.Size = new System.Drawing.Size(117, 31);
            this.creationDateLabel.TabIndex = 11;
            // 
            // restoreDateButton
            // 
            this.restoreDateButton.Enabled = false;
            this.restoreDateButton.Location = new System.Drawing.Point(350, 260);
            this.restoreDateButton.Margin = new System.Windows.Forms.Padding(0, 15, 0, 0);
            this.restoreDateButton.Name = "restoreDateButton";
            this.restoreDateButton.Size = new System.Drawing.Size(125, 50);
            this.restoreDateButton.TabIndex = 8;
            this.restoreDateButton.UseVisualStyleBackColor = true;
            this.restoreDateButton.Click += new System.EventHandler(this.RecoverDateButtonOnClick);
            // 
            // exchangeDateButton
            // 
            this.exchangeDateButton.Enabled = false;
            this.exchangeDateButton.Location = new System.Drawing.Point(195, 260);
            this.exchangeDateButton.Margin = new System.Windows.Forms.Padding(0, 15, 0, 0);
            this.exchangeDateButton.Name = "exchangeDateButton";
            this.exchangeDateButton.Size = new System.Drawing.Size(125, 50);
            this.exchangeDateButton.TabIndex = 7;
            this.exchangeDateButton.UseVisualStyleBackColor = true;
            this.exchangeDateButton.Click += new System.EventHandler(this.ExchangeDateButtonOnClick);
            // 
            // setToNowButton
            // 
            this.setToNowButton.Location = new System.Drawing.Point(40, 260);
            this.setToNowButton.Margin = new System.Windows.Forms.Padding(0, 15, 0, 0);
            this.setToNowButton.Name = "setToNowButton";
            this.setToNowButton.Size = new System.Drawing.Size(125, 50);
            this.setToNowButton.TabIndex = 6;
            this.setToNowButton.UseVisualStyleBackColor = true;
            this.setToNowButton.Click += new System.EventHandler(this.SetToNowButtonOnClick);
            // 
            // accessDateCheckBox
            // 
            this.accessDateCheckBox.AutoSize = true;
            this.accessDateCheckBox.Location = new System.Drawing.Point(40, 200);
            this.accessDateCheckBox.Margin = new System.Windows.Forms.Padding(0, 15, 0, 15);
            this.accessDateCheckBox.Name = "accessDateCheckBox";
            this.accessDateCheckBox.Size = new System.Drawing.Size(127, 34);
            this.accessDateCheckBox.TabIndex = 3;
            this.accessDateCheckBox.UseVisualStyleBackColor = true;
            this.accessDateCheckBox.CheckedChanged += new System.EventHandler(this.CheckBoxOnCheckedChanged);
            // 
            // modificationDateCheckBox
            // 
            this.modificationDateCheckBox.AutoSize = true;
            this.modificationDateCheckBox.Location = new System.Drawing.Point(195, 200);
            this.modificationDateCheckBox.Margin = new System.Windows.Forms.Padding(0, 15, 0, 15);
            this.modificationDateCheckBox.Name = "modificationDateCheckBox";
            this.modificationDateCheckBox.Size = new System.Drawing.Size(125, 31);
            this.modificationDateCheckBox.TabIndex = 4;
            this.modificationDateCheckBox.UseVisualStyleBackColor = true;
            this.modificationDateCheckBox.CheckedChanged += new System.EventHandler(this.CheckBoxOnCheckedChanged);
            // 
            // creationDateCheckBox
            // 
            this.creationDateCheckBox.AutoSize = true;
            this.creationDateCheckBox.Location = new System.Drawing.Point(350, 200);
            this.creationDateCheckBox.Margin = new System.Windows.Forms.Padding(0, 15, 0, 15);
            this.creationDateCheckBox.Name = "creationDateCheckBox";
            this.creationDateCheckBox.Size = new System.Drawing.Size(112, 31);
            this.creationDateCheckBox.TabIndex = 5;
            this.creationDateCheckBox.UseVisualStyleBackColor = true;
            this.creationDateCheckBox.CheckedChanged += new System.EventHandler(this.CheckBoxOnCheckedChanged);
            // 
            // accessDatePicker
            // 
            this.accessDatePicker.CalendarFont = new System.Drawing.Font("Microsoft YaHei", 10F);
            this.accessDatePicker.CustomFormat = " yyyy/M/d   H:mm:ss";
            this.accessDatePicker.Font = new System.Drawing.Font("Microsoft YaHei", 10F);
            this.accessDatePicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.accessDatePicker.Location = new System.Drawing.Point(180, 35);
            this.accessDatePicker.Margin = new System.Windows.Forms.Padding(15, 50, 50, 15);
            this.accessDatePicker.Name = "accessDatePicker";
            this.accessDatePicker.Size = new System.Drawing.Size(295, 34);
            this.accessDatePicker.TabIndex = 0;
            this.accessDatePicker.ValueChanged += new System.EventHandler(this.DateTimePickerOnValueChanged);
            // 
            // modificationDatePicker
            // 
            this.modificationDatePicker.CustomFormat = " yyyy/M/d   H:mm:ss";
            this.modificationDatePicker.Font = new System.Drawing.Font("Microsoft YaHei", 10F);
            this.modificationDatePicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.modificationDatePicker.Location = new System.Drawing.Point(180, 90);
            this.modificationDatePicker.Margin = new System.Windows.Forms.Padding(15, 15, 50, 15);
            this.modificationDatePicker.Name = "modificationDatePicker";
            this.modificationDatePicker.Size = new System.Drawing.Size(295, 34);
            this.modificationDatePicker.TabIndex = 1;
            this.modificationDatePicker.ValueChanged += new System.EventHandler(this.DateTimePickerOnValueChanged);
            // 
            // creationDatePicker
            // 
            this.creationDatePicker.CustomFormat = " yyyy/M/d   H:mm:ss";
            this.creationDatePicker.Font = new System.Drawing.Font("Microsoft YaHei", 10F);
            this.creationDatePicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.creationDatePicker.Location = new System.Drawing.Point(180, 145);
            this.creationDatePicker.Margin = new System.Windows.Forms.Padding(15, 15, 50, 15);
            this.creationDatePicker.Name = "creationDatePicker";
            this.creationDatePicker.Size = new System.Drawing.Size(295, 34);
            this.creationDatePicker.TabIndex = 2;
            this.creationDatePicker.ValueChanged += new System.EventHandler(this.DateTimePickerOnValueChanged);
            // 
            // EntryDateInformationPropertyPage
            // 
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.restoreDateButton);
            this.Controls.Add(this.exchangeDateButton);
            this.Controls.Add(this.accessDateCheckBox);
            this.Controls.Add(this.creationDatePicker);
            this.Controls.Add(this.setToNowButton);
            this.Controls.Add(this.modificationDatePicker);
            this.Controls.Add(this.modificationDateCheckBox);
            this.Controls.Add(this.accessDatePicker);
            this.Controls.Add(this.creationDateCheckBox);
            this.Controls.Add(this.creationDateLabel);
            this.Controls.Add(this.modificationDateLabel);
            this.Controls.Add(this.accessDateLabel);
            this.Name = "EntryDateInformationPropertyPage";
            this.Size = new System.Drawing.Size(511, 628);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private Label accessDateLabel;
        private Label modificationDateLabel;
        private Label creationDateLabel;
        protected Button restoreDateButton;
        protected Button exchangeDateButton;
        protected Button setToNowButton;
        protected CheckBox accessDateCheckBox;
        protected CheckBox modificationDateCheckBox;
        protected CheckBox creationDateCheckBox;
        protected DateTimePickerExtended accessDatePicker;
        protected DateTimePickerExtended modificationDatePicker;
        protected DateTimePickerExtended creationDatePicker;
    }
}
