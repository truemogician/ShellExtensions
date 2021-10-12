
namespace BatchRenamer.Windows {
	partial class MainForm {
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
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.SearchTextBox = new System.Windows.Forms.TextBox();
			this.ReplaceTextBox = new System.Windows.Forms.TextBox();
			this.richTextBox1 = new System.Windows.Forms.RichTextBox();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft YaHei", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.label1.Location = new System.Drawing.Point(29, 29);
			this.label1.Margin = new System.Windows.Forms.Padding(20);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(132, 27);
			this.label1.TabIndex = 0;
			this.label1.Text = "搜索表达式：";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Microsoft YaHei", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.label2.Location = new System.Drawing.Point(29, 81);
			this.label2.Margin = new System.Windows.Forms.Padding(20);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(132, 27);
			this.label2.TabIndex = 1;
			this.label2.Text = "替换表达式：";
			// 
			// SearchTextBox
			// 
			this.SearchTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.SearchTextBox.Font = new System.Drawing.Font("Fira Code", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.SearchTextBox.Location = new System.Drawing.Point(152, 29);
			this.SearchTextBox.Margin = new System.Windows.Forms.Padding(20);
			this.SearchTextBox.Name = "SearchTextBox";
			this.SearchTextBox.Size = new System.Drawing.Size(303, 32);
			this.SearchTextBox.TabIndex = 2;
			this.SearchTextBox.TextChanged += new System.EventHandler(this.SearchTextBoxTextChanged);
			// 
			// ReplaceTextBox
			// 
			this.ReplaceTextBox.Font = new System.Drawing.Font("Fira Code", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ReplaceTextBox.Location = new System.Drawing.Point(152, 81);
			this.ReplaceTextBox.Margin = new System.Windows.Forms.Padding(20);
			this.ReplaceTextBox.Name = "ReplaceTextBox";
			this.ReplaceTextBox.Size = new System.Drawing.Size(303, 32);
			this.ReplaceTextBox.TabIndex = 3;
			this.ReplaceTextBox.TextChanged += new System.EventHandler(this.SearchTextBoxTextChanged);
			this.ReplaceTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ReplaceTextBoxKeyPress);
			// 
			// richTextBox1
			// 
			this.richTextBox1.Location = new System.Drawing.Point(34, 148);
			this.richTextBox1.Margin = new System.Windows.Forms.Padding(20);
			this.richTextBox1.Name = "richTextBox1";
			this.richTextBox1.Size = new System.Drawing.Size(421, 249);
			this.richTextBox1.TabIndex = 4;
			this.richTextBox1.Text = "";
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(484, 426);
			this.Controls.Add(this.richTextBox1);
			this.Controls.Add(this.ReplaceTextBox);
			this.Controls.Add(this.SearchTextBox);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "MainForm";
			this.Text = "批量重命名";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox SearchTextBox;
		private System.Windows.Forms.TextBox ReplaceTextBox;
		private System.Windows.Forms.RichTextBox richTextBox1;
	}
}