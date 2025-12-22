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
            this.lastAccessDateLabel = new System.Windows.Forms.Label();
            this.lastWriteDateLabel = new System.Windows.Forms.Label();
            this.creationDateLabel = new System.Windows.Forms.Label();
            this.restoreDateButton = new System.Windows.Forms.Button();
            this.exchangeDateButton = new System.Windows.Forms.Button();
            this.setToNowButton = new System.Windows.Forms.Button();
            this.lastAccessDateCheckBox = new System.Windows.Forms.CheckBox();
            this.lastWriteDateCheckBox = new System.Windows.Forms.CheckBox();
            this.creationDateCheckBox = new System.Windows.Forms.CheckBox();
            this.lastAccessDatePicker = new Extension.Forms.DateTimePickerExtended();
            this.lastWriteDatePicker = new Extension.Forms.DateTimePickerExtended();
            this.creationDatePicker = new Extension.Forms.DateTimePickerExtended();
            this.SuspendLayout();
            // 
            // accessDateLabel
            // 
            this.lastAccessDateLabel.AutoSize = true;
            this.lastAccessDateLabel.Location = new System.Drawing.Point(35, 35);
            this.lastAccessDateLabel.Margin = new System.Windows.Forms.Padding(50, 50, 15, 15);
            this.lastAccessDateLabel.Name = "accessDateLabel";
            this.lastAccessDateLabel.Size = new System.Drawing.Size(134, 31);
            this.lastAccessDateLabel.TabIndex = 9;
			this.lastAccessDateLabel.Font = new System.Drawing.Font("Microsoft YaHei", 12F);
            this.lastAccessDateLabel.Text = "访问日期：";
            // 
            // modificationDateLabel
            // 
            this.lastWriteDateLabel.AutoSize = true;
            this.lastWriteDateLabel.Location = new System.Drawing.Point(35, 90);
            this.lastWriteDateLabel.Margin = new System.Windows.Forms.Padding(50, 15, 15, 15);
            this.lastWriteDateLabel.Name = "modificationDateLabel";
            this.lastWriteDateLabel.Size = new System.Drawing.Size(129, 31);
            this.lastWriteDateLabel.TabIndex = 10;
			this.lastWriteDateLabel.Font = new System.Drawing.Font("Microsoft YaHei", 12F);
            this.lastWriteDateLabel.Text = "Modified: ";
            // 
            // creationDateLabel
            // 
            this.creationDateLabel.AutoSize = true;
            this.creationDateLabel.Location = new System.Drawing.Point(35, 145);
            this.creationDateLabel.Margin = new System.Windows.Forms.Padding(50, 15, 15, 15);
            this.creationDateLabel.Name = "creationDateLabel";
            this.creationDateLabel.Size = new System.Drawing.Size(117, 31);
            this.creationDateLabel.TabIndex = 11;
			this.creationDateLabel.Font = new System.Drawing.Font("Microsoft YaHei", 12F);
            this.creationDateLabel.Text = "Created: ";
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
			this.restoreDateButton.Text = "Restore";
			this.restoreDateButton.Font = new System.Drawing.Font("Microsoft YaHei", 11F);
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
			this.exchangeDateButton.Font = new System.Drawing.Font("Microsoft YaHei", 11F);
            this.exchangeDateButton.Text = "Exchange";
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
			this.setToNowButton.Font = new System.Drawing.Font("Microsoft YaHei", 12F);
			this.setToNowButton.Text = "设为现在";
            // 
            // accessDateCheckBox
            // 
            this.lastAccessDateCheckBox.AutoSize = true;
            this.lastAccessDateCheckBox.Location = new System.Drawing.Point(40, 200);
            this.lastAccessDateCheckBox.Margin = new System.Windows.Forms.Padding(0, 15, 0, 15);
            this.lastAccessDateCheckBox.Name = "accessDateCheckBox";
            this.lastAccessDateCheckBox.Size = new System.Drawing.Size(127, 34);
            this.lastAccessDateCheckBox.TabIndex = 3;
            this.lastAccessDateCheckBox.UseVisualStyleBackColor = true;
            this.lastAccessDateCheckBox.CheckedChanged += new System.EventHandler(this.CheckBoxOnCheckedChanged);
			this.lastAccessDateCheckBox.Font = new System.Drawing.Font("Microsoft YaHei", 11F);
			this.lastAccessDateCheckBox.Text = "访问日期";
            // 
            // modificationDateCheckBox
            // 
            this.lastWriteDateCheckBox.AutoSize = true;
            this.lastWriteDateCheckBox.Location = new System.Drawing.Point(195, 200);
            this.lastWriteDateCheckBox.Margin = new System.Windows.Forms.Padding(0, 15, 0, 15);
            this.lastWriteDateCheckBox.Name = "modificationDateCheckBox";
            this.lastWriteDateCheckBox.Size = new System.Drawing.Size(125, 31);
            this.lastWriteDateCheckBox.TabIndex = 4;
            this.lastWriteDateCheckBox.UseVisualStyleBackColor = true;
            this.lastWriteDateCheckBox.CheckedChanged += new System.EventHandler(this.CheckBoxOnCheckedChanged);
			this.lastWriteDateCheckBox.Font = new System.Drawing.Font("Microsoft YaHei", 10F);
			this.lastWriteDateCheckBox.Text = "Modified";
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
			this.creationDateCheckBox.Font = new System.Drawing.Font("Microsoft YaHei", 10F);
			this.creationDateCheckBox.Text = "Created";
            // 
            // accessDatePicker
            // 
            this.lastAccessDatePicker.CalendarFont = new System.Drawing.Font("Microsoft YaHei", 10F);
            this.lastAccessDatePicker.CustomFormat = " yyyy/M/d   H:mm:ss";
            this.lastAccessDatePicker.Font = new System.Drawing.Font("Microsoft YaHei", 10F);
            this.lastAccessDatePicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.lastAccessDatePicker.Location = new System.Drawing.Point(180, 35);
            this.lastAccessDatePicker.Margin = new System.Windows.Forms.Padding(15, 50, 50, 15);
            this.lastAccessDatePicker.Name = "accessDatePicker";
            this.lastAccessDatePicker.Size = new System.Drawing.Size(295, 34);
            this.lastAccessDatePicker.TabIndex = 0;
            this.lastAccessDatePicker.ValueChanged += new System.EventHandler(this.DateTimePickerOnValueChanged);
            // 
            // modificationDatePicker
            // 
            this.lastWriteDatePicker.CustomFormat = " yyyy/M/d   H:mm:ss";
            this.lastWriteDatePicker.Font = new System.Drawing.Font("Microsoft YaHei", 10F);
            this.lastWriteDatePicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.lastWriteDatePicker.Location = new System.Drawing.Point(180, 90);
            this.lastWriteDatePicker.Margin = new System.Windows.Forms.Padding(15, 15, 50, 15);
            this.lastWriteDatePicker.Name = "modificationDatePicker";
            this.lastWriteDatePicker.Size = new System.Drawing.Size(295, 34);
            this.lastWriteDatePicker.TabIndex = 1;
            this.lastWriteDatePicker.ValueChanged += new System.EventHandler(this.DateTimePickerOnValueChanged);
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
            this.Controls.Add(this.lastAccessDateCheckBox);
            this.Controls.Add(this.creationDatePicker);
            this.Controls.Add(this.setToNowButton);
            this.Controls.Add(this.lastWriteDatePicker);
            this.Controls.Add(this.lastWriteDateCheckBox);
            this.Controls.Add(this.lastAccessDatePicker);
            this.Controls.Add(this.creationDateCheckBox);
            this.Controls.Add(this.creationDateLabel);
            this.Controls.Add(this.lastWriteDateLabel);
            this.Controls.Add(this.lastAccessDateLabel);
            this.Name = "EntryDateInformationPropertyPage";
            this.PageTitle = "File Dates";
            this.Size = new System.Drawing.Size(511, 628);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private Label lastAccessDateLabel;
        private Label lastWriteDateLabel;
        private Label creationDateLabel;
        protected Button restoreDateButton;
        protected Button exchangeDateButton;
        protected Button setToNowButton;
        protected CheckBox lastAccessDateCheckBox;
        protected CheckBox lastWriteDateCheckBox;
        protected CheckBox creationDateCheckBox;
        protected DateTimePickerExtended lastAccessDatePicker;
        protected DateTimePickerExtended lastWriteDatePicker;
        protected DateTimePickerExtended creationDatePicker;
    }
}
