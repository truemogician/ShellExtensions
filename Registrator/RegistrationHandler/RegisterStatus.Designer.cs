
namespace RegistrationHandler {
	partial class RegisterStatus {
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
			this.stdoutBox = new System.Windows.Forms.RichTextBox();
			this.stderrBox = new System.Windows.Forms.RichTextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.bar = new System.Windows.Forms.ProgressBar();
			this.SuspendLayout();
			// 
			// stdoutBox
			// 
			this.stdoutBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.stdoutBox.Font = new System.Drawing.Font("Microsoft YaHei", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.stdoutBox.Location = new System.Drawing.Point(25, 102);
			this.stdoutBox.Margin = new System.Windows.Forms.Padding(13, 12, 13, 12);
			this.stdoutBox.Name = "stdoutBox";
			this.stdoutBox.ReadOnly = true;
			this.stdoutBox.Size = new System.Drawing.Size(279, 224);
			this.stdoutBox.TabIndex = 1;
			this.stdoutBox.Text = "";
			this.stdoutBox.Visible = false;
			// 
			// stderrBox
			// 
			this.stderrBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.stderrBox.Font = new System.Drawing.Font("Microsoft YaHei", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.stderrBox.Location = new System.Drawing.Point(340, 102);
			this.stderrBox.Margin = new System.Windows.Forms.Padding(13, 12, 13, 12);
			this.stderrBox.Name = "stderrBox";
			this.stderrBox.ReadOnly = true;
			this.stderrBox.Size = new System.Drawing.Size(279, 224);
			this.stderrBox.TabIndex = 2;
			this.stderrBox.Text = "";
			this.stderrBox.Visible = false;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft YaHei", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.label1.Location = new System.Drawing.Point(111, 65);
			this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(112, 27);
			this.label1.TabIndex = 3;
			this.label1.Text = "标准输出流";
			this.label1.Visible = false;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Microsoft YaHei", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.label2.Location = new System.Drawing.Point(425, 65);
			this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(112, 27);
			this.label2.TabIndex = 4;
			this.label2.Text = "标准错误流";
			this.label2.Visible = false;
			// 
			// bar
			// 
			this.bar.Location = new System.Drawing.Point(19, 19);
			this.bar.Margin = new System.Windows.Forms.Padding(10);
			this.bar.MarqueeAnimationSpeed = 20;
			this.bar.Name = "bar";
			this.bar.Size = new System.Drawing.Size(341, 38);
			this.bar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
			this.bar.TabIndex = 5;
			// 
			// RegisterStatus
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(379, 76);
			this.Controls.Add(this.bar);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.stderrBox);
			this.Controls.Add(this.stdoutBox);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Margin = new System.Windows.Forms.Padding(4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "RegisterStatus";
			this.Text = "注册状态";
			this.Load += new System.EventHandler(this.RegisterStatusLoad);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.RichTextBox stdoutBox;
		private System.Windows.Forms.RichTextBox stderrBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ProgressBar bar;
	}
}