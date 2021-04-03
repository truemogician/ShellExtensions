using System.Windows.Forms;
using System.Drawing;
using Extension.Forms;
namespace FileDateSetter {
    partial class FileDateInformationPropertyPage {
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
            this.lastAccessLabel = new System.Windows.Forms.Label();
            this.lastWriteLabel = new System.Windows.Forms.Label();
            this.creationLabel = new System.Windows.Forms.Label();
            this.recoverDateButton = new System.Windows.Forms.Button();
            this.exchangeDateButton = new System.Windows.Forms.Button();
            this.setToNowButton = new System.Windows.Forms.Button();
            this.lastAccessCheckBox = new System.Windows.Forms.CheckBox();
            this.lastWriteCheckBox = new System.Windows.Forms.CheckBox();
            this.creationCheckBox = new System.Windows.Forms.CheckBox();
            this.lastAccessDateTimePicker = new Extension.Forms.DateTimePickerExtended();
            this.lastWriteDateTimePicker = new Extension.Forms.DateTimePickerExtended();
            this.creationDateTimePicker = new Extension.Forms.DateTimePickerExtended();
            this.SuspendLayout();
            // 
            // lastAccessLabel
            // 
            this.lastAccessLabel.AutoSize = true;
            this.lastAccessLabel.Font = new System.Drawing.Font("DengXian", 15F);
            this.lastAccessLabel.Location = new System.Drawing.Point(50, 50);
            this.lastAccessLabel.Margin = new System.Windows.Forms.Padding(50, 50, 15, 15);
            this.lastAccessLabel.Name = "lastAccessLabel";
            this.lastAccessLabel.Size = new System.Drawing.Size(137, 26);
            this.lastAccessLabel.TabIndex = 0;
            this.lastAccessLabel.Text = "访问日期：";
            // 
            // lastWriteLabel
            // 
            this.lastWriteLabel.AutoSize = true;
            this.lastWriteLabel.Font = new System.Drawing.Font("DengXian", 15F);
            this.lastWriteLabel.Location = new System.Drawing.Point(50, 105);
            this.lastWriteLabel.Margin = new System.Windows.Forms.Padding(50, 15, 15, 15);
            this.lastWriteLabel.Name = "lastWriteLabel";
            this.lastWriteLabel.Size = new System.Drawing.Size(137, 26);
            this.lastWriteLabel.TabIndex = 3;
            this.lastWriteLabel.Text = "修改日期：";
            // 
            // creationLabel
            // 
            this.creationLabel.AutoSize = true;
            this.creationLabel.Font = new System.Drawing.Font("DengXian", 15F);
            this.creationLabel.Location = new System.Drawing.Point(50, 160);
            this.creationLabel.Margin = new System.Windows.Forms.Padding(50, 15, 15, 15);
            this.creationLabel.Name = "creationLabel";
            this.creationLabel.Size = new System.Drawing.Size(137, 26);
            this.creationLabel.TabIndex = 6;
            this.creationLabel.Text = "创建日期：";
            // 
            // recoverDateButton
            // 
            this.recoverDateButton.Enabled = false;
            this.recoverDateButton.Font = new System.Drawing.Font("KaiTi", 15F);
            this.recoverDateButton.Location = new System.Drawing.Point(336, 273);
            this.recoverDateButton.Margin = new System.Windows.Forms.Padding(0, 15, 0, 0);
            this.recoverDateButton.Name = "recoverDateButton";
            this.recoverDateButton.Size = new System.Drawing.Size(125, 50);
            this.recoverDateButton.TabIndex = 5;
            this.recoverDateButton.Text = "恢复原值";
            this.recoverDateButton.UseVisualStyleBackColor = true;
            this.recoverDateButton.Click += new System.EventHandler(this.RecoverDateButtonOnClick);
            // 
            // exchangeDateButton
            // 
            this.exchangeDateButton.Enabled = false;
            this.exchangeDateButton.Font = new System.Drawing.Font("KaiTi", 15F);
            this.exchangeDateButton.Location = new System.Drawing.Point(195, 274);
            this.exchangeDateButton.Margin = new System.Windows.Forms.Padding(0, 15, 0, 0);
            this.exchangeDateButton.Name = "exchangeDateButton";
            this.exchangeDateButton.Size = new System.Drawing.Size(125, 50);
            this.exchangeDateButton.TabIndex = 4;
            this.exchangeDateButton.Text = "交换日期";
            this.exchangeDateButton.UseVisualStyleBackColor = true;
            this.exchangeDateButton.Click += new System.EventHandler(this.ExchangeDateButtonOnClick);
            // 
            // setToNowButton
            // 
            this.setToNowButton.Font = new System.Drawing.Font("KaiTi", 15F);
            this.setToNowButton.Location = new System.Drawing.Point(55, 273);
            this.setToNowButton.Margin = new System.Windows.Forms.Padding(0, 15, 0, 0);
            this.setToNowButton.Name = "setToNowButton";
            this.setToNowButton.Size = new System.Drawing.Size(125, 50);
            this.setToNowButton.TabIndex = 3;
            this.setToNowButton.Text = "设为现在";
            this.setToNowButton.UseVisualStyleBackColor = true;
            this.setToNowButton.Click += new System.EventHandler(this.SetToNowButtonOnClick);
            // 
            // lastAccessCheckBox
            // 
            this.lastAccessCheckBox.AutoSize = true;
            this.lastAccessCheckBox.Font = new System.Drawing.Font("DengXian", 15F);
            this.lastAccessCheckBox.Location = new System.Drawing.Point(55, 215);
            this.lastAccessCheckBox.Margin = new System.Windows.Forms.Padding(0, 15, 0, 15);
            this.lastAccessCheckBox.Name = "lastAccessCheckBox";
            this.lastAccessCheckBox.Size = new System.Drawing.Size(134, 30);
            this.lastAccessCheckBox.TabIndex = 0;
            this.lastAccessCheckBox.Text = "访问日期";
            this.lastAccessCheckBox.UseVisualStyleBackColor = true;
            this.lastAccessCheckBox.CheckedChanged += new System.EventHandler(this.CheckBoxOnCheckedChanged);
            // 
            // lastWriteCheckBox
            // 
            this.lastWriteCheckBox.AutoSize = true;
            this.lastWriteCheckBox.Font = new System.Drawing.Font("DengXian", 15F);
            this.lastWriteCheckBox.Location = new System.Drawing.Point(195, 215);
            this.lastWriteCheckBox.Margin = new System.Windows.Forms.Padding(0, 15, 0, 15);
            this.lastWriteCheckBox.Name = "lastWriteCheckBox";
            this.lastWriteCheckBox.Size = new System.Drawing.Size(134, 30);
            this.lastWriteCheckBox.TabIndex = 1;
            this.lastWriteCheckBox.Text = "修改日期";
            this.lastWriteCheckBox.UseVisualStyleBackColor = true;
            this.lastWriteCheckBox.CheckedChanged += new System.EventHandler(this.CheckBoxOnCheckedChanged);
            // 
            // creationCheckBox
            // 
            this.creationCheckBox.AutoSize = true;
            this.creationCheckBox.Font = new System.Drawing.Font("DengXian", 15F);
            this.creationCheckBox.Location = new System.Drawing.Point(334, 215);
            this.creationCheckBox.Margin = new System.Windows.Forms.Padding(0, 15, 0, 15);
            this.creationCheckBox.Name = "creationCheckBox";
            this.creationCheckBox.Size = new System.Drawing.Size(134, 30);
            this.creationCheckBox.TabIndex = 2;
            this.creationCheckBox.Text = "创建日期";
            this.creationCheckBox.UseVisualStyleBackColor = true;
            this.creationCheckBox.CheckedChanged += new System.EventHandler(this.CheckBoxOnCheckedChanged);
            // 
            // lastAccessDateTimePicker
            // 
            this.lastAccessDateTimePicker.CustomFormat = " yyyy/M/d   H:mm:ss";
            this.lastAccessDateTimePicker.Font = new System.Drawing.Font("DengXian", 14F);
            this.lastAccessDateTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.lastAccessDateTimePicker.Location = new System.Drawing.Point(195, 44);
            this.lastAccessDateTimePicker.Margin = new System.Windows.Forms.Padding(15, 50, 50, 15);
            this.lastAccessDateTimePicker.Name = "lastAccessDateTimePicker";
            this.lastAccessDateTimePicker.Size = new System.Drawing.Size(266, 32);
            this.lastAccessDateTimePicker.TabIndex = 10;
            this.lastAccessDateTimePicker.ValueChanged += new System.EventHandler(this.DateTimePickerOnValueChanged);
            // 
            // lastWriteDateTimePicker
            // 
            this.lastWriteDateTimePicker.CustomFormat = " yyyy/M/d   H:mm:ss";
            this.lastWriteDateTimePicker.Font = new System.Drawing.Font("DengXian", 14F);
            this.lastWriteDateTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.lastWriteDateTimePicker.Location = new System.Drawing.Point(195, 99);
            this.lastWriteDateTimePicker.Margin = new System.Windows.Forms.Padding(15, 15, 50, 15);
            this.lastWriteDateTimePicker.Name = "lastWriteDateTimePicker";
            this.lastWriteDateTimePicker.Size = new System.Drawing.Size(266, 32);
            this.lastWriteDateTimePicker.TabIndex = 11;
            this.lastWriteDateTimePicker.ValueChanged += new System.EventHandler(this.DateTimePickerOnValueChanged);
            // 
            // creationDateTimePicker
            // 
            this.creationDateTimePicker.CustomFormat = " yyyy/M/d   H:mm:ss";
            this.creationDateTimePicker.Font = new System.Drawing.Font("DengXian", 14F);
            this.creationDateTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.creationDateTimePicker.Location = new System.Drawing.Point(195, 154);
            this.creationDateTimePicker.Margin = new System.Windows.Forms.Padding(15, 15, 50, 15);
            this.creationDateTimePicker.Name = "creationDateTimePicker";
            this.creationDateTimePicker.Size = new System.Drawing.Size(266, 32);
            this.creationDateTimePicker.TabIndex = 12;
            this.creationDateTimePicker.ValueChanged += new System.EventHandler(this.DateTimePickerOnValueChanged);
            // 
            // FileDateInformationPropertyPage
            // 
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.recoverDateButton);
            this.Controls.Add(this.exchangeDateButton);
            this.Controls.Add(this.lastAccessCheckBox);
            this.Controls.Add(this.creationDateTimePicker);
            this.Controls.Add(this.setToNowButton);
            this.Controls.Add(this.lastWriteDateTimePicker);
            this.Controls.Add(this.lastWriteCheckBox);
            this.Controls.Add(this.lastAccessDateTimePicker);
            this.Controls.Add(this.creationCheckBox);
            this.Controls.Add(this.creationLabel);
            this.Controls.Add(this.lastWriteLabel);
            this.Controls.Add(this.lastAccessLabel);
            this.Name = "FileDateInformationPropertyPage";
            this.PageTitle = "文件日期";
            this.Size = new System.Drawing.Size(511, 628);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Label lastAccessLabel;
        private Label lastWriteLabel;
        private Label creationLabel;
        protected Button recoverDateButton;
        protected Button exchangeDateButton;
        protected Button setToNowButton;
        protected CheckBox lastAccessCheckBox;
        protected CheckBox lastWriteCheckBox;
        protected CheckBox creationCheckBox;
        protected DateTimePickerExtended lastAccessDateTimePicker;
        protected DateTimePickerExtended lastWriteDateTimePicker;
        protected DateTimePickerExtended creationDateTimePicker;
    }
}
